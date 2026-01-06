using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.Controllers
{
    // المتحكم الخاص بتصحيح الامتحانات
    [Authorize(Roles = "Admin")]
    public class GradingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Grading
        // يعرض قائمة بالامتحانات التي تحتاج إلى تصحيح يدوي
        [PermissionAuthorizationFilter("عرض قائمة الامتحانات للتصحيح", "صلاحية عرض قائمة الامتحانات التي تحتاج إلى تصحيح يدوي.")]
        [AuditLog("إدارة الاجابات والتصحيح", "إدارة الاجابات والتصحيح")]
        public async Task<ActionResult> Index()
        {
            // تم تعديل هذا السطر لمقارنة QuestionType مع قيمة التعداد (enum) الصحيحة
            var examsToGrade = await db.Exams.Where(e => e.Questions.Any(q => q.QuestionType != QuestionType.MultipleChoice && e.ExamAttendees.Any(ea => ea.IsCompleted)))
                                             .ToListAsync();

            return View(examsToGrade);
        }

        // GET: Grading/GradeExam/5
        // يعرض قائمة بجميع الإجابات التي تحتاج إلى تصحيح لامتحان معين
        [PermissionAuthorizationFilter("عرض إجابات امتحان للتصحيح", "صلاحية عرض إجابات امتحان معين بهدف التصحيح اليدوي.")]
        [AuditLog("عرض الاجابات للتصحيح", "عرض الاجابات والتصحيح")]
        public async Task<ActionResult> GradeExam(int examId)
        {
            var answersToGrade = await db.Answers
                                             .Include(a => a.Question)
                                             .Include(a => a.ExamAttendee)
                                             .Where(a => a.ExamAttendee.ExamId == examId && a.IsGraded == false)
                                             .ToListAsync();

            ViewBag.ExamTitle = (await db.Exams.FindAsync(examId))?.Title;
            return View(answersToGrade);
        }

        // POST: Grading/SubmitGrade
        // يستقبل الدرجة التي تم تعيينها من المصحح ويحدث الإجابة
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدخال درجة تصحيح", "صلاحية إدخال درجة تصحيح لإجابة امتحان.")]
        [AuditLog("ادراج علامة تصحيح اختبار", "ادراج علامة تصحيح اختبار")]
        public async Task<ActionResult> SubmitGrade(int answerId, decimal score)
        {
            var answer = await db.Answers.FindAsync(answerId);
            if (answer == null)
            {
                return HttpNotFound();
            }

            // التحقق من أن الدرجة لا تتجاوز الدرجة القصوى للسؤال
            var maxScore = db.Questions.Find(answer.QuestionId).Score;
            if (score > maxScore)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"The score cannot exceed the maximum score of {maxScore}.");
            }

            answer.AcquiredScore = score;
            answer.IsGraded = true;
            await db.SaveChangesAsync();

            // تحديث إجمالي الدرجات في ExamResult
            var examAttendee = await db.ExamAttendees.FindAsync(answer.ExamAttendeeId);
            var examResult = await db.ExamResults.SingleOrDefaultAsync(er => er.ExamAttendeeId == examAttendee.Id);

            if (examResult != null)
            {
                examResult.TotalScoreAchieved += score;
                // التحقق مما إذا كان التصحيح قد اكتمل بالكامل
                if (!await db.Answers.AnyAsync(a => a.ExamAttendeeId == examAttendee.Id && a.IsGraded == false))
                {
                    examResult.IsGradingComplete = true;
                }
                await db.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
