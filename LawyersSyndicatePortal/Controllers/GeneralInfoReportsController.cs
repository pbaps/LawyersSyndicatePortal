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
    /// المتحكم المسؤول عن التقارير والاستعلامات الخاصة بالمعلومات العامة للمحامين.
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    public class GeneralInfoReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeneralInfoReportsController()
        {
            _context = new ApplicationDbContext();
            // تفعيل ترخيص EPPlusFree
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: GeneralInfoReports/Index
        // يعرض صفحة البحث الرئيسية

        [PermissionAuthorizationFilter("عرض تقرير المعلومات العامة", "صلاحية عرض تقرير المعلومات العامة للمحامين")]
        [AuditLog("عرض تقرير المعلومات العامة", "عرض صفحة تقرير المعلومات العامة")]
        public ActionResult Index()
        {
            var model = new GeneralInfoReportViewModel();
            // تهيئة الأعمدة المتاحة عند تحميل الصفحة لأول مرة
            model.AvailableColumns = new List<GeneralInfoReportViewModel.ReportColumn>
            {
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "LawyerFullName", DisplayName = "اسم المحامي", IsSelected = true},
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم الهوية / العضوية", IsSelected = true},
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "PracticesShariaLaw", DisplayName = "يمارس المهنة الشرعية؟", IsSelected = true},
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "ShariaLawPracticeStartDate", DisplayName = "تاريخ مزاولة الشرعية", IsSelected = true},
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "ReceivedAidFromSyndicate", DisplayName = "استلم مساعدات؟", IsSelected = true},
                new GeneralInfoReportViewModel.ReportColumn { ColumnName = "ReceivedAidsDetails", DisplayName = "تفاصيل المساعدات المستلمة", IsSelected = true}
            };
            return View(model);
        }

        // POST: GeneralInfoReports/Search
        // يعالج طلب البحث ويعيد النتائج كجزء جزئي (Partial View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("بحث في تقرير المعلومات العامة", "صلاحية البحث عن بيانات في تقرير المعلومات العامة")]
        [AuditLog("استعلام تقرير المعلومات العامة", "استعلام عن تقرير المعلومات العامة")]
        public ActionResult Search(GeneralInfoReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var results = GetReportData(model);

                var resultViewModel = new GeneralInfoReportResultViewModel
                {
                    Reports = results,
                    SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
                };

                return PartialView("_GeneralInfoReportResults", resultViewModel);
            }
            return Content("<div class='alert alert-danger text-center mt-5' role='alert'>حدث خطأ في معايير البحث.</div>");
        }

        // POST: GeneralInfoReports/ExportToExcel
        // يعالج طلب تصدير النتائج إلى ملف Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تصدير تقرير المعلومات العامة", "صلاحية تصدير تقرير المعلومات العامة إلى Excel")]
        [AuditLog("تصدير تقرير المعلومات العامة", "تصدير تقرير المعلومات العامة إلى Excel")]
        public ActionResult ExportToExcel(GeneralInfoReportViewModel model)
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
                    var worksheet = package.Workbook.Worksheets.Add("تقرير المعلومات العامة");
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
                            var prop = typeof(GeneralInfoReportResultItem).GetProperty(selectedColumns[j].ColumnName);
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
                                else
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = value?.ToString();
                                }
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns(); // ضبط عرض الأعمدة تلقائيًا

                    var fileBytes = package.GetAsByteArray();
                    var fileName = $"تقرير_المعلومات_العامة_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    // ترميز اسم الملف لضمان دعمه للأحرف العربية في المتصفحات
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لتسهيل عملية التتبع
                System.Diagnostics.Debug.WriteLine($"Error exporting general info report to Excel: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        /// <summary>
        /// استرجاع البيانات من قاعدة البيانات بناءً على معايير البحث المحددة.
        /// </summary>
        /// <param name="model">نموذج العرض الذي يحتوي على معايير البحث.</param>
        /// <returns>قائمة بالنتائج المطابقة.</returns>
        private List<GeneralInfoReportResultItem> GetReportData(GeneralInfoReportViewModel model)
        {
            // إنشاء الاستعلام الأساسي مع تضمين الكيانات ذات الصلة
            var query = _context.Lawyers
                .Include(l => l.GeneralInfos)
                .Include(l => l.GeneralInfos.Select(gi => gi.ReceivedAids)) // تضمين المساعدات المستلمة
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

            if (model.PracticesShariaLaw.HasValue)
            {
                query = query.Where(l => l.GeneralInfos.Any(gi => gi.PracticesShariaLaw == model.PracticesShariaLaw.Value));
            }

            if (model.ReceivedAidFromSyndicate.HasValue)
            {
                query = query.Where(l => l.GeneralInfos.Any(gi => gi.ReceivedAidFromSyndicate == model.ReceivedAidFromSyndicate.Value));
            }

            if (!string.IsNullOrEmpty(model.AidType))
            {
                query = query.Where(l => l.GeneralInfos.Any(gi => gi.ReceivedAids.Any(ra => ra.AidType.Contains(model.AidType))));
            }

            var filteredLawyers = query.ToList();

            var results = new List<GeneralInfoReportResultItem>();
            foreach (var lawyer in filteredLawyers)
            {
                var generalInfo = lawyer.GeneralInfos.FirstOrDefault(); // افتراض وجود معلومات عامة واحدة لكل محامي

                string receivedAidsDetails = "لا يوجد";
                if (generalInfo != null && generalInfo.ReceivedAidFromSyndicate && generalInfo.ReceivedAids != null && generalInfo.ReceivedAids.Any())
                {
                    receivedAidsDetails = string.Join("; ", generalInfo.ReceivedAids.Select(ra =>
                        $"{ra.AidType} ({ra.ReceivedDate?.ToShortDateString() ?? "غير محدد"})"));
                }

                results.Add(new GeneralInfoReportResultItem
                {
                    LawyerIdNumber = lawyer.IdNumber,
                    LawyerFullName = lawyer.FullName,
                    PracticesShariaLaw = generalInfo?.PracticesShariaLaw,
                    ShariaLawPracticeStartDate = generalInfo?.ShariaLawPracticeStartDate,
                    ReceivedAidFromSyndicate = generalInfo?.ReceivedAidFromSyndicate,
                    ReceivedAidsDetails = receivedAidsDetails
                });
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
