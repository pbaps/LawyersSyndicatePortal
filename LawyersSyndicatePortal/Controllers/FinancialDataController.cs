using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web; // Needed for HttpUtility
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinancialDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialDataController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: FinancialData/Index
        [PermissionAuthorizationFilter("عرض البيانات المالية", "صلاحية عرض صفحة تقرير البيانات المالية")]
        [AuditLog("عرض البيانات المالية", "عرض صفحة تقرير البيانات المالية")]
        public ActionResult Index()
        {
            var model = new FinancialDataReportViewModel();

            // تعبئة قائمة البنوك
            model.BankList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل البنوك" },
                new SelectListItem { Value = "بنك فلسطين", Text = "بنك فلسطين" },
                new SelectListItem { Value = "البنك الإسلامي العربي", Text = "البنك الإسلامي العربي" },
                new SelectListItem { Value = "البنك الإسلامي الفلسطيني", Text = "البنك الإسلامي الفلسطيني" },
                new SelectListItem { Value = "البنك الوطني", Text = "البنك الوطني" },
                new SelectListItem { Value = "بنك القدس", Text = "بنك القدس" },
                new SelectListItem { Value = "البنك العربي", Text = "البنك العربي" }
            };

            // تحديد الأعمدة المتاحة للعرض والتصدير
            model.AvailableColumns = new List<FinancialDataReportViewModel.ReportColumn>
            {
                // معلومات المحامي الأساسية
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "IdNumber", DisplayName = "رقم الهوية", IsSelected = true },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "FullName", DisplayName = "الاسم الكامل", IsSelected = true },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "MembershipNumber", DisplayName = "رقم العضوية", IsSelected = false },

                // تفاصيل الحساب البنكي
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.BankName", DisplayName = "اسم البنك", IsSelected = true },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.IBAN", DisplayName = "رقم الايبان", IsSelected = true },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.BankAccountNumber", DisplayName = "رقم الحساب", IsSelected = false },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.WalletType", DisplayName = "نوع المحفظة", IsSelected = false },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.WalletAccountNumber", DisplayName = "رقم حساب المحفظة", IsSelected = false },
                new FinancialDataReportViewModel.ReportColumn { ColumnName = "PersonalDetails.BankBranch", DisplayName = "فرع البنك", IsSelected = false },
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("استعلام البيانات المالية", "صلاحية استعراض نتائج استعلام البيانات المالية")]
        [AuditLog("استعلام البيانات المالية", "استعلام عن البيانات المالية")]
        public async Task<ActionResult> Search(FinancialDataReportViewModel model)
        {
            IQueryable<Lawyer> query = _context.Lawyers;

            // تطبيق مرشحات البحث
            if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                query = query.Where(l => l.FullName.Contains(model.FullName));
            }
            if (!string.IsNullOrWhiteSpace(model.IdNumber))
            {
                query = query.Where(l => l.IdNumber.Contains(model.IdNumber));
            }
            if (!string.IsNullOrWhiteSpace(model.MembershipNumber))
            {
                query = query.Where(l => l.MembershipNumber == model.MembershipNumber);
            }
            if (!string.IsNullOrWhiteSpace(model.BankName))
            {
                // Note: The query now checks if ANY of the PersonalDetails has the specified BankName
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.BankName == model.BankName));
            }

            // تضمين البيانات المرتبطة
            query = query.Include(l => l.PersonalDetails);

            var lawyers = await query.ToListAsync();

            var resultModel = new FinancialDataReportResultViewModel
            {
                Lawyers = lawyers,
                SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
            };

            return PartialView("_FinancialDataReportPartial", resultModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تصدير البيانات المالية", "صلاحية تصدير تقرير البيانات المالية إلى Excel")]
        [AuditLog("تصدير البيانات المالية", "تصدير تقرير البيانات المالية")]
        public async Task<ActionResult> ExportToExcel(FinancialDataReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, "بيانات الطلب غير صالحة.");
            }

            try
            {
                IQueryable<Lawyer> query = _context.Lawyers;

                // تطبيق نفس مرشحات البحث
                if (!string.IsNullOrWhiteSpace(model.FullName))
                {
                    query = query.Where(l => l.FullName.Contains(model.FullName));
                }
                if (!string.IsNullOrWhiteSpace(model.IdNumber))
                {
                    query = query.Where(l => l.IdNumber.Contains(model.IdNumber));
                }
                if (!string.IsNullOrWhiteSpace(model.MembershipNumber))
                {
                    query = query.Where(l => l.MembershipNumber == model.MembershipNumber);
                }
                if (!string.IsNullOrWhiteSpace(model.BankName))
                {
                    query = query.Where(l => l.PersonalDetails.Any(pd => pd.BankName == model.BankName));
                }

                query = query.Include(l => l.PersonalDetails);

                var lawyers = await query.ToListAsync();
                var selectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList();

                if (!lawyers.Any() || !selectedColumns.Any())
                {
                    return new HttpStatusCodeResult(404, "لا توجد بيانات للتصدير.");
                }

                using (var pck = new ExcelPackage())
                {
                    var ws = pck.Workbook.Worksheets.Add("تقرير البيانات المالية");
                    ws.View.RightToLeft = true; // Set worksheet direction to right-to-left

                    // Add headers with styling
                    int colCount = 1;
                    foreach (var column in selectedColumns)
                    {
                        ws.Cells[1, colCount].Value = column.DisplayName;
                        ws.Cells[1, colCount].Style.Font.Bold = true;
                        ws.Cells[1, colCount].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[1, colCount].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        colCount++;
                    }

                    int row = 2;
                    foreach (var lawyer in lawyers)
                    {
                        int col = 1;
                        foreach (var column in selectedColumns)
                        {
                            var value = GetPropertyValue(lawyer, column.ColumnName);
                            ws.Cells[row, col++].Value = value?.ToString();
                        }
                        row++;
                    }

                    ws.Cells.AutoFitColumns();
                    var fileBytes = pck.GetAsByteArray();
                    var fileName = $"FinancialDataReport_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting financial report to Excel: {ex.Message}");
                return new HttpStatusCodeResult(500, "حدث خطأ داخلي في الخادم أثناء تصدير الملف.");
            }
        }

        // هذه الدالة المساعدة تم تعديلها للتعامل مع المجموعات (ICollection)
        private object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null) return null;

            var parts = propertyPath.Split('.');
            object currentObject = obj;

            foreach (var part in parts)
            {
                if (currentObject == null) return null;

                // If the current object is a collection, find the first item to get the property from
                if (currentObject is System.Collections.IEnumerable enumerable && !(currentObject is string))
                {
                    currentObject = enumerable.Cast<object>().FirstOrDefault();
                    if (currentObject == null) return null;
                }

                var propertyInfo = currentObject.GetType().GetProperty(part);
                if (propertyInfo != null)
                {
                    currentObject = propertyInfo.GetValue(currentObject, null);
                }
                else
                {
                    return null;
                }
            }
            return currentObject;
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
