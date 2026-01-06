using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web; // لإضافة HttpUtility.UrlEncode
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن التقارير والاستعلامات الخاصة بمرفقات المحامين.
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    public class LawyerAttachmentReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LawyerAttachmentReportsController()
        {
            _context = new ApplicationDbContext();
            // تفعيل ترخيص EPPlusFree
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: LawyerAttachmentReports/Index
        // يعرض صفحة البحث الرئيسية
        [PermissionAuthorizationFilter("عرض صفحة تقارير مرفقات المحامين", "صلاحية الوصول إلى صفحة البحث عن مرفقات المحامين.")]
        [AuditLog("الوصول إلى صفحة تقارير المرفقات", "الوصول إلى صفحة تقارير المرفقات")]
        public ActionResult Index()
        {
            var model = new LawyerAttachmentReportViewModel();
            // تهيئة الأعمدة المتاحة عند تحميل الصفحة لأول مرة
            model.AvailableColumns = new List<LawyerAttachmentReportViewModel.ReportColumn>
            {
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "LawyerFullName", DisplayName = "اسم المحامي", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم الهوية / العضوية", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "FileName", DisplayName = "اسم الملف", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "AttachmentType", DisplayName = "نوع المرفق", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "UploadDate", DisplayName = "تاريخ الرفع", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "FileSize", DisplayName = "حجم الملف (بايت)", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "ContentType", DisplayName = "نوع المحتوى", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "Notes", DisplayName = "ملاحظات", IsSelected = true},
                new LawyerAttachmentReportViewModel.ReportColumn { ColumnName = "FilePath", DisplayName = "رابط المرفق", IsSelected = true} // لعرض الرابط
            };
            return View(model);
        }

        // POST: LawyerAttachmentReports/Search
        // يعالج طلب البحث ويعيد النتائج كجزء جزئي (Partial View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("البحث في تقارير مرفقات المحامين", "صلاحية إجراء البحث عن مرفقات المحامين بناءً على معايير محددة.")]
        [AuditLog("البحث في تقارير المرفقات", "البحث في تقارير المرفقات")]
        public ActionResult Search(LawyerAttachmentReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var results = GetReportData(model);

                var resultViewModel = new LawyerAttachmentReportResultViewModel
                {
                    Reports = results,
                    SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
                };

                return PartialView("_LawyerAttachmentReportResults", resultViewModel);
            }
            return Content("<div class='alert alert-danger text-center mt-5' role='alert'>حدث خطأ في معايير البحث.</div>");
        }

        // POST: LawyerAttachmentReports/ExportToExcel
        // يعالج طلب تصدير النتائج إلى ملف Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تصدير تقارير مرفقات المحامين إلى Excel", "صلاحية تصدير نتائج البحث عن مرفقات المحامين إلى ملف Excel.")]
        [AuditLog("تصدير تقرير المرفقات إلى Excel", "تصدير تقرير المرفقات إلى Excel")]
        public ActionResult ExportToExcel(LawyerAttachmentReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, "بيانات الطلب غير صالحة.");
            }

            try
            {
                var results = GetReportData(model);
                var selectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList();

                if (!results.Any() || !selectedColumns.Any())
                {
                    return Content("لا توجد بيانات للتصدير.");
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("تقرير المرفقات");
                    worksheet.View.RightToLeft = true; // لتعيين اتجاه الورقة من اليمين إلى اليسار

                    // إضافة العناوين
                    for (int i = 0; i < selectedColumns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = selectedColumns[i].DisplayName;
                        // تنسيق رأس العمود
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }
                    // إضافة عنوان لعمود "استعراض في المتصفح" في Excel
                    worksheet.Cells[1, selectedColumns.Count + 1].Value = "استعراض في المتصفح";
                    worksheet.Cells[1, selectedColumns.Count + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, selectedColumns.Count + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, selectedColumns.Count + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[1, selectedColumns.Count + 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                    // إضافة البيانات
                    for (int i = 0; i < results.Count; i++)
                    {
                        var resultItem = results[i];
                        for (int j = 0; j < selectedColumns.Count; j++)
                        {
                            var prop = typeof(LawyerAttachmentReportResultItem).GetProperty(selectedColumns[j].ColumnName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(resultItem, null);
                                // معالجة خاصة للقيم المنطقية (bool?) والتاريخ (DateTime?)
                                if (prop.PropertyType == typeof(bool?))
                                {
                                    bool? boolValue = (bool?)value;
                                    worksheet.Cells[i + 2, j + 1].Value = (boolValue.HasValue && boolValue.Value) ? "نعم" : "لا";
                                }
                                else if (prop.PropertyType == typeof(DateTime?))
                                {
                                    DateTime? dateValue = (DateTime?)value;
                                    worksheet.Cells[i + 2, j + 1].Value = dateValue.HasValue ? dateValue.Value.ToShortDateString() : "غير متوفر";
                                }
                                else if (selectedColumns[j].ColumnName == "FilePath") // معالجة خاصة لرابط الملف
                                {
                                    // بدلاً من عرض المسار الفيزيائي، سنعرض رابطًا إلى إجراء ViewAttachment
                                    string fileUrl = Url.Action("ViewAttachment", "LawyerAttachmentReports", new { attachmentId = resultItem.Id }, Request.Url.Scheme);
                                    worksheet.Cells[i + 2, j + 1].Hyperlink = new Uri(fileUrl);
                                    worksheet.Cells[i + 2, j + 1].Value = resultItem.FileName; // عرض اسم الملف كنص للرابط
                                    worksheet.Cells[i + 2, j + 1].Style.Font.UnderLine = true;
                                    worksheet.Cells[i + 2, j + 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                                }
                                else
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = value?.ToString();
                                }
                            }
                        }
                        // إضافة رابط لعمود "استعراض في المتصفح" في Excel
                        string browseUrl = Url.Action("DisplayAttachment", "LawyerAttachmentReports", new { attachmentId = resultItem.Id }, Request.Url.Scheme);
                        worksheet.Cells[i + 2, selectedColumns.Count + 1].Hyperlink = new Uri(browseUrl);
                        worksheet.Cells[i + 2, selectedColumns.Count + 1].Value = "استعراض";
                        worksheet.Cells[i + 2, selectedColumns.Count + 1].Style.Font.UnderLine = true;
                        worksheet.Cells[i + 2, selectedColumns.Count + 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    }

                    worksheet.Cells.AutoFitColumns(); // ضبط عرض الأعمدة تلقائيًا

                    var fileBytes = package.GetAsByteArray();
                    var fileName = $"تقرير_المرفقات_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    // ترميز اسم الملف لضمان دعمه للأحرف العربية في المتصفحات
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لتسهيل عملية التتبع
                System.Diagnostics.Debug.WriteLine($"Error exporting lawyer attachment report to Excel: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        /// <summary>
        /// استرجاع البيانات من قاعدة البيانات بناءً على معايير البحث المحددة.
        /// </summary>
        /// <param name="model">نموذج العرض الذي يحتوي على معايير البحث.</param>
        /// <returns>قائمة بالنتائج المطابقة.</returns>
        private List<LawyerAttachmentReportResultItem> GetReportData(LawyerAttachmentReportViewModel model)
        {
            // إنشاء الاستعلام الأساسي مع تضمين الكيانات ذات الصلة
            var query = _context.Lawyers
                .Include(l => l.LawyerAttachments) // تضمين المرفقات
                .AsQueryable();

            // تطبيق فلاتر البحث
            if (!string.IsNullOrEmpty(model.LawyerFullName))
            {
                query = query.Where(l => l.FullName.Contains(model.LawyerFullName));
            }

            if (!string.IsNullOrEmpty(model.LawyerIdNumber))
            {
                query = query.Where(l => l.IdNumber == model.LawyerIdNumber);
            }

            if (!string.IsNullOrEmpty(model.FileName))
            {
                query = query.Where(l => l.LawyerAttachments.Any(la => la.FileName.Contains(model.FileName)));
            }

            if (!string.IsNullOrEmpty(model.AttachmentType))
            {
                query = query.Where(l => l.LawyerAttachments.Any(la => la.AttachmentType == model.AttachmentType));
            }

            var filteredLawyers = query.ToList();

            var results = new List<LawyerAttachmentReportResultItem>();
            foreach (var lawyer in filteredLawyers)
            {
                foreach (var attachment in lawyer.LawyerAttachments)
                {
                    results.Add(new LawyerAttachmentReportResultItem
                    {
                        Id = attachment.Id, // تم ملء خاصية Id هنا
                        LawyerIdNumber = lawyer.IdNumber,
                        LawyerFullName = lawyer.FullName,
                        FileName = attachment.FileName,
                        FilePath = attachment.FilePath, // هذا هو المسار الفيزيائي للملف
                        FileSize = attachment.FileSize,
                        ContentType = attachment.ContentType,
                        AttachmentType = attachment.AttachmentType,
                        Notes = attachment.Notes,
                        UploadDate = attachment.UploadDate
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// إجراء لعرض المرفق بناءً على معرف المرفق.
        /// يقوم بقراءة الملف من المسار الفيزيائي وإرجاعه كـ FileResult.
        /// </summary>
        /// <param name="attachmentId">معرف المرفق.</param>
        /// <returns>FileResult للمرفق.</returns>
        [HttpGet]
        [PermissionAuthorizationFilter("عرض مرفق محامٍ", "صلاحية عرض مرفق محامٍ بشكل مباشر في المتصفح.")]
        [AuditLog("عرض مرفق", "عرض مرفق")]
        public ActionResult ViewAttachment(int attachmentId)
        {
            try
            {
                var attachment = _context.LawyerAttachments.Find(attachmentId);

                if (attachment == null)
                {
                    return HttpNotFound("المرفق غير موجود.");
                }

                string filePath = attachment.FilePath;

                // التعامل مع المسارات التي تبدأ بـ "file:///" أو مسارات محرك الأقراص
                if (filePath.StartsWith("file:///"))
                {
                    filePath = new Uri(filePath).LocalPath; // يحول file:///E:/... إلى E:\...
                }

                // التحقق من وجود الملف في المسار المطلق أولاً
                if (!System.IO.File.Exists(filePath))
                {
                    // إذا لم يتم العثور عليه، جرب بناء المسار بافتراض أنه نسبي من مجلد App_Data/Uploads/LawyerAttachments/
                    // هذا المسار يجب أن يتطابق مع مكان تخزين الملفات الفعلي لديك
                    string appDataUploadsPath = Server.MapPath("~/App_Data/Uploads/LawyerAttachments/");
                    string potentialFullPath = Path.Combine(appDataUploadsPath, Path.GetFileName(filePath));

                    if (System.IO.File.Exists(potentialFullPath))
                    {
                        filePath = potentialFullPath;
                    }
                    else
                    {
                        // إذا لم يتم العثور على الملف في أي من المسارين، أرجع خطأ
                        return HttpNotFound("الملف غير موجود على الخادم.");
                    }
                }

                // إرجاع الملف كـ FileResult بدون تحديد اسم ملف للتنزيل
                // هذا يسمح للمتصفح بعرضه مباشرة إذا كان يدعم نوع المحتوى
                return File(filePath, attachment.ContentType); // تم إزالة attachment.FileName هنا
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error viewing attachment: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ أثناء محاولة عرض المرفق.");
            }
        }

        /// <summary>
        /// إجراء لعرض المرفق في صفحة تفاصيل مخصصة.
        /// </summary>
        /// <param name="attachmentId">معرف المرفق.</param>
        /// <returns>ViewResult لصفحة عرض المرفق.</returns>
        [HttpGet]
        [PermissionAuthorizationFilter("عرض مرفق محامٍ في صفحة مخصصة", "صلاحية عرض مرفق محامٍ في صفحة تفاصيل مخصصة.")]
        [AuditLog("عرض صفحة المرفق", "عرض صفحة المرفق")]
        public ActionResult DisplayAttachment(int attachmentId)
        {
            try
            {
                var attachment = _context.LawyerAttachments.Find(attachmentId);

                if (attachment == null)
                {
                    return HttpNotFound("المرفق غير موجود.");
                }

                // يمكنك تمرير نموذج مخصص لصفحة العرض إذا احتجت لبيانات إضافية
                // حالياً، سنمرر كائن المرفق مباشرة
                return View("DisplayAttachment", attachment);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ الذي يحدث أثناء محاولة عرض الـ View
                System.Diagnostics.Debug.WriteLine($"Error rendering DisplayAttachment view for attachment ID {attachmentId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء عرض المرفق.");
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
