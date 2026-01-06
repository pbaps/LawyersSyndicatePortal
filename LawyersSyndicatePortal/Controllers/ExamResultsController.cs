using System;
using System.Linq;
using System.Web.Mvc;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Models;
using OfficeOpenXml;
using System.IO;
using System.Net;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using LawyersSyndicatePortal.Filters;
using System.Threading.Tasks;
using LawyersSyndicatePortal.Utilities;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم (Controller) المسؤول عن إدارة وعرض نتائج الامتحانات.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    public class ExamResultsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ExamResults
        /// <summary>
        /// يعرض قائمة بنتائج الامتحانات الخاصة بالمحامين المرتبطين بالمستخدم الحالي.
        /// </summary>
        [PermissionAuthorizationFilter("عرض نتائج الامتحانات", "صلاحية عرض نتائج الامتحانات")]
        [AuditLog("عرض نتائج الامتحانات", "عرض نتائج الامتحانات")]
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var examResults = await db.ExamResults
                                      .Where(r => r.ExamAttendee.Lawyer.LinkedUsers.Any(u => u.Id == userId))
                                      .OrderByDescending(r => r.ExamAttendee.StartTime)
                                      .Include(r => r.Exam)
                                      .Include(r => r.ExamAttendee.Lawyer)
                                      .ToListAsync();

            // العودة إلى العرض المحدد باسم "ExamResult.cshtml" مع قائمة النتائج
            return View("ExamResult", examResults);
        }

        // GET: ExamResults/Details/5
        /// <summary>
        /// يعرض تفاصيل نتيجة اختبار معينة بناءً على المعرف (ID).
        /// </summary>
        [PermissionAuthorizationFilter("عرض تفاصيل نتيجة اختبار", "صلاحية عرض تفاصيل نتيجة اختبار معينة")]
        [AuditLog("عرض تفاصيل نتيجة اختبار", "عرض تفاصيل نتيجة اختبار")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var examResult = await db.ExamResults
                                     .Include(er => er.ExamAttendee.Answers.Select(a => a.Question))
                                     .Include(er => er.Exam)
                                     .Include(er => er.Lawyer)
                                     .FirstOrDefaultAsync(r => r.ExamAttendeeId == id);

            if (examResult == null)
            {
                return HttpNotFound();
            }

            return View(examResult);
        }

        // POST: ExamResults/GradeTextQuestion
        /// <summary>
        /// يقوم بتصحيح سؤال نصي يدويًا وتحديث الدرجة المحصلة.
        /// </summary>
        /// <param name="answerId">معرف الإجابة المراد تصحيحها.</param>
        /// <param name="questionScore">الدرجة التي حصل عليها الطالب.</param>
        /// <returns>نتيجة JSON تشير إلى نجاح أو فشل العملية.</returns>
        [HttpPost]
        [PermissionAuthorizationFilter("تصحيح سؤال نصي", "صلاحية تصحيح أسئلة الاختبارات اليدوية")]
        [AuditLog("تصحيح سؤال نصي", "تصحيح سؤال نصي")]
        public async Task<ActionResult> GradeTextQuestion(int answerId, decimal questionScore)
        {
            try
            {
                var answer = await db.Answers.FirstOrDefaultAsync(a => a.Id == answerId);

                if (answer == null)
                {
                    return Json(new { success = false, message = "الإجابة غير موجودة." });
                }

                answer.AcquiredScore = questionScore;
                answer.IsGraded = true;
                answer.CorrectionDate = DateTime.Now;
                answer.CorrectorId = User.Identity.GetUserId();
                await db.SaveChangesAsync();

                var examResult = await db.ExamResults
                                         .Include(er => er.ExamAttendee.Answers)
                                         .FirstOrDefaultAsync(er => er.ExamAttendeeId == answer.ExamAttendeeId);

                if (examResult != null)
                {
                    examResult.TotalScoreAchieved = examResult.ExamAttendee.Answers.Sum(a => a.AcquiredScore.GetValueOrDefault());

                    // التحقق مما إذا تم تصحيح جميع الأسئلة اليدوية
                    bool allQuestionsGraded = examResult.ExamAttendee.Answers
                                                         .Where(a => a.Question.RequiresManualGrading)
                                                         .All(a => a.IsGraded);

                    examResult.IsGradingComplete = allQuestionsGraded;
                    await db.SaveChangesAsync();
                }

                return Json(new { success = true, message = "تم تحديث الدرجة بنجاح." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"حدث خطأ أثناء تحديث الدرجة: {ex.Message}" });
            }
        }

        // GET: ExamResults/ExportToExcel
        /// <summary>
        /// يقوم بتصدير جميع نتائج الامتحانات إلى ملف Excel.
        /// </summary>
        [PermissionAuthorizationFilter("تصدير نتائج الاختبارات", "صلاحية تصدير نتائج الاختبارات إلى ملف Excel")]
        [AuditLog("تصدير نتائج الاختبارات إلى Excel", "تصدير نتائج الاختبارات إلى Excel")]
        public async Task<ActionResult> ExportToExcel()
        {
            var exportData = await db.ExamResults
                                     .Include(r => r.Exam)
                                     .Include(r => r.Lawyer)
                                     .Select(r => new ExamResultsExportViewModel
                                     {
                                         ExamTitle = r.Exam.Title,
                                         LawyerIdNumber = r.LawyerIdNumber,
                                         LawyerName = r.Lawyer.FullName,
                                         CorrectAnswersCount = (int)r.TotalScoreAchieved,
                                         TotalQuestionsCount = (int)r.TotalExamScore,
                                         Score = (double)r.TotalScoreAchieved / (double)r.TotalExamScore * 100
                                     }).ToListAsync();

            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Exam Results");
                worksheet.Cells["A1"].Value = "عنوان الامتحان";
                worksheet.Cells["B1"].Value = "رقم هوية المحامي";
                worksheet.Cells["C1"].Value = "اسم المحامي";
                worksheet.Cells["D1"].Value = "الدرجة التي حصل عليها";
                worksheet.Cells["E1"].Value = "الدرجة الكلية للاختبار";
                worksheet.Cells["F1"].Value = "النسبة المئوية للنتيجة";
                worksheet.Cells["A2"].LoadFromCollection(exportData);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:F1"].Style.Font.Bold = true;

                var stream = new MemoryStream();
                excelPackage.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ExamResults-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        /// <summary>
        /// تنظيف الموارد المستخدمة من قبل المتحكم.
        /// </summary>
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
