using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;
using System.Drawing;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController()
        {
            _context = new ApplicationDbContext();
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: Reports/Index
        [PermissionAuthorizationFilter("عرض قائمة التقارير", "صلاحية عرض قائمة التقارير")]
        [AuditLog("عرض التقارير", "عرض قائمة التقارير")]
        public ActionResult Index()
        {
            ViewBag.Title = "التقارير والإحصائيات";
            return View();
        }

        // =======================================================================
        //  NEW: Comprehensive Report (التقرير الشامل) - الكود الجديد بالكامل
        // =======================================================================

        // GET: Reports/ComprehensiveReport
        // GET: Reports/ComprehensiveReport
        public ActionResult ComprehensiveReport()
        {
            try
            {
                // 1. التأكد من الدوال المساعدة
                var govs = GetGovernorates();
                if (govs == null) throw new Exception("GetGovernorates returned null");

                var genders = GetGenders();
                var profs = GetExactProfessionalStatuses();
                var maritals = GetMaritalStatuses();

                // 2. إنشاء الموديل
                var model = new ComprehensiveLawyerReportViewModel
                {
                    // تعبئة القوائم
                    GovernorateList = new MultiSelectList(govs, "Value", "Text"),
                    GenderList = new SelectList(genders, "Value", "Text"),
                    ProfessionalStatusList = new SelectList(profs, "Value", "Text"),
                    MaritalStatusList = new MultiSelectList(maritals, "Value", "Text"),

                    // 3. تهيئة القوائم الفارغة لتجنب Null Reference في الفيو
                    SelectedCurrentGovernorates = new List<string>(),
                    SelectedOriginalGovernorates = new List<string>(),
                    SelectedMaritalStatuses = new List<string>(),
                    Results = new List<LawyerReportRow>(),

                    // تهيئة الأعمدة الافتراضية
                    SelectedColumns = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // طباعة الخطأ على الشاشة مباشرة
                string msg = "<h1>Error Details</h1>";
                msg += $"<h3>Message: {ex.Message}</h3>";
                if (ex.InnerException != null)
                {
                    msg += $"<h3>Inner Exception: {ex.InnerException.Message}</h3>";
                }
                msg += $"<pre>{ex.StackTrace}</pre>";

                return Content(msg, "text/html");
            }
        }

        [HttpPost]
        public async Task<ActionResult> ComprehensiveReport(ComprehensiveLawyerReportViewModel model, string submitButton)
        {
            // 1. Refill lists
            model.GovernorateList = new MultiSelectList(GetGovernorates(), "Value", "Text", model.SelectedCurrentGovernorates);
            model.GenderList = new SelectList(GetGenders(), "Value", "Text", model.SelectedGender);
            model.ProfessionalStatusList = new SelectList(GetExactProfessionalStatuses(), "Value", "Text", model.SelectedProfessionalStatus);
            model.MaritalStatusList = new MultiSelectList(GetMaritalStatuses(), "Value", "Text", model.SelectedMaritalStatuses);

            // 2. Build Query
            var query = _context.Lawyers
                .Include(l => l.PersonalDetails)
                .Include(l => l.FamilyDetails.Select(fd => fd.Spouses))
                .Include(l => l.FamilyDetails.Select(fd => fd.Children))
                .AsQueryable();

            // --- Apply Filters ---

            // Professional Status (Arabic DB value)
            if (!string.IsNullOrEmpty(model.SelectedProfessionalStatus) && model.SelectedProfessionalStatus != "كل الحالات")
            {
                if (model.SelectedProfessionalStatus == "فارغ")
                    query = query.Where(l => string.IsNullOrEmpty(l.ProfessionalStatus));
                else
                    query = query.Where(l => l.ProfessionalStatus == model.SelectedProfessionalStatus);
            }

            // Gender (English DB value)
            if (!string.IsNullOrEmpty(model.SelectedGender) && model.SelectedGender != "الكل")
            {
                query = query.Where(l => l.Gender == model.SelectedGender);
            }

            // Governorates (Arabic DB value - Multi-select)
            if (model.SelectedCurrentGovernorates != null && model.SelectedCurrentGovernorates.Any())
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => model.SelectedCurrentGovernorates.Contains(pd.CurrentGovernorate)));
            }
            if (model.SelectedOriginalGovernorates != null && model.SelectedOriginalGovernorates.Any())
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => model.SelectedOriginalGovernorates.Contains(pd.OriginalGovernorate)));
            }

            // Marital Status (English DB value - Multi-select)
            if (model.SelectedMaritalStatuses != null && model.SelectedMaritalStatuses.Any())
            {
                // If "All" (empty string) is not selected, apply filter
                if (!model.SelectedMaritalStatuses.Contains(""))
                {
                    query = query.Where(l => l.FamilyDetails.Any(fd => model.SelectedMaritalStatuses.Contains(fd.MaritalStatus)));
                }
            }

            // Numeric Family Filters
            if (model.SpousesCount.HasValue)
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.NumberOfSpouses == model.SpousesCount.Value));

            if (model.ChildrenCountMin.HasValue)
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.NumberOfChildren >= model.ChildrenCountMin.Value));

            // Child Age Filter
            if (model.MinChildAge.HasValue || model.MaxChildAge.HasValue)
            {
                var today = DateTime.Today;
                DateTime? minDob = model.MaxChildAge.HasValue ? today.AddYears(-(model.MaxChildAge.Value + 1)).AddDays(1) : (DateTime?)null;
                DateTime? maxDob = model.MinChildAge.HasValue ? today.AddYears(-model.MinChildAge.Value) : (DateTime?)null;

                query = query.Where(l => l.FamilyDetails.Any(fd => fd.Children.Any(c =>
                    c.DateOfBirth.HasValue &&
                    (!minDob.HasValue || c.DateOfBirth >= minDob.Value) &&
                    (!maxDob.HasValue || c.DateOfBirth <= maxDob.Value)
                )));
            }

            // Total Family Members Filter
            if (model.TotalFamilyMembersMin.HasValue)
            {
                query = query.Where(l => l.FamilyDetails.Any(fd =>
                    (1 + (fd.NumberOfSpouses ?? 0) + (fd.NumberOfChildren ?? 0)) >= model.TotalFamilyMembersMin.Value
                ));
            }

            // 3. Execute Query
            var lawyers = await query.ToListAsync();

            // 4. Map to ViewModel
            model.Results = lawyers.Select(l => {
                var personal = l.PersonalDetails.FirstOrDefault();
                var family = l.FamilyDetails.FirstOrDefault();
                var children = family?.Children.ToList() ?? new List<Child>();
                var spouses = family?.Spouses.ToList() ?? new List<Spouse>();

                int spousesCount = family?.NumberOfSpouses ?? 0;
                int childrenCount = family?.NumberOfChildren ?? 0;
                int totalMembers = 1 + spousesCount + childrenCount;

                return new LawyerReportRow
                {
                    LawyerIdNumber = l.IdNumber,
                    FullName = l.FullName,
                    ProfessionalStatus = l.ProfessionalStatus,
                    MembershipNumber = l.MembershipNumber,
                    Gender = l.Gender,
                    MobileNumber = personal != null ? personal.MobileNumber : "",
                    CurrentGovernorate = personal != null ? personal.CurrentGovernorate : "",
                    OriginalGovernorate = personal != null ? personal.OriginalGovernorate : "",
                    MaritalStatus = family != null ? family.MaritalStatus : "",
                    NumberOfSpouses = spousesCount,
                    HasChildren = family != null ? family.HasChildren : false,
                    NumberOfChildren = childrenCount,
                    TotalFamilyMembers = totalMembers,
                    SpousesNames = string.Join(" | ", spouses.Select(s => s.SpouseName)),
                    SpousesIds = string.Join(" | ", spouses.Select(s => s.SpouseIdNumber)),
                    SpousesMobiles = string.Join(" | ", spouses.Select(s => s.SpouseMobileNumber)),
                    ChildrenNames = string.Join(" | ", children.Select(c => c.ChildName)),
                    ChildrenGenders = string.Join(" | ", children.Select(c => c.Gender)),
                    ChildrenDOBs = string.Join(" | ", children.Select(c => c.DateOfBirth?.ToString("yyyy-MM-dd") ?? "-")),
                    ChildrenAges = string.Join(" | ", children.Select(c => c.DateOfBirth.HasValue ? (DateTime.Now.Year - c.DateOfBirth.Value.Year).ToString() : "-"))
                };
            }).ToList();

            if (submitButton == "ExportExcel")
            {
                return ExportComprehensiveReportToExcel(model.Results, model.SelectedColumns);
            }

            return View(model);
        }

        private FileResult ExportComprehensiveReportToExcel(List<LawyerReportRow> data, List<string> selectedColumns)
        {
            // Default to all columns if none selected
            if (selectedColumns == null || !selectedColumns.Any())
            {
                selectedColumns = Enumerable.Range(0, 13).Select(x => x.ToString()).ToList();
            }

            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("التقرير الشامل");

                // Define Columns Map
                var columnsMap = new Dictionary<string, (string Header, Func<LawyerReportRow, object> GetValue)>
                {
                    { "0", ("اسم المحامي", x => x.FullName) },
                    { "1", ("رقم الهوية", x => x.LawyerIdNumber) },
                    { "2", ("رقم الجوال", x => x.MobileNumber) },
                    { "3", ("الحالة المهنية", x => x.ProfessionalStatus) },
                    { "4", ("رقم العضوية", x => x.MembershipNumber) },
                    { "5", ("الحالة الاجتماعية", x => x.MaritalStatus) },
                    { "6", ("عدد الزوجات", x => x.NumberOfSpouses) },
                    { "7", ("أسماء الزوجات", x => x.SpousesNames.Replace(" | ", "\n")) },
                    { "8", ("هويات الزوجات", x => x.SpousesIds.Replace(" | ", "\n")) },
                    { "9", ("جوالات الزوجات", x => x.SpousesMobiles.Replace(" | ", "\n")) },
                    { "10", ("عدد الأبناء", x => x.NumberOfChildren) },
                    { "11", ("إجمالي الأسرة", x => x.TotalFamilyMembers) },
                    { "12", ("بيانات الأبناء", x =>
                        string.IsNullOrEmpty(x.ChildrenNames) ? "-" :
                        string.Join("\n", x.ChildrenNames.Split(new[] { " | " }, StringSplitOptions.None)
                            .Zip(x.ChildrenGenders.Split(new[] { " | " }, StringSplitOptions.None), (n, g) => new { n, g })
                            .Zip(x.ChildrenAges.Split(new[] { " | " }, StringSplitOptions.None), (pair, a) =>
                                $"{pair.n} ({ (pair.g=="Male"?"ذكر":(pair.g=="Female"?"أنثى":pair.g)) }) - {a} سنة")
                        ))
                    }
                };

                // Filter Active Columns based on selection, preserving Order from View if possible
                var activeColumns = new List<(string Header, Func<LawyerReportRow, object> GetValue)>();
                foreach (var colKey in selectedColumns)
                {
                    if (columnsMap.ContainsKey(colKey))
                    {
                        activeColumns.Add(columnsMap[colKey]);
                    }
                }

                // 1. Draw Header
                for (int i = 0; i < activeColumns.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = activeColumns[i].Header;
                    ws.Cells[1, i + 1].Style.Font.Bold = true;
                    ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    ws.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // 2. Fill Data
                int rowIdx = 2;
                foreach (var item in data)
                {
                    for (int colIdx = 0; colIdx < activeColumns.Count; colIdx++)
                    {
                        var cell = ws.Cells[rowIdx, colIdx + 1];
                        cell.Value = activeColumns[colIdx].GetValue(item);
                        cell.Style.WrapText = true;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    rowIdx++;
                }

                // Formatting
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                for (int i = 1; i <= activeColumns.Count; i++)
                {
                    if (ws.Column(i).Width > 60) ws.Column(i).Width = 60;
                }
                ws.View.RightToLeft = true;

                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "التقرير_الشامل.xlsx");
            }
        }

        // =======================================================================
        //  LEGACY REPORTS (Original Code Preserved)
        // =======================================================================

        // 1. Lawyer Professional Status Report
        public async Task<ActionResult> LawyerProfessionalStatusReport()
        {
            ViewBag.Title = "تقرير إحصائي عن المحامين حسب الحالة المهنية";
            var statusData = await _context.Lawyers.GroupBy(l => l.ProfessionalStatus)
                .Select(g => new ProfessionalStatusCount { ProfessionalStatus = g.Key, Count = g.Count() })
                .OrderBy(s => s.ProfessionalStatus).ToListAsync();
            return View(new LawyerProfessionalStatusReportViewModel { StatusCounts = statusData, TotalLawyers = statusData.Sum(s => s.Count) });
        }

        public async Task<FileResult> ExportProfessionalStatusReportToExcel()
        {
            var statusData = await _context.Lawyers.GroupBy(l => l.ProfessionalStatus)
                .Select(g => new ProfessionalStatusCount { ProfessionalStatus = g.Key, Count = g.Count() }).OrderBy(s => s.ProfessionalStatus).ToListAsync();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير الحالة المهنية");
                ws.Cells[1, 1].Value = "الحالة المهنية"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in statusData) { ws.Cells[row, 1].Value = item.ProfessionalStatus; ws.Cells[row, 2].Value = item.Count; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_الحالة_المهنية.xlsx");
            }
        }

        // 2. Lawyer Gender Report
        public async Task<ActionResult> LawyerGenderReport()
        {
            ViewBag.Title = "تقرير إحصائي عن المحامين حسب الجنس";
            var genderData = await _context.Lawyers.GroupBy(l => l.Gender)
                .Select(g => new GenderCount { Gender = g.Key, Count = g.Count() }).OrderBy(s => s.Gender).ToListAsync();
            return View(new LawyerGenderReportViewModel { GenderCounts = genderData, TotalLawyers = genderData.Sum(s => s.Count) });
        }

        public async Task<FileResult> ExportGenderReportToExcel()
        {
            var genderData = await _context.Lawyers.GroupBy(l => l.Gender)
               .Select(g => new GenderCount { Gender = g.Key, Count = g.Count() }).ToListAsync();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير الجنس");
                ws.Cells[1, 1].Value = "الجنس"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in genderData) { ws.Cells[row, 1].Value = item.Gender; ws.Cells[row, 2].Value = item.Count; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_الجنس.xlsx");
            }
        }

        // 3. Lawyer Governorate Report
        public async Task<ActionResult> LawyerGovernorateReport(string type = "Current")
        {
            ViewBag.Title = type == "Current" ? "المحافظة الحالية" : "المحافظة الأصلية";
            IQueryable<Lawyer> query = _context.Lawyers.Include(l => l.PersonalDetails);
            var data = type == "Current" ?
                await query.Where(l => l.PersonalDetails.Any()).SelectMany(l => l.PersonalDetails).GroupBy(p => p.CurrentGovernorate).Select(g => new GovernorateCount { Governorate = g.Key, Count = g.Count() }).ToListAsync() :
                await query.Where(l => l.PersonalDetails.Any()).SelectMany(l => l.PersonalDetails).GroupBy(p => p.OriginalGovernorate).Select(g => new GovernorateCount { Governorate = g.Key, Count = g.Count() }).ToListAsync();
            return View(new LawyerGovernorateReportViewModel { GovernorateCounts = data, TotalLawyers = data.Sum(s => s.Count), ReportType = type });
        }

        public async Task<FileResult> ExportGovernorateReportToExcel(string type = "Current")
        {
            IQueryable<Lawyer> query = _context.Lawyers.Include(l => l.PersonalDetails);
            var data = type == "Current" ?
               await query.Where(l => l.PersonalDetails.Any()).SelectMany(l => l.PersonalDetails).GroupBy(p => p.CurrentGovernorate).Select(g => new GovernorateCount { Governorate = g.Key, Count = g.Count() }).ToListAsync() :
               await query.Where(l => l.PersonalDetails.Any()).SelectMany(l => l.PersonalDetails).GroupBy(p => p.OriginalGovernorate).Select(g => new GovernorateCount { Governorate = g.Key, Count = g.Count() }).ToListAsync();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير المحافظات");
                ws.Cells[1, 1].Value = "المحافظة"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in data) { ws.Cells[row, 1].Value = item.Governorate; ws.Cells[row, 2].Value = item.Count; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_المحافظات.xlsx");
            }
        }

        // 4. Lawyer Children Age Report
        public async Task<ActionResult> LawyerChildrenAgeReport()
        {
            ViewBag.Title = "تقرير إحصائي عن المحامين حسب عمر الأطفال";
            var childrenBirthDates = await _context.FamilyDetails.Where(fd => fd.HasChildren && fd.Children.Any())
                .SelectMany(fd => fd.Children).Select(c => c.DateOfBirth).Where(dob => dob.HasValue).ToListAsync();
            var ageRanges = new Dictionary<string, int> { { "0-5 سنوات", 0 }, { "6-12 سنة", 0 }, { "13-18 سنة", 0 }, { "19+ سنة", 0 } };
            foreach (var dob in childrenBirthDates)
            {
                int age = DateTime.Now.Year - dob.Value.Year;
                if (dob.Value.Date > DateTime.Now.AddYears(-age)) age--;
                if (age <= 5) ageRanges["0-5 سنوات"]++;
                else if (age <= 12) ageRanges["6-12 سنة"]++;
                else if (age <= 18) ageRanges["13-18 سنة"]++;
                else ageRanges["19+ سنة"]++;
            }
            return View(new LawyerChildrenAgeReportViewModel { AgeGroupCounts = ageRanges.Select(kv => new ChildAgeGroupCount { AgeGroup = kv.Key, Count = kv.Value }).ToList(), TotalChildren = childrenBirthDates.Count });
        }

        public async Task<FileResult> ExportChildrenAgeReportToExcel()
        {
            var childrenBirthDates = await _context.FamilyDetails.Where(fd => fd.HasChildren && fd.Children.Any())
               .SelectMany(fd => fd.Children).Select(c => c.DateOfBirth).Where(dob => dob.HasValue).ToListAsync();
            var ageRanges = new Dictionary<string, int> { { "0-5 سنوات", 0 }, { "6-12 سنة", 0 }, { "13-18 سنة", 0 }, { "19+ سنة", 0 } };
            foreach (var dob in childrenBirthDates)
            {
                int age = DateTime.Now.Year - dob.Value.Year;
                if (dob.Value.Date > DateTime.Now.AddYears(-age)) age--;
                if (age <= 5) ageRanges["0-5 سنوات"]++;
                else if (age <= 12) ageRanges["6-12 سنة"]++;
                else if (age <= 18) ageRanges["13-18 سنة"]++;
                else ageRanges["19+ سنة"]++;
            }
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير عمر الأطفال");
                ws.Cells[1, 1].Value = "الفئة"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in ageRanges) { ws.Cells[row, 1].Value = item.Key; ws.Cells[row, 2].Value = item.Value; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_عمر_الأطفال.xlsx");
            }
        }

        // 5. Lawyer Office Property Type Report
        public async Task<ActionResult> LawyerOfficePropertyTypeReport()
        {
            ViewBag.Title = "تقرير إحصائي عن المكاتب حسب نوع الملكية";
            var data = await _context.Lawyers.Where(l => l.OfficeDetails.Any()).SelectMany(l => l.OfficeDetails)
                .GroupBy(od => od.PropertyType).Select(g => new OfficePropertyTypeCount { PropertyType = g.Key, Count = g.Count() }).ToListAsync();
            return View(new LawyerOfficePropertyTypeReportViewModel { PropertyTypeCounts = data, TotalOffices = data.Sum(s => s.Count) });
        }

        public async Task<FileResult> ExportOfficePropertyTypeReportToExcel()
        {
            var data = await _context.Lawyers.Where(l => l.OfficeDetails.Any()).SelectMany(l => l.OfficeDetails)
                .GroupBy(od => od.PropertyType).Select(g => new OfficePropertyTypeCount { PropertyType = g.Key, Count = g.Count() }).ToListAsync();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير الملكية");
                ws.Cells[1, 1].Value = "النوع"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in data) { ws.Cells[row, 1].Value = item.PropertyType; ws.Cells[row, 2].Value = item.Count; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_الملكية.xlsx");
            }
        }

        // 6. Lawyer Office Damage Report
        public async Task<ActionResult> LawyerOfficeDamageReport()
        {
            ViewBag.Title = "تقرير إحصائي عن المكاتب حسب نوع الضرر";
            var data = await _context.Lawyers.Where(l => l.OfficeDamages.Any()).SelectMany(l => l.OfficeDamages)
                .GroupBy(od => od.DamageType).Select(g => new OfficeDamageCount { DamageType = g.Key, Count = g.Count() }).ToListAsync();
            return View(new LawyerOfficeDamageReportViewModel { OfficeDamageCounts = data, TotalDamagedOffices = data.Sum(s => s.Count) });
        }

        public async Task<FileResult> ExportOfficeDamageReportToExcel()
        {
            var data = await _context.Lawyers.Where(l => l.OfficeDamages.Any()).SelectMany(l => l.OfficeDamages)
                .GroupBy(od => od.DamageType).Select(g => new OfficeDamageCount { DamageType = g.Key, Count = g.Count() }).ToListAsync();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("تقرير الضرر");
                ws.Cells[1, 1].Value = "النوع"; ws.Cells[1, 2].Value = "العدد";
                int row = 2; foreach (var item in data) { ws.Cells[row, 1].Value = item.DamageType; ws.Cells[row, 2].Value = item.Count; row++; }
                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "تقرير_الضرر.xlsx");
            }
        }

        // 7. Lawyer Detailed Search Report (OLD)
        public async Task<ActionResult> LawyerDetailedSearchReport(LawyerDetailedReportViewModel model)
        {
            model.ProfessionalStatuses = GetProfessionalStatuses();
            model.Genders = GetGenders();
            model.Governorates = GetGovernorates();
            model.OfficePropertyTypes = GetPropertyTypes();
            model.OfficeDamageTypes = GetDamageTypes();
            model.MaritalStatuses = GetMaritalStatuses();
            model.YesNoOptions = GetYesNoOptions();
            model.HomeDamageTypes = GetDamageTypes();
            model.LawyerConditions = GetLawyerConditions();
            model.AidTypes = GetAidTypes();
            model.DetentionTypes = GetDetentionTypes();
            model.DetentionLocations = GetDetentionLocations();

            IQueryable<Lawyer> lawyersQuery = _context.Lawyers
                .Include(l => l.PersonalDetails).Include(l => l.FamilyDetails.Select(fd => fd.Children))
                .Include(l => l.FamilyDetails.Select(fd => fd.Spouses)).Include(l => l.HealthStatuses.Select(hs => hs.FamilyMemberInjuries))
                .Include(l => l.OfficeDetails.Select(od => od.Partners)).Include(l => l.HomeDamages).Include(l => l.OfficeDamages)
                .Include(l => l.DetentionDetails).Include(l => l.ColleagueInfos.Select(ci => ci.MartyrColleagues))
                .Include(l => l.ColleagueInfos.Select(ci => ci.DetainedColleagues)).Include(l => l.ColleagueInfos.Select(ci => ci.InjuredColleagues))
                .Include(l => l.GeneralInfos.Select(gi => gi.ReceivedAids)).Include(l => l.LawyerAttachments);

            if (!string.IsNullOrWhiteSpace(model.SelectedProfessionalStatus) && model.SelectedProfessionalStatus != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.ProfessionalStatus == model.SelectedProfessionalStatus);
            if (!string.IsNullOrWhiteSpace(model.SelectedGender) && model.SelectedGender != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.Gender == model.SelectedGender));
            if (!string.IsNullOrWhiteSpace(model.SelectedCurrentGovernorate) && model.SelectedCurrentGovernorate != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.CurrentGovernorate == model.SelectedCurrentGovernorate));
            if (!string.IsNullOrWhiteSpace(model.SelectedOriginalGovernorate) && model.SelectedOriginalGovernorate != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.OriginalGovernorate == model.SelectedOriginalGovernorate));
            if (!string.IsNullOrWhiteSpace(model.SelectedOfficePropertyType) && model.SelectedOfficePropertyType != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.OfficeDetails.Any(od => od.PropertyType == model.SelectedOfficePropertyType));
            if (!string.IsNullOrWhiteSpace(model.SelectedOfficeDamageType) && model.SelectedOfficeDamageType != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.OfficeDamages.Any(od => od.DamageType == model.SelectedOfficeDamageType));
            if (!string.IsNullOrWhiteSpace(model.SelectedMaritalStatus) && model.SelectedMaritalStatus != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.FamilyDetails.Any(fd => fd.MaritalStatus == model.SelectedMaritalStatus));
            if (model.SelectedWasDetained.HasValue)
                lawyersQuery = lawyersQuery.Where(l => l.DetentionDetails.Any(dd => dd.WasDetained == model.SelectedWasDetained.Value));
            if (model.SelectedHasHomeDamage.HasValue)
                lawyersQuery = lawyersQuery.Where(l => l.HomeDamages.Any(hd => hd.HasHomeDamage == model.SelectedHasHomeDamage.Value));
            if (!string.IsNullOrWhiteSpace(model.SelectedHomeDamageType) && model.SelectedHomeDamageType != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.HomeDamages.Any(hd => hd.DamageType == model.SelectedHomeDamageType));
            if (model.SelectedHasFamilyMembersInjured.HasValue)
                lawyersQuery = lawyersQuery.Where(l => l.HealthStatuses.Any(hs => hs.HasFamilyMembersInjured == model.SelectedHasFamilyMembersInjured.Value));
            if (!string.IsNullOrWhiteSpace(model.SelectedLawyerCondition) && model.SelectedLawyerCondition != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.HealthStatuses.Any(hs => hs.LawyerCondition == model.SelectedLawyerCondition));
            if (model.SelectedPracticesShariaLaw.HasValue)
                lawyersQuery = lawyersQuery.Where(l => l.GeneralInfos.Any(gi => gi.PracticesShariaLaw == model.SelectedPracticesShariaLaw.Value));
            if (!string.IsNullOrWhiteSpace(model.SelectedAidType) && model.SelectedAidType != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.GeneralInfos.Any(gi => gi.ReceivedAids.Any(ra => ra.AidType == model.SelectedAidType)));
            if (!string.IsNullOrWhiteSpace(model.SelectedDetentionType) && model.SelectedDetentionType != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.DetentionDetails.Any(dd => dd.DetentionType == model.SelectedDetentionType));
            if (!string.IsNullOrWhiteSpace(model.SelectedDetentionLocation) && model.SelectedDetentionLocation != "الكل")
                lawyersQuery = lawyersQuery.Where(l => l.DetentionDetails.Any(dd => dd.DetentionLocation == model.SelectedDetentionLocation));
            if (model.MinChildAge.HasValue || model.MaxChildAge.HasValue)
            {
                int currentYear = DateTime.Today.Year;
                int? targetMaxBirthYear = model.MinChildAge.HasValue ? (int?)(currentYear - model.MinChildAge.Value) : null;
                int? targetMinBirthYear = model.MaxChildAge.HasValue ? (int?)(currentYear - (model.MaxChildAge.Value + 1)) : null;
                lawyersQuery = lawyersQuery.Where(l => l.FamilyDetails.Any(fd => fd.Children.Any(c => c.DateOfBirth.HasValue && (!targetMinBirthYear.HasValue || c.DateOfBirth.Value.Year >= targetMinBirthYear.Value) && (!targetMaxBirthYear.HasValue || c.DateOfBirth.Value.Year <= targetMaxBirthYear.Value))));
            }

            model.Lawyers = await lawyersQuery.ToListAsync();

            if (model.ExportToExcel)
                return ExportLawyerDetailedReportToExcel(model);

            return View(model);
        }

        private ActionResult ExportLawyerDetailedReportToExcel(LawyerDetailedReportViewModel model)
        {
            if (model.Lawyers == null || !model.Lawyers.Any())
            {
                TempData["ErrorMessage"] = "لا توجد بيانات لتصديرها.";
                return RedirectToAction("LawyerDetailedSearchReport", model);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير المحامين المفصل");
                int col = 1;
                foreach (var columnOption in model.AvailableColumns.Where(c => model.SelectedColumnNames.Contains(c.Name))) { worksheet.Cells[1, col].Value = columnOption.DisplayName; col++; }
                using (var range = worksheet.Cells[1, 1, 1, col - 1]) { range.Style.Font.Bold = true; range.Style.Fill.PatternType = ExcelFillStyle.Solid; range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue); range.Style.Font.Color.SetColor(Color.Black); range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; }
                int row = 2;
                foreach (var lawyer in model.Lawyers) { col = 1; foreach (var columnOption in model.AvailableColumns.Where(c => model.SelectedColumnNames.Contains(c.Name))) { worksheet.Cells[row, col].Value = model.GetPropertyValue(lawyer, columnOption.Name); col++; } row++; }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var stream = new System.IO.MemoryStream(); package.SaveAs(stream); stream.Position = 0;
                string excelName = $"تقرير_المحامين_المفصل_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // ==========================================================================================
        //  Helper Methods (All)
        // ==========================================================================================

        private List<SelectListItem> GetGovernorates()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل المحافظات" },
                new SelectListItem { Value = "شمال غزة", Text = "شمال غزة" },
                new SelectListItem { Value = "مدينة غزة", Text = "مدينة غزة" },
                new SelectListItem { Value = "الوسطى", Text = "الوسطى" },
                new SelectListItem { Value = "خانيونس", Text = "خانيونس" },
                new SelectListItem { Value = "رفح", Text = "رفح" },
                new SelectListItem { Value = "الضفة الغربية", Text = "الضفة الغربية" },
                new SelectListItem { Value = "خارج البلاد", Text = "خارج البلاد" }
            };
        }

        private List<SelectListItem> GetExactProfessionalStatuses()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل الحالات" },
                new SelectListItem { Value = "مزاول", Text = "مزاول" },
                new SelectListItem { Value = "غير مزاول", Text = "غير مزاول" },
                new SelectListItem { Value = "متدرب", Text = "متدرب" },
                new SelectListItem { Value = "موقوف", Text = "موقوف" },
                new SelectListItem { Value = "مشطوب", Text = "مشطوب" },
                new SelectListItem { Value = "متقاعد", Text = "متقاعد" },
                new SelectListItem { Value = "متوفي", Text = "متوفي" },
                new SelectListItem { Value = "إلغاء إجازة المحاماة", Text = "إلغاء إجازة المحاماة" },
                new SelectListItem { Value = "غير مسدد للرسوم", Text = "غير مسدد للرسوم" },
                new SelectListItem { Value = "موظف", Text = "موظف" },
                new SelectListItem { Value = "مقيد", Text = "مقيد" },
                new SelectListItem { Value = "عليه تغيير مدرب", Text = "عليه تغيير مدرب" }
            };
        }

        private List<SelectListItem> GetProfessionalStatuses() => GetExactProfessionalStatuses(); // For old reports

        private List<SelectListItem> GetGenders()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "الكل" },
                new SelectListItem { Value = "Male", Text = "ذكر" },
                new SelectListItem { Value = "Female", Text = "أنثى" }
            };
        }

        private List<SelectListItem> GetMaritalStatuses()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "الكل" },
                new SelectListItem { Value = "Single", Text = "أعزب" },
                new SelectListItem { Value = "Married", Text = "متزوج" },
                new SelectListItem { Value = "Divorced", Text = "مطلق" },
                new SelectListItem { Value = "Widowed", Text = "أرمل" }
            };
        }

        private List<SelectListItem> GetPropertyTypes()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "Owned", Text = "مملوك" }, new SelectListItem { Value = "Rented", Text = "مستأجر" }, new SelectListItem { Value = "Shared", Text = "مشترك" } };
        }
        private List<SelectListItem> GetDamageTypes()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "Partial", Text = "جزئي" }, new SelectListItem { Value = "Total", Text = "كلي" }, new SelectListItem { Value = "Minor", Text = "طفيف" }, new SelectListItem { Value = "Major", Text = "كبير" } };
        }
        private List<SelectListItem> GetYesNoOptions()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "True", Text = "نعم" }, new SelectListItem { Value = "False", Text = "لا" } };
        }
        private static List<SelectListItem> GetLawyerConditions()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "Good", Text = "جيدة" }, new SelectListItem { Value = "Stable", Text = "مستقرة" }, new SelectListItem { Value = "Critical", Text = "حرجة" }, new SelectListItem { Value = "Deceased", Text = "متوفي" } };
        }
        private static List<SelectListItem> GetAidTypes()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "FinancialAid", Text = "مساعدة مالية" }, new SelectListItem { Value = "InKindAid", Text = "مساعدة عينية" }, new SelectListItem { Value = "LegalAid", Text = "مساعدة قانونية" }, new SelectListItem { Value = "Other", Text = "أخرى" } };
        }
        private static List<SelectListItem> GetDetentionTypes()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "AdministrativeDetention", Text = "اعتقال إداري" }, new SelectListItem { Value = "JudicialDetention", Text = "اعتقال قضائي" }, new SelectListItem { Value = "InvestigationDetention", Text = "اعتقال تحقيق" }, new SelectListItem { Value = "Other", Text = "آخر" } };
        }
        private static List<SelectListItem> GetDetentionLocations()
        {
            return new List<SelectListItem> { new SelectListItem { Value = "", Text = "الكل" }, new SelectListItem { Value = "OferPrison", Text = "سجن عوفر" }, new SelectListItem { Value = "MajdoPrison", Text = "سجن مجدو" }, new SelectListItem { Value = "Other", Text = "آخر" } };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _context.Dispose(); }
            base.Dispose(disposing);
        }
    }
}