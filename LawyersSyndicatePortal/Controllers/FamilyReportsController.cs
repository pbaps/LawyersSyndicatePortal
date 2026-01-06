using ClosedXML.Excel;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FamilyReportsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// يعرض تقرير عائلات المحامين مع إمكانية التصفية.
        /// </summary>
        /// <param name="lawyerIdNumber">فلتر على رقم هوية المحامي.</param>
        /// <param name="lawyerFullName">فلتر على الاسم الكامل للمحامي.</param>
        /// <param name="originalGovernorate">فلتر على المحافظة الأصلية.</param>
        /// <param name="currentGovernorate">فلتر على محافظة التواجد حاليًا.</param>
        /// <param name="mobileNumber">فلتر على رقم الجوال.</param>
        /// <param name="professionalStatus">فلتر على الحالة المهنية.</param>
        /// <returns>عرض HTML للتقرير.</returns>
        [PermissionAuthorizationFilter("تقرير العائلات", "يسمح بالوصول إلى تقارير العائلات")]
        [AuditLog("تقرير العائلات", "استعلامات العائلة")]
        public ActionResult Index(string lawyerIdNumber, string lawyerFullName, string originalGovernorate, string currentGovernorate, string mobileNumber, string professionalStatus)
        {
            var viewModel = new FamilyReportViewModel
            {
                LawyerIdNumberFilter = lawyerIdNumber,
                LawyerFullNameFilter = lawyerFullName,
                OriginalGovernorateFilter = originalGovernorate,
                CurrentGovernorateFilter = currentGovernorate,
                MobileNumberFilter = mobileNumber,
                ProfessionalStatusFilter = professionalStatus
            };

            // استخدام الدالة المساعدة لجلب البيانات المفلترة
            var filteredLawyers = GetFilteredLawyersQuery(lawyerIdNumber, lawyerFullName, originalGovernorate, currentGovernorate, mobileNumber, professionalStatus).ToList();

            // ملء بيانات الزوجات والأبناء في ViewModel
            var allSpouses = new List<SpouseReportViewModel>();
            var allChildren = new List<ChildReportViewModel>();

            foreach (var lawyer in filteredLawyers)
            {
                var personalDetail = lawyer.PersonalDetails?.FirstOrDefault();
                var professionalStatusValue = lawyer.ProfessionalStatus;

                foreach (var familyDetail in lawyer.FamilyDetails)
                {
                    foreach (var spouse in familyDetail.Spouses)
                    {
                        allSpouses.Add(new SpouseReportViewModel
                        {
                            LawyerIdNumber = lawyer.IdNumber,
                            LawyerFullName = lawyer.FullName,
                            SpouseName = spouse.SpouseName,
                            SpouseIdNumber = spouse.SpouseIdNumber,
                            SpouseMobileNumber = spouse.SpouseMobileNumber,
                            CurrentGovernorate = personalDetail?.CurrentGovernorate,
                            OriginalGovernorate = personalDetail?.OriginalGovernorate,
                            ProfessionalStatus = professionalStatusValue
                        });
                    }

                    foreach (var child in familyDetail.Children)
                    {
                        allChildren.Add(new ChildReportViewModel
                        {
                            LawyerIdNumber = lawyer.IdNumber,
                            LawyerFullName = lawyer.FullName,
                            ChildName = child.ChildName,
                            ChildIdNumber = child.IdNumber,
                            Gender = child.Gender,
                            DateOfBirth = child.DateOfBirth.GetValueOrDefault(),
                            CurrentGovernorate = personalDetail?.CurrentGovernorate,
                            OriginalGovernorate = personalDetail?.OriginalGovernorate,
                            ProfessionalStatus = professionalStatusValue
                        });
                    }
                }
            }
            viewModel.Spouses = allSpouses;
            viewModel.Children = allChildren;

            return View(viewModel);
        }

        /// <summary>
        /// تصدير التقرير إلى ملف Excel.
        /// </summary>
        /// <param name="lawyerIdNumber">فلتر على رقم هوية المحامي.</param>
        /// <param name="lawyerFullName">فلتر على الاسم الكامل للمحامي.</param>
        /// <param name="originalGovernorate">فلتر على المحافظة الأصلية.</param>
        /// <param name="currentGovernorate">فلتر على محافظة التواجد حاليًا.</param>
        /// <param name="mobileNumber">فلتر على رقم الجوال.</param>
        /// <param name="professionalStatus">فلتر على الحالة المهنية.</param>
        /// <returns>ملف Excel.</returns>
        [PermissionAuthorizationFilter("تصدير تقرير العائلات", "صلاحية تصدير تقرير العائلات إلى ملف Excel")]
        [AuditLog("تصدير تقرير العائلات", "تصدير تقرير العائلات إلى إكسل")]
        public ActionResult ExportToExcel(string lawyerIdNumber, string lawyerFullName, string originalGovernorate, string currentGovernorate, string mobileNumber, string professionalStatus)
        {
            // استخدام الدالة المساعدة لجلب البيانات المفلترة
            var filteredLawyers = GetFilteredLawyersQuery(lawyerIdNumber, lawyerFullName, originalGovernorate, currentGovernorate, mobileNumber, professionalStatus).ToList();

            var allSpouses = new List<SpouseReportViewModel>();
            var allChildren = new List<ChildReportViewModel>();

            foreach (var lawyer in filteredLawyers)
            {
                var personalDetail = lawyer.PersonalDetails?.FirstOrDefault();
                var professionalStatusValue = lawyer.ProfessionalStatus;

                foreach (var familyDetail in lawyer.FamilyDetails)
                {
                    foreach (var spouse in familyDetail.Spouses)
                    {
                        allSpouses.Add(new SpouseReportViewModel
                        {
                            LawyerIdNumber = lawyer.IdNumber,
                            LawyerFullName = lawyer.FullName,
                            SpouseName = spouse.SpouseName,
                            SpouseIdNumber = spouse.SpouseIdNumber,
                            SpouseMobileNumber = spouse.SpouseMobileNumber,
                            CurrentGovernorate = personalDetail?.CurrentGovernorate,
                            OriginalGovernorate = personalDetail?.OriginalGovernorate,
                            ProfessionalStatus = professionalStatusValue
                        });
                    }

                    foreach (var child in familyDetail.Children)
                    {
                        allChildren.Add(new ChildReportViewModel
                        {
                            LawyerIdNumber = lawyer.IdNumber,
                            LawyerFullName = lawyer.FullName,
                            ChildName = child.ChildName,
                            ChildIdNumber = child.IdNumber,
                            Gender = child.Gender,
                            DateOfBirth = child.DateOfBirth.GetValueOrDefault(),
                            CurrentGovernorate = personalDetail?.CurrentGovernorate,
                            OriginalGovernorate = personalDetail?.OriginalGovernorate,
                            ProfessionalStatus = professionalStatusValue
                        });
                    }
                }
            }

            using (var workbook = new XLWorkbook())
            {
                if (allSpouses.Any())
                {
                    var spousesWorksheet = workbook.Worksheets.Add("بيانات الزوجات/الأزواج");
                    spousesWorksheet.RightToLeft = true;
                    var spouseTable = spousesWorksheet.Cell(1, 1).InsertTable(allSpouses);
                    spouseTable.Theme = XLTableTheme.TableStyleMedium1;
                }

                if (allChildren.Any())
                {
                    var childrenWorksheet = workbook.Worksheets.Add("بيانات الأبناء");
                    childrenWorksheet.RightToLeft = true;
                    var childrenTable = childrenWorksheet.Cell(1, 1).InsertTable(allChildren);
                    childrenTable.Theme = XLTableTheme.TableStyleMedium1;
                }

                var lawyersWithNoFamily = filteredLawyers.Where(l => !l.FamilyDetails.Any()).ToList();
                if (lawyersWithNoFamily.Any())
                {
                    var noFamilyData = lawyersWithNoFamily.Select(l => new
                    {
                        LawyerIdNumber = l.IdNumber,
                        LawyerFullName = l.FullName,
                        ProfessionalStatus = l.ProfessionalStatus,
                        CurrentGovernorate = l.PersonalDetails?.FirstOrDefault()?.CurrentGovernorate,
                        OriginalGovernorate = l.PersonalDetails?.FirstOrDefault()?.OriginalGovernorate
                    }).ToList();

                    var noFamilyWorksheet = workbook.Worksheets.Add("محامون بدون بيانات عائلة");
                    noFamilyWorksheet.RightToLeft = true;
                    var noFamilyTable = noFamilyWorksheet.Cell(1, 1).InsertTable(noFamilyData);
                    noFamilyTable.Theme = XLTableTheme.TableStyleMedium1;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FamilyReport.xlsx");
                }
            }
        }

        /// <summary>
        /// دالة مساعدة لإنشاء استعلام قاعدة البيانات مع جميع عوامل التصفية.
        /// </summary>
        /// <param name="lawyerIdNumber">فلتر على رقم هوية المحامي.</param>
        /// <param name="lawyerFullName">فلتر على الاسم الكامل للمحامي.</param>
        /// <param name="originalGovernorate">فلتر على المحافظة الأصلية.</param>
        /// <param name="currentGovernorate">فلتر على محافظة التواجد حاليًا.</param>
        /// <param name="mobileNumber">فلتر على رقم الجوال.</param>
        /// <param name="professionalStatus">فلتر على الحالة المهنية.</param>
        /// <returns>استعلام IQueryable من المحامين.</returns>
        private IQueryable<Lawyer> GetFilteredLawyersQuery(string lawyerIdNumber, string lawyerFullName, string originalGovernorate, string currentGovernorate, string mobileNumber, string professionalStatus)
        {
            var lawyersQuery = db.Lawyers
                .Include(l => l.PersonalDetails)
                .Include(l => l.FamilyDetails.Select(fd => fd.Spouses))
                .Include(l => l.FamilyDetails.Select(fd => fd.Children))
                .AsQueryable();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                lawyersQuery = lawyersQuery.Where(l => l.IdNumber == lawyerIdNumber);
            }

            if (!string.IsNullOrEmpty(lawyerFullName))
            {
                lawyersQuery = lawyersQuery.Where(l => l.FullName.Contains(lawyerFullName));
            }

            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.OriginalGovernorate == originalGovernorate));
            }

            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.CurrentGovernorate == currentGovernorate));
            }

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                lawyersQuery = lawyersQuery.Where(l => l.PersonalDetails.Any(pd => pd.MobileNumber == mobileNumber));
            }

            if (!string.IsNullOrEmpty(professionalStatus))
            {
                lawyersQuery = lawyersQuery.Where(l => l.ProfessionalStatus == professionalStatus);
            }

            return lawyersQuery;
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
