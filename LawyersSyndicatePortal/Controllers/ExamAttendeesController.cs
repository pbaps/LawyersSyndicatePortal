using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using System.Web;
using System.Text;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    // المتحكم الخاص بإدارة الملتحقين بالامتحانات (من جانب الإدارة)
    [Authorize(Roles = "Admin")]
    public class ExamAttendeesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ExamAttendees/Index?examId=5
        // يعرض قائمة الملتحقين لامتحان محدد.
        // تم التعديل على صيغة Attributes هنا لتطابق الأسلوب المطلوب
        [PermissionAuthorizationFilter("عرض الملتحقين", "صلاحية عرض قائمة الملتحقين بالاختبار")]
        [AuditLog("عرض", "عرض قائمة الملتحقين بالاختبار")]
        public async Task<ActionResult> Index(int? examId)
        {
            if (examId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var exam = await db.Exams.FindAsync(examId);
            if (exam == null)
            {
                return HttpNotFound();
            }

            // تم تعديل هذا الجزء: يجب إضافة .Include(e => e.Lawyer) لتحميل بيانات المحامي
            // حتى يتم عرض الاسم الكامل في الجدول
            var examAttendees = db.ExamAttendees
                .Include(e => e.Exam)
                .Include(e => e.Lawyer)
                .Where(e => e.ExamId == examId);

            ViewBag.Exam = exam;
            return View(await examAttendees.ToListAsync());
        }

        // POST: ExamAttendees/Import
        // يستورد بيانات الملتحقين من ملف CSV.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("استيراد الملتحقين", "صلاحية استيراد قائمة الملتحقين بالاختبار")]
        [AuditLog("استيراد", "استيراد ملتحقين من ملف CSV")]
        public async Task<ActionResult> Import(int examId, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    using (var reader = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        // تم تجاوز السطر الأول (رؤوس الأعمدة)
                        await reader.ReadLineAsync();

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            // التعديل هنا: استخدام الفاصلة المنقوطة ';' بدلاً من الفاصلة العادية ','
                            var parts = line.Split(';');

                            var lawyerIdNumber = parts.Length > 0 ? parts[0].Trim() : null;
                            var mobileNumber = parts.Length > 1 ? parts[1].Trim() : null;

                            if (!string.IsNullOrEmpty(lawyerIdNumber))
                            {
                                var lawyer = await db.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);
                                if (lawyer != null)
                                {
                                    var existingAttendee = await db.ExamAttendees.FirstOrDefaultAsync(
                                        ea => ea.ExamId == examId && ea.LawyerIdNumber == lawyerIdNumber);
                                    if (existingAttendee == null)
                                    {
                                        db.ExamAttendees.Add(new ExamAttendee
                                        {
                                            ExamId = examId,
                                            LawyerIdNumber = lawyerIdNumber,
                                            MobileNumber = mobileNumber,
                                            CanAttend = true,
                                            IsExamVisible = true
                                        });
                                    }
                                }
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم استيراد الملتحقين بنجاح.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "حدث خطأ أثناء الاستيراد: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "الرجاء تحديد ملف صالح.";
            }

            return RedirectToAction("Index", new { examId });
        }


        // GET: ExamAttendees/DownloadImportTemplate
        // يقوم بتوليد ملف CSV فارغ كنموذج للاستيراد.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("تنزيل نموذج استيراد", "صلاحية تنزيل نموذج استيراد الملتحقين")]
        [AuditLog("تنزيل", "تنزيل نموذج استيراد الملتحقين")]
        public ActionResult DownloadImportTemplate()
        {
            var fileName = "ImportTemplate.csv";
            var content = new StringBuilder();
            content.AppendLine("LawyerIdNumber;MobileNumber");
            var bytes = Encoding.UTF8.GetBytes(content.ToString());
            var memoryStream = new MemoryStream(bytes);
            return File(memoryStream, "text/csv", fileName);
        }

        // GET: ExamAttendees/Export
        // يقوم بتصدير بيانات الملتحقين إلى ملف CSV.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("تصدير تقارير الزملاء", "صلاحية تصدير تقارير الزملاء إلى Excel")]
        [AuditLog("تصدير", "تصدير تقارير الزملاء إلى Excel")]
        public async Task<ActionResult> Export(int? examId)
        {
            if (examId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var attendees = await db.ExamAttendees
                .Where(ea => ea.ExamId == examId)
                .Select(ea => new ExamAttendeeExportModel
                {
                    ExamTitle = ea.Exam.Title,
                    LawyerIdNumber = ea.LawyerIdNumber,
                    LawyerName = ea.Lawyer.FullName,
                    LawyerMobile = ea.MobileNumber,
                    CanAttend = ea.CanAttend,
                    IsExamVisible = ea.IsExamVisible
                })
                .ToListAsync();

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                writer.WriteLine("\"عنوان الامتحان\";\"رقم هوية المحامي\";\"اسم المحامي\";\"رقم الجوال\";\"هل يحق له الالتحاق؟\";\"هل الاختبار مرئي له؟\"");
                foreach (var attendee in attendees)
                {
                    var line = $"\"{attendee.ExamTitle}\";\"{attendee.LawyerIdNumber}\";\"{attendee.LawyerName}\";\"{attendee.LawyerMobile}\";\"{attendee.CanAttend}\";\"{attendee.IsExamVisible}\"";
                    writer.WriteLine(line);
                }
                writer.Flush();
                memoryStream.Position = 0;
                var fileName = $"ExamAttendees_Exam_{examId}.csv";
                return File(memoryStream.ToArray(), "text/csv", fileName);
            }
        }

        // GET: ExamAttendees/Create?examId=5
        // يعرض صفحة لإنشاء ملتحق جديد بالامتحان.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("إنشاء ملتحق", "صلاحية إنشاء ملتحق جديد بالاختبار")]
        [AuditLog("إنشاء", "الدخول إلى صفحة إضافة ملتحق جديد")]
        public ActionResult Create(int examId)
        {
            ViewBag.ExamId = examId;
            return View();
        }

        // POST: ExamAttendees/Create
        // يستقبل بيانات الملتحق الجديد ويضيفها إلى قاعدة البيانات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("إنشاء ملتحق", "صلاحية إنشاء ملتحق جديد بالاختبار")]
        [AuditLog("إنشاء", "إنشاء ملتحق جديد بالاختبار")]
        public async Task<ActionResult> Create([Bind(Include = "ExamId,LawyerIdNumber,MobileNumber,CanAttend,IsExamVisible")] ExamAttendee examAttendee)
        {
            if (ModelState.IsValid)
            {
                var lawyerExists = await db.Lawyers.AnyAsync(l => l.IdNumber == examAttendee.LawyerIdNumber);
                if (!lawyerExists)
                {
                    ModelState.AddModelError("LawyerIdNumber", "رقم المحامي غير موجود.");
                    return View(examAttendee);
                }

                db.ExamAttendees.Add(examAttendee);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { examId = examAttendee.ExamId });
            }
            return View(examAttendee);
        }

        // GET: ExamAttendees/SearchLawyers
        // يبحث عن المحامين بناءً على رقم الهوية أو الاسم.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("البحث عن محامين", "صلاحية البحث عن محامين")]
        [AuditLog("بحث", "البحث عن محامين بناءً على الاستعلام")]
        public async Task<ActionResult> SearchLawyers(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
            var lawyers = await db.Lawyers
                .Where(l => l.IdNumber.Contains(query) || l.FullName.Contains(query))
                .Select(l => new { idNumber = l.IdNumber, name = l.FullName })
                .ToListAsync();
            return Json(lawyers, JsonRequestBehavior.AllowGet);
        }

        // GET: ExamAttendees/Details/5
        // يعرض تفاصيل ملتحق معين بالامتحان.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("عرض تفاصيل الملتحق", "صلاحية عرض تفاصيل ملتحق معين")]
        [AuditLog("عرض", "عرض تفاصيل ملتحق معين")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamAttendee examAttendee = await db.ExamAttendees.Include(e => e.Exam).Include(e => e.Lawyer).FirstOrDefaultAsync(e => e.Id == id); // Added .Include(e => e.Lawyer)
            if (examAttendee == null)
            {
                return HttpNotFound();
            }
            return View(examAttendee);
        }

        // GET: ExamAttendees/Edit/5
        // يعرض صفحة لتعديل بيانات ملتحق موجود.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("تعديل بيانات ملتحق", "صلاحية تعديل بيانات ملتحق بالاختبار")]
        [AuditLog("تعديل", "الدخول إلى صفحة تعديل ملتحق")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamAttendee examAttendee = await db.ExamAttendees.FindAsync(id);
            if (examAttendee == null)
            {
                return HttpNotFound();
            }
            return View(examAttendee);
        }

        // POST: ExamAttendees/Edit/5
        // يستقبل البيانات المعدلة للملتحق ويحدثها في قاعدة البيانات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("تعديل بيانات ملتحق", "صلاحية تعديل بيانات ملتحق بالاختبار")]
        [AuditLog("تعديل", "تعديل بيانات ملتحق بالاختبار")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ExamId,LawyerIdNumber,MobileNumber,CanAttend,IsCompleted,StartTime,EndTime,IsExamVisible")] ExamAttendee examAttendee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(examAttendee).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { examId = examAttendee.ExamId });
            }
            return View(examAttendee);
        }

        // GET: ExamAttendees/Delete/5
        // يعرض صفحة لتأكيد حذف ملتحق.
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("حذف ملتحق", "صلاحية حذف ملتحق من الاختبار")]
        [AuditLog("حذف", "الدخول إلى صفحة تأكيد حذف ملتحق")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamAttendee examAttendee = await db.ExamAttendees.Include(e => e.Exam).FirstOrDefaultAsync(e => e.Id == id);
            if (examAttendee == null)
            {
                return HttpNotFound();
            }
            return View(examAttendee);
        }

        // POST: ExamAttendees/Delete/5
        // يحذف الملتحق من قاعدة البيانات.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // تم التعديل على صيغة Attributes هنا
        [PermissionAuthorizationFilter("حذف ملتحق", "صلاحية حذف ملتحق من الاختبار")]
        [AuditLog("حذف", "حذف ملتحق من الاختبار")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ExamAttendee examAttendee = await db.ExamAttendees.FindAsync(id);
            var examId = examAttendee.ExamId;
            db.ExamAttendees.Remove(examAttendee);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { examId });
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
