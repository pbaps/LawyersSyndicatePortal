// LawyersSyndicatePortal/Controllers/NewDetentionReportsController.cs
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
    /// المتحكم المسؤول عن التقارير والاستعلامات الخاصة بتفاصيل الاعتقال للمحامين.
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    // السماح للمسؤولين فقط بالوصول إلى هذا المتحكم
    [Authorize(Roles = "Admin")]
    public class NewDetentionReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewDetentionReportsController()
        {
            _context = new ApplicationDbContext();
            // تفعيل ترخيص EPPlusFree
            // تم إزالة التعليق عن السطر التالي لحل مشكلة التصدير
         ///   ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: NewDetentionReports/Index
        // يعرض صفحة البحث الرئيسية
        public ActionResult Index()
        {
            var model = new NewDetentionReportViewModel();
            // تهيئة الأعمدة المتاحة عند تحميل الصفحة لأول مرة
            model.AvailableColumns = new List<NewDetentionReportViewModel.ReportColumn>
            {
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "LawyerFullName", DisplayName = "اسم المحامي", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم الهوية / العضوية", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "WasDetained", DisplayName = "هل تعرض للاعتقال؟", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "DetentionDuration", DisplayName = "مدة الاعتقال", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "DetentionStartDate", DisplayName = "تاريخ بدء الاعتقال", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "IsStillDetained", DisplayName = "هل ما زال معتقلاً؟", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "ReleaseDate", DisplayName = "تاريخ الإفراج", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "DetentionType", DisplayName = "نوع الاعتقال", IsSelected = true},
                new NewDetentionReportViewModel.ReportColumn { ColumnName = "DetentionLocation", DisplayName = "مكان الاعتقال", IsSelected = true}
            };
            return View(model);
        }

        // POST: NewDetentionReports/Search
        // يعالج طلب البحث ويعيد النتائج كجزء جزئي (Partial View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(NewDetentionReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var results = GetReportData(model);

                var resultViewModel = new NewDetentionReportResultViewModel
                {
                    Reports = results,
                    SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
                };

                return PartialView("_NewDetentionReportResults", resultViewModel);
            }
            return Content("<div class='alert alert-danger text-center mt-5' role='alert'>حدث خطأ في معايير البحث.</div>");
        }

        // POST: NewDetentionReports/ExportToExcel
        // يعالج طلب تصدير النتائج إلى ملف Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportToExcel(NewDetentionReportViewModel model)
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
                    var worksheet = package.Workbook.Worksheets.Add("تقرير تفاصيل الاعتقال");
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

                    // إضافة البيانات
                    for (int i = 0; i < results.Count; i++)
                    {
                        var resultItem = results[i];
                        for (int j = 0; j < selectedColumns.Count; j++)
                        {
                            var prop = typeof(NewDetentionReportResultItem).GetProperty(selectedColumns[j].ColumnName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(resultItem, null);
                                // استخدام نفس الأسلوب العام للتنسيق مثل OfficeAndDamageReportsController
                                // معالجة خاصة للقيم المنطقية (bool) والتاريخ (DateTime?)
                                if (prop.PropertyType == typeof(bool))
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = (bool)value ? "نعم" : "لا";
                                }
                                else if (prop.PropertyType == typeof(bool?))
                                {
                                    bool? boolValue = (bool?)value;
                                    worksheet.Cells[i + 2, j + 1].Value = (boolValue.HasValue && boolValue.Value) ? "نعم" : "لا";
                                }
                                else if (prop.PropertyType == typeof(DateTime?))
                                {
                                    DateTime? dateValue = (DateTime?)value;
                                    worksheet.Cells[i + 2, j + 1].Value = dateValue.HasValue ? dateValue.Value.ToShortDateString() : "غير متوفر";
                                }
                                else
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = value?.ToString();
                                }
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns(); // ضبط عرض الأعمدة تلقائيًا

                    var fileBytes = package.GetAsByteArray();
                    var fileName = $"تقرير_تفاصيل_الاعتقال_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    // ترميز اسم الملف لضمان دعمه للأحرف العربية في المتصفحات
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لتسهيل عملية التتبع
                System.Diagnostics.Debug.WriteLine($"Error exporting detention report to Excel: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        /// <summary>
        /// استرجاع البيانات من قاعدة البيانات بناءً على معايير البحث المحددة.
        /// تم تحديث هذه الدالة لتطبيق فلاتر البحث مباشرة على استعلام قاعدة البيانات.
        /// </summary>
        /// <param name="model">نموذج العرض الذي يحتوي على معايير البحث.</param>
        /// <returns>قائمة بالنتائج المطابقة.</returns>
        private List<NewDetentionReportResultItem> GetReportData(NewDetentionReportViewModel model)
        {
            // إنشاء الاستعلام الأساسي مع تضمين الكيانات ذات الصلة
            var query = _context.Lawyers
                .Include(l => l.DetentionDetails) // تضمين تفاصيل الاعتقال
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

            if (model.IsStillDetained.HasValue)
            {
                // فلترة بناءً على ما إذا كان المحامي لا يزال معتقلاً
                query = query.Where(l => l.DetentionDetails.Any(dd => dd.IsStillDetained == model.IsStillDetained.Value));
            }

            if (!string.IsNullOrEmpty(model.DetentionType))
            {
                // فلترة بناءً على نوع الاعتقال
                query = query.Where(l => l.DetentionDetails.Any(dd => dd.DetentionType == model.DetentionType));
            }

            var filteredLawyers = query.ToList();

            var results = new List<NewDetentionReportResultItem>();
            foreach (var lawyer in filteredLawyers)
            {
                // الحصول على تفاصيل الاعتقال (يمكنك تعديل هذا المنطق إذا كان هناك أكثر من اعتقال واحد للمحامي)
                var detentionDetail = lawyer.DetentionDetails.OrderByDescending(dd => dd.DetentionStartDate).FirstOrDefault(); // افتراض آخر اعتقال

                if (detentionDetail != null) // تأكد من وجود تفاصيل اعتقال للمحامي
                {
                    results.Add(new NewDetentionReportResultItem
                    {
                        LawyerIdNumber = lawyer.IdNumber,
                        LawyerFullName = lawyer.FullName,
                        WasDetained = detentionDetail.WasDetained,
                        DetentionDuration = detentionDetail.DetentionDuration,
                        DetentionStartDate = detentionDetail.DetentionStartDate,
                        IsStillDetained = detentionDetail.IsStillDetained,
                        ReleaseDate = detentionDetail.ReleaseDate,
                        DetentionType = detentionDetail.DetentionType,
                        DetentionLocation = detentionDetail.DetentionLocation
                    });
                }
            }

            return results;
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
