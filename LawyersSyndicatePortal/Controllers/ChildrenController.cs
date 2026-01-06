// LawyersSyndicatePortal\Controllers\ChildrenController.cs
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Collections.Generic;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Utilities;
using LawyersSyndicatePortal.Utility;



namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن عرض وإدارة بيانات أبناء المحامين.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ChildrenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChildrenController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Children
        /// <summary>
        /// يعرض قائمة بجميع الأبناء مع أعمارهم، مع دعم التصفية.
        /// </summary>
        [PermissionAuthorizationFilter("استعلام بيانات الأبناء", "صلاحية استعلام بيانات الأبناء")]
        [AuditLog("استعلام", "استعلام ابناء المحامين")]
        public async Task<ActionResult> Index(int? minAge, int? maxAge, DateTime? startDate, DateTime? endDate, string currentGovernorate, string originalGovernorate, string professionalStatus, string gender)
        {
            ViewBag.Title = "بيانات الأبناء";

            // تصحيح استعلام Include لضمان تحميل البيانات المرتبطة بشكل صحيح
            // لقد قمت بتغيير Lawyer.PersonalDetail إلى Lawyer.PersonalDetails لتتناسب مع نموذجك الجديد
            var childrenQuery = _context.Children
                .Include(c => c.FamilyDetail.Lawyer.PersonalDetails)
                .Where(c => c.DateOfBirth.HasValue);

            // تطبيق فلاتر العمر
            if (minAge.HasValue)
            {
                var minDateOfBirth = DateTime.Today.AddYears(-minAge.Value).AddDays(1);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= minDateOfBirth);
            }
            if (maxAge.HasValue)
            {
                var maxDateOfBirth = DateTime.Today.AddYears(-maxAge.Value);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= maxDateOfBirth);
            }

            // تطبيق فلاتر نطاق تاريخ الميلاد
            if (startDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= endDate.Value);
            }

            // تطبيق فلاتر البيانات الجديدة
            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault().CurrentGovernorate.Contains(currentGovernorate));
            }
            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault().OriginalGovernorate.Contains(originalGovernorate));
            }
            if (!string.IsNullOrEmpty(professionalStatus))
            {
                // تم تغيير هذا السطر ليستخدم الخاصية ProfessionalStatus الموجودة مباشرة في نموذج Lawyer
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.ProfessionalStatus.Contains(professionalStatus));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                childrenQuery = childrenQuery.Where(c => c.Gender == gender);
            }

            var childrenData = await childrenQuery.OrderBy(c => c.FamilyDetail.Lawyer.FullName).ToListAsync();

            // تحويل البيانات إلى ViewModel
            var viewModelList = childrenData.Select(child => new ChildReportViewModel
            {
                LawyerFullName = child.FamilyDetail.Lawyer.FullName,
                LawyerIdNumber = child.FamilyDetail.Lawyer.IdNumber,
                ChildName = child.ChildName,
                DateOfBirth = child.DateOfBirth.Value,
                Gender = child.Gender,
                // تم تصحيح الخطأ هنا باستخدام الخاصية الصحيحة
                ChildIdNumber = child.IdNumber,
                // تأكد من استخدام?. لتجنب الأخطاء في حال كانت البيانات غير موجودة
                CurrentGovernorate = child.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.CurrentGovernorate,
                OriginalGovernorate = child.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.OriginalGovernorate,
                ProfessionalStatus = child.FamilyDetail.Lawyer.ProfessionalStatus,
            }).ToList();

            // تمرير قيم التصفية الحالية وعدد النتائج إلى الـ View
            ViewBag.minAge = minAge;
            ViewBag.maxAge = maxAge;
            ViewBag.startDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.endDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.currentGovernorate = currentGovernorate;
            ViewBag.originalGovernorate = originalGovernorate;
            ViewBag.professionalStatus = professionalStatus;
            ViewBag.gender = gender;
            ViewBag.ResultCount = viewModelList.Count(); // تم تصحيح الخطأ هنا

            return View(viewModelList);
        }


        // GET: Children/ExportToExcel
        /// <summary>
        /// تصدير بيانات الأبناء المصفاة إلى ملف Excel.
        /// </summary>
        [PermissionAuthorizationFilter("تصدير بيانات الأبناء", "صلاحية تصدير بيانات الأبناء")]
        [AuditLog("تصدير", "تصدير بيانات ابناء المحامين")]
        public async Task<ActionResult> ExportToExcel(int? minAge, int? maxAge, DateTime? startDate, DateTime? endDate, string currentGovernorate, string originalGovernorate, string professionalStatus, string gender)
        {
            var childrenQuery = _context.Children
                .Include(c => c.FamilyDetail.Lawyer.PersonalDetails)
                .Where(c => c.DateOfBirth.HasValue);

            // تطبيق الفلاتر
            if (minAge.HasValue)
            {
                var minDateOfBirth = DateTime.Today.AddYears(-minAge.Value).AddDays(1);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= minDateOfBirth);
            }
            if (maxAge.HasValue)
            {
                var maxDateOfBirth = DateTime.Today.AddYears(-maxAge.Value);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= maxDateOfBirth);
            }
            if (startDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault().CurrentGovernorate.Contains(currentGovernorate));
            }
            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault().OriginalGovernorate.Contains(originalGovernorate));
            }
            if (!string.IsNullOrEmpty(professionalStatus))
            {
                // تم تصحيح الخطأ هنا
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.ProfessionalStatus.Contains(professionalStatus));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                childrenQuery = childrenQuery.Where(c => c.Gender == gender);
            }

            var childrenData = await childrenQuery.ToListAsync();

            var viewModelList = childrenData.Select(child => new ChildReportViewModel
            {
                LawyerFullName = child.FamilyDetail.Lawyer.FullName,
                LawyerIdNumber = child.FamilyDetail.Lawyer.IdNumber,
                ChildName = child.ChildName,
                DateOfBirth = child.DateOfBirth.Value,
                Gender = child.Gender,
                ChildIdNumber = child.IdNumber,
                CurrentGovernorate = child.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.CurrentGovernorate,
                OriginalGovernorate = child.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.OriginalGovernorate,
                ProfessionalStatus = child.FamilyDetail.Lawyer.ProfessionalStatus,
            }).ToList();

            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("بيانات الأبناء");

                // تعيين رؤوس الأعمدة باللغة العربية
                ws.Cells[1, 1].Value = "اسم المحامي";
                ws.Cells[1, 2].Value = "رقم هوية المحامي";
                ws.Cells[1, 3].Value = "اسم الابن/الابنة";
                ws.Cells[1, 4].Value = "تاريخ الميلاد";
                ws.Cells[1, 5].Value = "العمر (سنوات)";
                ws.Cells[1, 6].Value = "الجنس";
                ws.Cells[1, 7].Value = "رقم هوية الابن";
                ws.Cells[1, 8].Value = "محافظة التواجد حاليًا";
                ws.Cells[1, 9].Value = "المحافظة الأصلية";
                ws.Cells[1, 10].Value = "الحالة المهنية";

                // تنسيق رؤوس الأعمدة
                using (var range = ws.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // تعبئة البيانات في الجدول
                var row = 2;
                foreach (var child in viewModelList)
                {
                    ws.Cells[row, 1].Value = child.LawyerFullName;
                    ws.Cells[row, 2].Value = child.LawyerIdNumber;
                    ws.Cells[row, 3].Value = child.ChildName;
                    ws.Cells[row, 4].Value = child.DateOfBirth.ToString("yyyy-MM-dd");
                    ws.Cells[row, 5].Value = child.Age;
                    ws.Cells[row, 6].Value = child.Gender;
                    ws.Cells[row, 7].Value = child.ChildIdNumber;
                    ws.Cells[row, 8].Value = child.CurrentGovernorate;
                    ws.Cells[row, 9].Value = child.OriginalGovernorate;
                    ws.Cells[row, 10].Value = child.ProfessionalStatus;
                    row++;
                }

                // ضبط عرض الأعمدة تلقائياً وتنسيق النص ليكون يمينياً
                ws.Cells.AutoFitColumns();
                ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                var fileBytes = pck.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "بيانات_الأبناء.xlsx");
            }
        }

        /// <summary>
        /// A private method to apply all the filters to the children query.
        /// </summary>
        private IQueryable<Child> ApplyFilters(IQueryable<Child> childrenQuery, int? minAge, int? maxAge, DateTime? startDate, DateTime? endDate, string currentGovernorate, string originalGovernorate, string professionalStatus, string gender)
        {
            if (minAge.HasValue)
            {
                var minDateOfBirth = DateTime.Today.AddYears(-minAge.Value).AddDays(1);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= minDateOfBirth);
            }
            if (maxAge.HasValue)
            {
                var maxDateOfBirth = DateTime.Today.AddYears(-maxAge.Value);
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= maxDateOfBirth);
            }
            if (startDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                childrenQuery = childrenQuery.Where(c => c.DateOfBirth.Value <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                // Note: The use of FirstOrDefault() here can be inefficient. 
                // A better approach would be to check if any PersonalDetails match the criteria.
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.Any(p => p.CurrentGovernorate.Contains(currentGovernorate)));
            }
            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                // Note: The use of FirstOrDefault() here can be inefficient.
                // A better approach would be to check if any PersonalDetails match the criteria.
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.PersonalDetails.Any(p => p.OriginalGovernorate.Contains(originalGovernorate)));
            }
            if (!string.IsNullOrEmpty(professionalStatus))
            {
                childrenQuery = childrenQuery.Where(c => c.FamilyDetail.Lawyer.ProfessionalStatus.Contains(professionalStatus));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                childrenQuery = childrenQuery.Where(c => c.Gender == gender);
            }
            return childrenQuery;
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
