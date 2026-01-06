using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using ClosedXML.Excel;
using System.IO;
using LawyersSyndicatePortal.Filters;

// يجب تثبيت حزمة ClosedXML عبر NuGet:
// Install-Package ClosedXML

namespace LawyersSyndicatePortal.Controllers
{

    // ViewModels لمساعدتنا على تمرير البيانات إلى View
    [Authorize(Roles = "Admin")]
    public class AdminResultViewModel
    {
        public List<Exam> ManualGradingExams { get; set; }
        public List<Exam> CompletedGradingExams { get; set; }
    }

    // ViewModel جديد لعملية الحفظ
    public class ManualGradingViewModel
    {
        public int ExamAttendeeId { get; set; }
        public List<QuestionGrade> Grades { get; set; }
    }

    // ViewModel فرعي لدرجة كل سؤال
    public class QuestionGrade
    {
        public int AnswerId { get; set; }
        public decimal AcquiredScore { get; set; }
    }

    // تم إزالة مرشح AuditLog من مستوى الفئة
    public class AdminResultController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: AdminResult
        // يعرض الاختبارات التي تحتاج إلى تصحيح يدوي وتلك المكتملة
        [PermissionAuthorizationFilter("عرض نتائج الاختبارات", "صلاحية عرض نتائج الاختبارات التي تحتاج تصحيحا يدويا")]
        [AuditLog("عرض", "عرض نتائج الاختبارات التي تحتاج تصحيحا يدويا")]
        public async Task<ActionResult> Index()
        {
            // الحصول على جميع نتائج الاختبارات
            var examResults = await _db.ExamResults
                                         .Include(er => er.Exam)
                                         .Include(er => er.ExamAttendee)
                                         .ToListAsync();

            // معرفات الاختبارات التي تحتاج إلى تصحيح يدوي (أي أن التصحيح لم يكتمل بعد)
            var manualGradingExamIds = examResults
                .Where(er => !er.IsGradingComplete)
                .Select(er => er.ExamId)
                .Distinct()
                .ToList();

            // معرفات الاختبارات التي اكتمل تصحيحها
            var completedGradingExamIds = examResults
                .Where(er => er.IsGradingComplete)
                .Select(er => er.ExamId)
                .Distinct()
                .ToList();

            var viewModel = new AdminResultViewModel
            {
                ManualGradingExams = await _db.Exams.Where(e => manualGradingExamIds.Contains(e.Id)).ToListAsync(),
                CompletedGradingExams = await _db.Exams.Where(e => completedGradingExamIds.Contains(e.Id)).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: AdminResult/AttendeesNeedingGrading/5
        // يعرض قائمة الملتحقين الذين يحتاجون إلى تصحيح يدوي لاختبار معين
        [PermissionAuthorizationFilter("عرض الملتحقين للتصحيح اليدوي", "صلاحية عرض قائمة الملتحقين الذين يحتاجون تصحيحا يدويا")]
        [AuditLog("عرض", "عرض قائمة الملتحقين الذين يحتاجون تصحيحا يدويا")]
        public async Task<ActionResult> AttendeesNeedingGrading(int examId)
        {
            var exam = await _db.Exams.FindAsync(examId);
            if (exam == null)
            {
                return HttpNotFound();
            }

            // جلب الملتحقين الذين لم يكتمل تصحيحهم بعد لهذا الاختبار
            var attendees = await _db.ExamAttendees
                                   .Include(ea => ea.ExamResult)
                                   .Where(ea => ea.ExamId == examId && (ea.ExamResult == null || !ea.ExamResult.IsGradingComplete))
                                   .ToListAsync();

            ViewBag.ExamTitle = exam.Title;
            return View(attendees);
        }

        // GET: AdminResult/ManualGrading/5
        // يعرض الأسئلة التي تحتاج إلى تصحيح يدوي لملتحق معين
        [PermissionAuthorizationFilter("تصحيح أسئلة يدويا", "صلاحية تصحيح أسئلة اختبار يدويا")]
        [AuditLog("تصحيح", "تصحيح أسئلة اختبار يدويا")]
        public async Task<ActionResult> ManualGrading(int examAttendeeId)
        {
            // Include related data to avoid NullReferenceException
            var examAttendee = await _db.ExamAttendees
                                         .Include(ea => ea.Exam)
                                         .Include(ea => ea.Lawyer)
                                         .Include(ea => ea.Answers.Select(a => a.Question))
                                         .FirstOrDefaultAsync(ea => ea.Id == examAttendeeId);

            if (examAttendee == null)
            {
                return HttpNotFound();
            }

            // تصفية الإجابات التي تتطلب تصحيحاً يدوياً ولم يتم تصحيحها بعد
            // استخدام الشرط الصفري (Null-Conditional Operator) ?. لضمان عدم وجود أخطاء في حالة كانت البيانات فارغة
            var answersToGrade = examAttendee.Answers.Where(a => a.Question?.RequiresManualGrading ?? false && !a.IsGraded).ToList();

            ViewBag.ExamAttendee = examAttendee;
            return View(answersToGrade);
        }

        // POST: AdminResult/SaveManualGrade
        // يحفظ الدرجة التي رصدها الأدمن للأسئلة اليدوية
        [PermissionAuthorizationFilter("حفظ درجات التصحيح اليدوي", "صلاحية حفظ درجات الأسئلة التي تم تصحيحها يدويا")]
        [AuditLog("حفظ", "حفظ درجات الأسئلة التي تم تصحيحها يدويا")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveManualGrade(ManualGradingViewModel model)
        {
            try
            {
                var examAttendee = await _db.ExamAttendees
                                                 .Include(ea => ea.Answers.Select(a => a.Question))
                                                 .Include(ea => ea.ExamResult)
                                                 .Include(ea => ea.Exam)
                                                 .FirstOrDefaultAsync(ea => ea.Id == model.ExamAttendeeId);

                if (examAttendee == null)
                {
                    TempData["ErrorMessage"] = "حدث خطأ: لم يتم العثور على بيانات الملتحق.";
                    return RedirectToAction("ManualGrading", new { examAttendeeId = model.ExamAttendeeId });
                }

                // Update answers and recalculate total score
                foreach (var grade in model.Grades)
                {
                    var answer = examAttendee.Answers.FirstOrDefault(a => a.Id == grade.AnswerId);
                    if (answer != null)
                    {
                        answer.AcquiredScore = grade.AcquiredScore;
                        answer.IsGraded = true;
                        answer.CorrectionDate = DateTime.Now;
                        // يمكنك إضافة CorrectorId هنا بناءً على المستخدم الذي قام بتسجيل الدخول
                    }
                }

                // Recalculate total score
                var totalAchieved = examAttendee.Answers.Sum(a => a.AcquiredScore ?? 0);
                var totalExamScore = examAttendee.Answers.Sum(a => a.Question?.Score ?? 0);

                // Update ExamResult
                if (examAttendee.ExamResult == null)
                {
                    // إذا لم يكن هناك ExamResult، قم بإنشاء واحد جديد
                    examAttendee.ExamResult = new ExamResult();
                    _db.ExamResults.Add(examAttendee.ExamResult);
                }

                examAttendee.ExamResult.TotalScoreAchieved = totalAchieved;
                examAttendee.ExamResult.TotalExamScore = totalExamScore;
                examAttendee.ExamResult.PassPercentage = (totalExamScore > 0) ? (totalAchieved / totalExamScore) * 100 : 0;

                if (examAttendee.Exam != null)
                {
                    examAttendee.ExamResult.IsPassed = (examAttendee.ExamResult.PassPercentage >= examAttendee.Exam.PassingScorePercentage);
                }
                else
                {
                    examAttendee.ExamResult.IsPassed = false; // تعيين قيمة افتراضية في حالة عدم وجود كائن Exam
                }

                // Check if grading is complete
                // هذا السطر يضمن عدم حدوث خطأ إذا كان هناك سؤال فارغ
                var manualQuestionsCount = examAttendee.Answers.Count(a => a.Question?.RequiresManualGrading ?? false);
                var gradedAnswersCount = examAttendee.Answers.Count(a => a.IsGraded);

                if (manualQuestionsCount == gradedAnswersCount)
                {
                    examAttendee.ExamResult.IsGradingComplete = true;
                }

                // Save changes to the database
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم حفظ الدرجات بنجاح.";

                // Redirect to the same page to show the updated state
                return RedirectToAction("ManualGrading", new { examAttendeeId = model.ExamAttendeeId });
            }
            catch (Exception ex)
            {
                // Catch any exception and pass a user-friendly error message
                TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ الدرجات: " + ex.Message;
                // يمكنك تسجيل الخطأ بالكامل هنا لأغراض تصحيح الأخطاء
                return RedirectToAction("ManualGrading", new { examAttendeeId = model.ExamAttendeeId });
            }
        }


        // GET: AdminResult/ExportToExcel/5
        // يقوم بتصدير نتائج اختبار معين إلى ملف إكسل
        [PermissionAuthorizationFilter("تصدير نتائج الاختبارات إلى إكسل", "صلاحية تصدير نتائج اختبار إلى ملف إكسل")]
        [AuditLog("تصدير", "تصدير نتائج اختبار إلى ملف إكسل")]
        public async Task<ActionResult> ExportToExcel(int examId)
        {
            var exam = await _db.Exams
                                 .Include(e => e.ExamAttendees.Select(ea => ea.ExamResult))
                                 .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                return HttpNotFound();
            }

            var results = exam.ExamAttendees.Select(ea => new
            {
                Lawyer = ea.Lawyer?.FullName ?? ea.LawyerIdNumber, // جلب اسم المحامي الحقيقي إذا كان موجوداً
                IdNumber = ea.LawyerIdNumber,
                MobileNumber = ea.MobileNumber,
                TotalExamScore = ea.ExamResult?.TotalExamScore ?? 0,
                TotalScoreAchieved = ea.ExamResult?.TotalScoreAchieved ?? 0,
                PassPercentage = ea.ExamResult?.PassPercentage ?? 0,
                IsPassed = ea.ExamResult?.IsPassed ?? false
            }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("نتائج الاختبار");

                // Headers in Arabic
                worksheet.Cell(1, 1).Value = "اسم المحامي";
                worksheet.Cell(1, 2).Value = "رقم الهوية";
                worksheet.Cell(1, 3).Value = "رقم الجوال";
                worksheet.Cell(1, 4).Value = "الدرجة القصوى";
                worksheet.Cell(1, 5).Value = "الدرجة التي حصل عليها";
                worksheet.Cell(1, 6).Value = "نسبة النجاح";
                worksheet.Cell(1, 7).Value = "حالة النجاح";

                // Data
                int row = 2;
                foreach (var result in results)
                {
                    worksheet.Cell(row, 1).Value = result.Lawyer;
                    worksheet.Cell(row, 2).Value = result.IdNumber;
                    worksheet.Cell(row, 3).Value = result.MobileNumber;
                    worksheet.Cell(row, 4).Value = result.TotalExamScore;
                    worksheet.Cell(row, 5).Value = result.TotalScoreAchieved;
                    worksheet.Cell(row, 6).Value = result.PassPercentage;
                    worksheet.Cell(row, 7).Value = result.IsPassed ? "ناجح" : "راسب";
                    row++;
                }

                worksheet.RightToLeft = true; // Set sheet direction to right-to-left

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"نتائج-اختبار-{exam.Title}.xlsx");
            }
        }
    }
}
