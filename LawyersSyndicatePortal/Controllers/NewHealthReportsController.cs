// LawyersSyndicatePortal/Controllers/NewHealthReportsController.cs
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
    /// المتحكم المسؤول عن التقارير والاستعلامات الخاصة بالحالة الصحية للمحامين.
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    // السماح للمسؤولين فقط بالوصول إلى هذا المتحكم
    [Authorize(Roles = "Admin")]
    public class NewHealthReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewHealthReportsController()
        {
            _context = new ApplicationDbContext();
            // تفعيل ترخيص EPPlusFree
            // تم إزالة التعليق عن السطر التالي لحل مشكلة التصدير
       //     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: NewHealthReports/Index
        // يعرض صفحة البحث الرئيسية
        [PermissionAuthorizationFilter("عرض التقارير الصحية", "صلاحية عرض تقارير صحية")]
        [AuditLog("عرض", "عرض صفحة تقارير صحية جديدة")]
        public ActionResult Index()
        {
            var model = new NewHealthReportViewModel();
            // تهيئة الأعمدة المتاحة عند تحميل الصفحة لأول مرة
            model.AvailableColumns = new List<NewHealthReportViewModel.ReportColumn>
            {
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerFullName", DisplayName = "اسم المحامي", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم الهوية / العضوية", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerHealthCondition", DisplayName = "الحالة الصحية للمحامي", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "ProfessionalStatus", DisplayName = "الحالة المهنية", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "HasFamilyMembersInjured", DisplayName = "أفراد عائلة مصابون؟", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "FamilyMembersInjuredDetails", DisplayName = "تفاصيل إصابات العائلة", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "NumberOfInjuredFamilyMembers", DisplayName = "عدد أفراد العائلة المصابين", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerInjuryDetails", DisplayName = "طبيعة إصابة المحامي", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerTreatmentNeeded", DisplayName = "طبيعة العلاج للمحامي", IsSelected = true},
                new NewHealthReportViewModel.ReportColumn { ColumnName = "LawyerDiagnosis", DisplayName = "تشخيص حالة المحامي", IsSelected = true}
            };
            return View(model);
        }

        // POST: NewHealthReports/Search
        // يعالج طلب البحث ويعيد النتائج كجزء جزئي (Partial View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("البحث في التقارير الصحية", "صلاحية البحث في التقارير الصحية")]
        [AuditLog("بحث", "بحث في التقارير الصحية")]
        public ActionResult Search(NewHealthReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var results = GetReportData(model);

                var resultViewModel = new NewHealthReportResultViewModel
                {
                    Reports = results,
                    SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
                };

                return PartialView("_NewHealthReportResults", resultViewModel);
            }
            return Content("<div class='alert alert-danger text-center mt-5' role='alert'>حدث خطأ في معايير البحث.</div>");
        }

        // POST: NewHealthReports/ExportToExcel
        // يعالج طلب تصدير النتائج إلى ملف Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تصدير تقارير صحية", "صلاحية تصدير تقارير صحية")]
        [AuditLog("تصدير", "تصدير تقرير صحي جديد إلى Excel")]
        public ActionResult ExportToExcel(NewHealthReportViewModel model)
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
                    var worksheet = package.Workbook.Worksheets.Add("تقرير الحالة الصحية");
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
                            var prop = typeof(NewHealthReportResultItem).GetProperty(selectedColumns[j].ColumnName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(resultItem, null);
                                // معالجة خاصة للقيم المنطقية (bool?) لتظهر "نعم" أو "لا"
                                if (prop.PropertyType == typeof(bool?))
                                {
                                    bool? boolValue = (bool?)value;
                                    worksheet.Cells[i + 2, j + 1].Value = (boolValue.HasValue && boolValue.Value) ? "نعم" : "لا";
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
                    var fileName = $"تقرير_الحالة_الصحية_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    // ترميز اسم الملف لضمان دعمه للأحرف العربية في المتصفحات
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لتسهيل عملية التتبع
                System.Diagnostics.Debug.WriteLine($"Error exporting health report to Excel: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        /// <summary>
        /// استرجاع البيانات من قاعدة البيانات بناءً على معايير البحث المحددة.
        /// تم تحديث هذه الدالة لتطبيق فلاتر البحث مباشرة على استعلام قاعدة البيانات.
        /// </summary>
        /// <param name="model">نموذج العرض الذي يحتوي على معايير البحث.</param>
        /// <returns>قائمة بالنتائج المطابقة.</returns>
        private List<NewHealthReportResultItem> GetReportData(NewHealthReportViewModel model)
        {
            // إنشاء الاستعلام الأساسي مع تضمين الكيانات ذات الصلة
            // نستخدم HealthStatuses و FamilyMemberInjuries بناءً على الموديلات المرفقة
            var query = _context.Lawyers
                .Include(l => l.HealthStatuses)
                .Include(l => l.HealthStatuses.Select(hs => hs.FamilyMemberInjuries))
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

            if (!string.IsNullOrEmpty(model.LawyerHealthCondition))
            {
                // البحث في LawyerCondition داخل HealthStatuses
                query = query.Where(l => l.HealthStatuses.Any(hs => hs.LawyerCondition == model.LawyerHealthCondition));
            }

            if (!string.IsNullOrEmpty(model.ProfessionalStatus))
            {
                query = query.Where(l => l.ProfessionalStatus == model.ProfessionalStatus);
            }

            if (model.HasFamilyMembersInjured.HasValue)
            {
                if (model.HasFamilyMembersInjured.Value)
                {
                    // البحث عن محامين لديهم أي حالة صحية تشير إلى إصابة أفراد العائلة
                    query = query.Where(l => l.HealthStatuses.Any(hs => hs.HasFamilyMembersInjured));
                }
                else
                {
                    // البحث عن محامين ليس لديهم أي حالة صحية تشير إلى إصابة أفراد العائلة
                    query = query.Where(l => !l.HealthStatuses.Any(hs => hs.HasFamilyMembersInjured));
                }
            }

            var filteredLawyers = query.ToList();

            var results = new List<NewHealthReportResultItem>();
            foreach (var lawyer in filteredLawyers)
            {
                // الحصول على آخر حالة صحية للمحامي (أو التعامل معها حسب منطق عملك)
                var healthStatus = lawyer.HealthStatuses.OrderByDescending(hs => hs.Id).FirstOrDefault(); // افتراض أن ID يعكس الترتيب الزمني

                // تجميع تفاصيل إصابات أفراد العائلة
                string familyInjuriesDetails = "لا يوجد";
                int? numInjuredFamily = 0;

                if (healthStatus != null && healthStatus.HasFamilyMembersInjured && healthStatus.FamilyMemberInjuries != null && healthStatus.FamilyMemberInjuries.Any())
                {
                    familyInjuriesDetails = string.Join("; ", healthStatus.FamilyMemberInjuries.Select(fmi =>
                        $"{fmi.InjuredFamilyMemberName} ({fmi.RelationshipToLawyer}): {fmi.InjuryDetails}"));
                    numInjuredFamily = healthStatus.FamilyMemberInjuries.Count;
                }

                results.Add(new NewHealthReportResultItem
                {
                    LawyerIdNumber = lawyer.IdNumber,
                    LawyerFullName = lawyer.FullName,
                    LawyerHealthCondition = healthStatus?.LawyerCondition,
                    ProfessionalStatus = lawyer.ProfessionalStatus,
                    HasFamilyMembersInjured = healthStatus?.HasFamilyMembersInjured,
                    FamilyMembersInjuredDetails = familyInjuriesDetails,
                    NumberOfInjuredFamilyMembers = numInjuredFamily,
                    LawyerInjuryDetails = healthStatus?.InjuryDetails,
                    LawyerTreatmentNeeded = healthStatus?.TreatmentNeeded,
                    LawyerDiagnosis = healthStatus?.LawyerDiagnosis
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
