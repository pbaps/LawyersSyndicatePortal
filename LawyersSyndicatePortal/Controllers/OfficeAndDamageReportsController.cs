using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml; // تأكد من تثبيت حزمة EPPlus
using OfficeOpenXml.Style;
using System.Web; // إضافة هذه المكتبة للتعامل مع URL Encode
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن التقارير والاستعلامات الخاصة بالمكاتب وأضرارها وأضرار المنازل.
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    // السماح للمسؤولين فقط بالوصول إلى هذا المتحكم
    [Authorize(Roles = "Admin")]
    public class OfficeAndDamageReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficeAndDamageReportsController()
        {
            _context = new ApplicationDbContext();
            // ملاحظة: إذا كنت تستخدم EPPlus 5 أو أحدث، قد تحتاج لتعيين رخصة غير تجارية
            // عن طريق إزالة التعليق عن السطر التالي:
            ///   ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: OfficeAndDamageReports/Index
        // يعرض صفحة البحث الرئيسية
        // يعرض صفحة البحث الرئيسية
        [PermissionAuthorizationFilter("عرض تقارير المكاتب والأضرار", "صلاحية عرض تقارير المكاتب والأضرار")]
        [AuditLog("عرض", "عرض صفحة تقارير المكاتب والأضرار")]
        public ActionResult Index()
        {
            var model = new OfficeAndDamageReportViewModel();
            // Set the property value

            return View(model);
        }

        // POST: OfficeAndDamageReports/Search
        // يعالج طلب البحث ويعيد النتائج كجزء جزئي (Partial View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("البحث في تقارير المكاتب والأضرار", "صلاحية البحث في تقارير المكاتب والأضرار")]
        [AuditLog("بحث", "بحث في تقارير المكاتب والأضرار")]
        public ActionResult Search(OfficeAndDamageReportViewModel model)
        {
            // التحقق من صحة النموذج
            if (ModelState.IsValid)
            {
                // تنفيذ عملية البحث
                var results = GetReportData(model);
                // تحديث عدد النتائج في النموذج
                model.SearchResultsCount = results.Count;
                // إنشاء نموذج عرض النتائج
                var resultViewModel = new OfficeAndDamageReportResultViewModel
                {
                    Reports = results,
                    SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList(),
                    SearchResultsCount = model.SearchResultsCount // نقل العدد إلى نموذج النتائج
                };

                // إعادة الجزء الجزئي مع النتائج
                return PartialView("_OfficeAndDamageReportResults", resultViewModel);
            }

            // في حالة عدم صحة النموذج، قم بإرجاع جزء فارغ أو رسالة خطأ
            return Content("<div class='alert alert-danger text-center mt-5' role='alert'>حدث خطأ في معايير البحث.</div>");
        }

        // POST: OfficeAndDamageReports/ExportToExcel
        // يعالج طلب تصدير النتائج إلى ملف Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تصدير تقارير المكاتب والأضرار", "صلاحية تصدير تقارير المكاتب والأضرار")]
        [AuditLog("تصدير", "تصدير تقرير المكاتب والأضرار إلى Excel")]
        public ActionResult ExportToExcel(OfficeAndDamageReportViewModel model)
        {
            // التحقق من صحة النموذج قبل معالجة الطلب
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, "بيانات الطلب غير صالحة.");
            }

            try
            {
                var results = GetReportData(model);
                var selectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList();

                // إنشاء حزمة Excel جديدة
                using (var package = new ExcelPackage())
                {
                    // إنشاء ورقة عمل جديدة
                    var worksheet = package.Workbook.Worksheets.Add("تقارير المكاتب والأضرار");

                    // تعيين اتجاه الورقة من اليمين إلى اليسار للنصوص العربية
                    worksheet.View.RightToLeft = true;

                    // إضافة العناوين
                    for (int i = 0; i < selectedColumns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = selectedColumns[i].DisplayName;
                    }

                    // تنسيق العناوين
                    using (var range = worksheet.Cells[1, 1, 1, selectedColumns.Count])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // إضافة بيانات النتائج
                    for (int i = 0; i < results.Count; i++)
                    {
                        var resultItem = results[i];
                        for (int j = 0; j < selectedColumns.Count; j++)
                        {
                            var prop = typeof(OfficeAndDamageReportResultItem).GetProperty(selectedColumns[j].ColumnName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(resultItem, null);
                                worksheet.Cells[i + 2, j + 1].Value = value?.ToString();
                            }
                        }
                    }

                    // ضبط عرض الأعمدة تلقائيًا
                    worksheet.Cells.AutoFitColumns();

                    // إرجاع الملف مع ترميز اسم الملف بالعربية
                    var fileBytes = package.GetAsByteArray();
                    var fileName = $"تقرير_المكاتب_والأضرار_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ هنا لتسهيل عملية التتبع في بيئة التطوير/الإنتاج
                System.Diagnostics.Debug.WriteLine($"Error exporting to Excel: {ex.Message}");
                // إرجاع رسالة خطأ مناسبة للمستخدم
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        /// <summary>
        /// منطق تجميع البيانات بناءً على معايير البحث
        /// تم تحديث هذه الدالة لتطبيق فلاتر البحث مباشرة على استعلام قاعدة البيانات (Server-side filtering).
        /// هذا يقلل من حجم البيانات المنقولة ويزيد من كفاءة الأداء.
        /// </summary>
        /// <param name="model">نموذج العرض الذي يحتوي على معايير البحث.</param>
        /// <returns>قائمة بالنتائج المطابقة.</returns>
        private List<OfficeAndDamageReportResultItem> GetReportData(OfficeAndDamageReportViewModel model)
        {
            // إنشاء الاستعلام الأساسي مع تضمين الكيانات ذات الصلة مرة واحدة
            var query = _context.Lawyers
                .Include(l => l.PersonalDetails)
                .Include(l => l.OfficeDetails)
                .Include(l => l.HomeDamages)
                .Include(l => l.OfficeDamages)
                .AsQueryable();

            // تطبيق فلاتر البحث ديناميكياً على مستوى قاعدة البيانات
            if (!string.IsNullOrEmpty(model.LawyerFullName))
            {
                query = query.Where(l => l.FullName.Contains(model.LawyerFullName));
            }

            if (!string.IsNullOrEmpty(model.LawyerIdNumber))
            {
                query = query.Where(l => l.IdNumber == model.LawyerIdNumber);
            }

            if (!string.IsNullOrEmpty(model.OfficeDamageType))
            {
                query = query.Where(l => l.OfficeDamages.Any(od => od.DamageType == model.OfficeDamageType));
            }

            if (!string.IsNullOrEmpty(model.HomeDamageType))
            {
                query = query.Where(l => l.HomeDamages.Any(hd => hd.DamageType == model.HomeDamageType));
            }

            if (!string.IsNullOrEmpty(model.CurrentGovernorate))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.CurrentGovernorate == model.CurrentGovernorate));
            }

            if (!string.IsNullOrEmpty(model.OriginalGovernorate))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.OriginalGovernorate == model.OriginalGovernorate));
            }

            // تنفيذ الاستعلام بعد تطبيق جميع الفلاتر
            var filteredLawyers = query.ToList();

            // تحويل النتائج إلى نموذج العرض المطلوب
            var results = new List<OfficeAndDamageReportResultItem>();
            foreach (var lawyer in filteredLawyers)
            {
                // يمكننا الآن الوصول إلى البيانات المضمنة مباشرة
                var personalDetail = lawyer.PersonalDetails.FirstOrDefault();
                var officeDetail = lawyer.OfficeDetails.FirstOrDefault();
                var officeDamage = lawyer.OfficeDamages.FirstOrDefault();
                var homeDamage = lawyer.HomeDamages.FirstOrDefault();

                results.Add(new OfficeAndDamageReportResultItem
                {
                    LawyerIdNumber = lawyer.IdNumber,
                    LawyerFullName = lawyer.FullName,
                    CurrentGovernorate = personalDetail?.CurrentGovernorate,
                    OriginalGovernorate = personalDetail?.OriginalGovernorate,
                    OfficeAddress = officeDetail?.OfficeAddress,
                    OfficeDamageType = officeDamage?.DamageType,
                    OfficeDamageDetails = officeDamage?.DamageDetails,
                    HomeDamageType = homeDamage?.DamageType,
                    HomeDamageDetails = homeDamage?.DamageDetails
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
