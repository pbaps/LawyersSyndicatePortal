using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static LawyersSyndicatePortal.ViewModels.ColleagueReportViewModel;


namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن التقارير والاستعلامات المتخصصة حول الزملاء (شهداء, معتقلين, مصابين).
    /// يسمح بالبحث الديناميكي وتصدير النتائج إلى Excel.
    /// </summary>
    // تمت إزالة فلتر الصلاحيات و AuditLog من هنا لتطبيقهما بشكل فردي
    [Authorize(Roles = "Admin")]
    public class ColleagueReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColleagueReportsController()
        {
            _context = new ApplicationDbContext();
            // Note: If you are using EPPlus 5 or later, you may need to set a non-commercial license
            // by uncommenting the following line:
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: ColleagueReports/Index
        // Displays the search and filter page.
        [PermissionAuthorizationFilter("تقارير الزملاء", "صلاحية عرض تقارير الزملاء")]
        [AuditLog("عرض", "عرض صفحة تقارير الزملاء")]
        public ActionResult Index()
        {
            var model = new ColleagueReportViewModel
            {
                // تهيئة قائمة الأعمدة المتاحة بناءً على الحقول المتوفرة في النماذج
                AvailableColumns = new List<ReportColumn>
        {
            // هذا العمود سيكون مختارًا بشكل افتراضي
            new ReportColumn { ColumnName = "LawyerFullName", DisplayName = "اسم المحامي (الراوي)", IsSelected = false },
            new ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم هوية المحامي (الراوي)" , IsSelected = false },
            // هذا العمود سيكون مختارًا بشكل افتراضي
            new ReportColumn { ColumnName = "ColleagueFullName", DisplayName = "اسم الزميل", IsSelected = true },
            new ReportColumn { ColumnName = "MobileNumber", DisplayName = "رقم التواصل" , IsSelected = true },
            new ReportColumn { ColumnName = "ReportType", DisplayName = "نوع التقرير" , IsSelected = true }
        }
            };

            return View(model);
        }

        // POST: ColleagueReports/Search
        // Handles the search request via AJAX and returns a partial view.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("البحث في تقارير الزملاء", "صلاحية البحث في تقارير الزملاء")]
        [AuditLog("البحث", "البحث في تقارير الزملاء")]
        public async Task<ActionResult> Search(ColleagueReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return an empty or error view if the model is not valid.
                return PartialView("_ColleagueReportResults", new ColleagueReportResultViewModel());
            }

            // Get selected columns from the model
            var selectedColumns = model.AvailableColumns
                .Where(c => c.IsSelected)
                .ToList();

            var results = await GetReportDataAsync(model);

            // Set the search results count here
            var viewModel = new ColleagueReportResultViewModel
            {
                Reports = results,
                SelectedColumns = selectedColumns,
                SearchResultsCount = results.Count // Adding the count to the view model
            };

            return PartialView("_ColleagueReportResults", viewModel);
        }

        // POST: ColleagueReports/ExportToExcel
        // Exports the search results to an Excel file.
        [HttpPost]
        [PermissionAuthorizationFilter("تصدير تقارير الزملاء", "صلاحية تصدير تقارير الزملاء إلى Excel")]
        [AuditLog("تصدير", "تصدير تقارير الزملاء إلى Excel")]
        public async Task<ActionResult> ExportToExcel(ColleagueReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Handle invalid model state if necessary
                return RedirectToAction("Index");
            }

            var results = await GetReportDataAsync(model);

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Colleague Report");
                worksheet.Cells.Style.Font.Name = "Arial";
                worksheet.Cells.Style.Font.Size = 10;
                worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                // Add header row
                var selectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList();
                for (int i = 0; i < selectedColumns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = selectedColumns[i].DisplayName;
                }

                // Add data rows
                for (int i = 0; i < results.Count; i++)
                {
                    var item = results[i];
                    for (int j = 0; j < selectedColumns.Count; j++)
                    {
                        var propName = selectedColumns[j].ColumnName;
                        var prop = typeof(ColleagueReportResultItem).GetProperty(propName);
                        if (prop != null)
                        {
                            var value = prop.GetValue(item, null);
                            worksheet.Cells[i + 2, j + 1].Value = value;
                        }
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set content type and file name for the Excel file
                var fileGuid = Guid.NewGuid().ToString();
                var fileName = $"ColleagueReport_{fileGuid}.xlsx";
                var stream = new MemoryStream(package.GetAsByteArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Private method to get the filtered data
        private async Task<List<ColleagueReportResultItem>> GetReportDataAsync(ColleagueReportViewModel model)
        {
            var results = new List<ColleagueReportResultItem>();

            // Get all lawyers that have colleague info
            var query = _context.Lawyers
                .Include(l => l.ColleagueInfos.Select(ci => ci.MartyrColleagues))
                .Include(l => l.ColleagueInfos.Select(ci => ci.DetainedColleagues))
                .Include(l => l.ColleagueInfos.Select(ci => ci.InjuredColleagues))
                .Where(l => l.ColleagueInfos.Any());

            // Filter by lawyer name if provided
            if (!string.IsNullOrEmpty(model.FullName))
            {
                query = query.Where(l => l.FullName.Contains(model.FullName));
            }

            var lawyers = await query.ToListAsync();

            if (model.ReportType == "Martyr" || string.IsNullOrEmpty(model.ReportType))
            {
                foreach (var lawyer in lawyers)
                {
                    foreach (var colleagueInfo in lawyer.ColleagueInfos)
                    {
                        results.AddRange(colleagueInfo.MartyrColleagues.Select(m => new ColleagueReportResultItem
                        {
                            LawyerFullName = lawyer.FullName,
                            LawyerIdNumber = lawyer.IdNumber,
                            ColleagueFullName = m.MartyrName,
                            MobileNumber = m.ContactNumber,
                            ReportType = "شهيد",
                        }));
                    }
                }
            }

            if (model.ReportType == "Detained" || string.IsNullOrEmpty(model.ReportType))
            {
                foreach (var lawyer in lawyers)
                {
                    foreach (var colleagueInfo in lawyer.ColleagueInfos)
                    {
                        results.AddRange(colleagueInfo.DetainedColleagues.Select(d => new ColleagueReportResultItem
                        {
                            LawyerFullName = lawyer.FullName,
                            LawyerIdNumber = lawyer.IdNumber,
                            ColleagueFullName = d.DetainedName,
                            MobileNumber = d.ContactNumber,
                            ReportType = "معتقل",
                        }));
                    }
                }
            }

            if (model.ReportType == "Injured" || string.IsNullOrEmpty(model.ReportType))
            {
                foreach (var lawyer in lawyers)
                {
                    foreach (var colleagueInfo in lawyer.ColleagueInfos)
                    {
                        results.AddRange(colleagueInfo.InjuredColleagues.Select(i => new ColleagueReportResultItem
                        {
                            LawyerFullName = lawyer.FullName,
                            LawyerIdNumber = lawyer.IdNumber,
                            ColleagueFullName = i.InjuredName,
                            MobileNumber = i.ContactNumber,
                            ReportType = "مصاب",
                        }));
                    }
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
