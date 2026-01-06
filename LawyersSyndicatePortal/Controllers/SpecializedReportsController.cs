// LawyersSyndicatePortal\\Controllers\\SpecializedReportsController.cs
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
using static LawyersSyndicatePortal.ViewModels.NewLawyerReportViewModel;

namespace LawyersSyndicatePortal.Controllers
{
    public class LawyerData
    {
        // حقول المحامي الأساسية
        public string IdNumber { get; set; }
        public string FullName { get; set; }
        public string ProfessionalStatus { get; set; }
        public DateTime? PracticeDate { get; set; }
        public string MembershipNumber { get; set; }
        public DateTime? TrainingStartDate { get; set; }
        public string TrainerLawyerName { get; set; }

        // حقول FamilyDetails
        public string MaritalStatus { get; set; }
        public int? NumberOfSpouses { get; set; }
        public bool? HasChildren { get; set; }
        public int? NumberOfChildren { get; set; }

        // حقول PersonalDetails
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public string OriginalGovernorate { get; set; }
        public string CurrentGovernorate { get; set; }
        public string AccommodationType { get; set; }
        public string FullAddress { get; set; }
        public string MobileNumber { get; set; }
        public string AltMobileNumber1 { get; set; }
        public string AltMobileNumber2 { get; set; }
        public string WhatsAppNumber { get; set; }
        public string LandlineNumber { get; set; }
    }

    /// <summary>
    /// المتحكم المسؤول عن التقارير والاستعلامات المتخصصة للمحامين.
    /// يسمح للمستخدمين بالبحث الديناميكي واختيار الأعمدة وتصدير النتائج إلى Excel.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SpecializedReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecializedReportsController()
        {
            _context = new ApplicationDbContext();
            // Note: If you are using EPPlus 5 or later, you may need to set a non-commercial license
            // by uncommenting the following line:
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: SpecializedReports/Index
        // Displa/s the search and column selection page
        [PermissionAuthorizationFilter("إدارة التقارير المتخصصة", "صلاحية عرض تقرير المحامين")]
        public ActionResult Index()
        {
            var model = new NewLawyerReportViewModel();

            // إنشاء قائمة الخيارات للمحافظات
            model.GovernorateList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل المحافظات" },
                new SelectListItem { Value = "متوفر", Text = "متوفر" }, // خيار جديد
                new SelectListItem { Value = "غير متوفر", Text = "غير متوفر" }, // خيار جديد
                new SelectListItem { Value = "شمال غزة", Text = "شمال غزة" },
                new SelectListItem { Value = "مدينة غزة", Text = "مدينة غزة" },
                new SelectListItem { Value = "الوسطى", Text = "الوسطى" },
                new SelectListItem { Value = "خانيونس", Text = "خانيونس" },
                new SelectListItem { Value = "رفح", Text = "رفح" },
                new SelectListItem { Value = "الضفة الغربية", Text = "الضفة الغربية" },
                new SelectListItem { Value = "خارج البلاد", Text = "خارج البلاد" }
            };

            // إنشاء قائمة الخيارات للحالات المهنية
            model.ProfessionalStatuses = new List<SelectListItem>
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
                new SelectListItem { Value = "فارغ", Text = "فارغ" },
                new SelectListItem { Value = "مزاول 2021", Text = "مزاول 2021" },
                new SelectListItem { Value = "مسدد 2022", Text = "مسدد 2022" },
                new SelectListItem { Value = "موظف", Text = "موظف" },
                new SelectListItem { Value = "مقيد", Text = "مقيد" },
                new SelectListItem { Value = "عليه تغيير مدرب", Text = "عليه تغيير مدرب" }
            };

            // إضافة قوائم الخيارات الجديدة
            model.MaritalStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل الحالات" },
                new SelectListItem { Value = "Single", Text = "أعزب/عزباء" },
                new SelectListItem { Value = "Married", Text = "متزوج/متزوجة" },
                new SelectListItem { Value = "Divorced", Text = "مطلق/مطلقة" },
                new SelectListItem { Value = "Widowed", Text = "أرمل/أرملة" }
            };

            model.Genders = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "الكل" },
                new SelectListItem { Value = "ذكر", Text = "ذكر" },
                new SelectListItem { Value = "أنثى", Text = "أنثى" }
            };

            model.HasChildrenOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "لا يهم" },
                new SelectListItem { Value = "true", Text = "نعم" },
                new SelectListItem { Value = "false", Text = "لا" }
            };

            // إضافة خيارات الأعمدة المتاحة
            model.AvailableColumns = new List<ReportColumn>
            {
                // حقول المحامي الأساسية
                new ReportColumn { ColumnName = "IdNumber", DisplayName = "رقم الهوية", IsSelected = true },
                new ReportColumn { ColumnName = "FullName", DisplayName = "الاسم الكامل", IsSelected = true },
                new ReportColumn { ColumnName = "ProfessionalStatus", DisplayName = "الحالة المهنية", IsSelected = true },
                new ReportColumn { ColumnName = "PracticeDate", DisplayName = "تاريخ المزاولة", IsSelected = true },
                new ReportColumn { ColumnName = "MembershipNumber", DisplayName = "رقم العضوية", IsSelected = true },
                // يجب إضافة خاصية 'TrainingStartDate' إلى نموذج Lawyer.cs
                new ReportColumn { ColumnName = "TrainingStartDate", DisplayName = "تاريخ بداية التدريب", IsSelected = true },
                new ReportColumn { ColumnName = "TrainerLawyerName", DisplayName = "اسم المحامي المدرب", IsSelected = true },

                // حقول تفاصيل العائلة
                new ReportColumn { ColumnName = "MaritalStatus", DisplayName = "الحالة الاجتماعية", IsSelected = true },
                new ReportColumn { ColumnName = "NumberOfSpouses", DisplayName = "عدد الزوجات", IsSelected = true },
                new ReportColumn { ColumnName = "HasChildren", DisplayName = "هل يوجد أبناء؟", IsSelected = true },
                new ReportColumn { ColumnName = "NumberOfChildren", DisplayName = "عدد الأبناء", IsSelected = true },

                // حقول التفاصيل الشخصية
                new ReportColumn { ColumnName = "Gender", DisplayName = "الجنس", IsSelected = true },
                new ReportColumn { ColumnName = "EmailAddress", DisplayName = "عنوان البريد الالكتروني", IsSelected = true },
                new ReportColumn { ColumnName = "OriginalGovernorate", DisplayName = "المحافظة الأصلية", IsSelected = true },
                new ReportColumn { ColumnName = "CurrentGovernorate", DisplayName = "محافظة التواجد حاليًا", IsSelected = true },
                new ReportColumn { ColumnName = "AccommodationType", DisplayName = "طبيعة السكن حاليًا", IsSelected = true },
                new ReportColumn { ColumnName = "FullAddress", DisplayName = "العنوان كامل", IsSelected = true },
                new ReportColumn { ColumnName = "MobileNumber", DisplayName = "رقم الجوال", IsSelected = true },
                new ReportColumn { ColumnName = "AltMobileNumber1", DisplayName = "رقم جوال احتياطي 1", IsSelected = true },
                new ReportColumn { ColumnName = "AltMobileNumber2", DisplayName = "رقم جوال احتياطي 2", IsSelected = true },
                new ReportColumn { ColumnName = "WhatsAppNumber", DisplayName = "رقم الواتساب", IsSelected = true },
                new ReportColumn { ColumnName = "LandlineNumber", DisplayName = "رقم الهاتف الأرضي", IsSelected = true },
            };

            return View(model);
        }

        [HttpPost]
        [PermissionAuthorizationFilter("إدارة التقارير المتخصصة", "صلاحية عرض تقرير المحامين")]
        public async Task<ActionResult> Search(NewLawyerReportViewModel model)
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

            if (!string.IsNullOrWhiteSpace(model.ProfessionalStatus))
            {
                query = query.Where(l => l.ProfessionalStatus == model.ProfessionalStatus);
            }

            if (!string.IsNullOrWhiteSpace(model.MaritalStatus))
            {
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.MaritalStatus == model.MaritalStatus));
            }

            if (!string.IsNullOrWhiteSpace(model.Gender))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.Gender == model.Gender));
            }

            if (model.HasChildren.HasValue)
            {
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.HasChildren == model.HasChildren.Value));
            }

            if (!string.IsNullOrWhiteSpace(model.OriginalGovernorate))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.OriginalGovernorate == model.OriginalGovernorate));
            }

            // تعديل منطق البحث عن محافظة التواجد الحالية
            if (!string.IsNullOrWhiteSpace(model.CurrentGovernorate))
            {
                if (model.CurrentGovernorate == "غير متوفر")
                {
                    // ابحث عن السجلات التي حقل CurrentGovernorate فيها فارغ
                    query = query.Where(l => !l.PersonalDetails.Any() || string.IsNullOrEmpty(l.PersonalDetails.FirstOrDefault().CurrentGovernorate));
                }
                else if (model.CurrentGovernorate == "متوفر")
                {
                    // ابحث عن السجلات التي حقل CurrentGovernorate فيها يحتوي على قيمة
                    query = query.Where(l => l.PersonalDetails.Any(pd => !string.IsNullOrEmpty(pd.CurrentGovernorate)));
                }
                else
                {
                    // البحث عن محافظة معينة
                    query = query.Where(l => l.PersonalDetails.Any(pd => pd.CurrentGovernorate == model.CurrentGovernorate));
                }
            }

            // تضمين بيانات الجداول المرتبطة
            query = query.Include(l => l.PersonalDetails)
                         .Include(l => l.FamilyDetails.Select(fd => fd.Children))
                         .Include(l => l.FamilyDetails.Select(fd => fd.Spouses));

            var lawyers = await query.ToListAsync();

            var resultModel = new NewLawyerReportResultViewModel
            {
                Lawyers = lawyers,
                // تأكد من أن الأعمدة المختارة تم تمريرها بشكل صحيح
                SelectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList()
            };

            return PartialView("LawyerDetailedSearchReport", resultModel);
        }

        [HttpPost]
        [PermissionAuthorizationFilter("إدارة التقارير المتخصصة", "صلاحية تصدير تقرير المحامين")]
        [AuditLog("تصدير", "تصدير تقرير المحامين")]
        public async Task<ActionResult> ExportToExcel(NewLawyerReportViewModel model)
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
            if (!string.IsNullOrWhiteSpace(model.ProfessionalStatus))
            {
                query = query.Where(l => l.ProfessionalStatus == model.ProfessionalStatus);
            }
            if (!string.IsNullOrWhiteSpace(model.MaritalStatus))
            {
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.MaritalStatus == model.MaritalStatus));
            }
            if (!string.IsNullOrWhiteSpace(model.Gender))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.Gender == model.Gender));
            }
            if (model.HasChildren.HasValue)
            {
                query = query.Where(l => l.FamilyDetails.Any(fd => fd.HasChildren == model.HasChildren.Value));
            }
            if (!string.IsNullOrWhiteSpace(model.OriginalGovernorate))
            {
                query = query.Where(l => l.PersonalDetails.Any(pd => pd.OriginalGovernorate == model.OriginalGovernorate));
            }
            if (!string.IsNullOrWhiteSpace(model.CurrentGovernorate))
            {
                if (model.CurrentGovernorate == "غير متوفر")
                {
                    query = query.Where(l => !l.PersonalDetails.Any() || string.IsNullOrEmpty(l.PersonalDetails.FirstOrDefault().CurrentGovernorate));
                }
                else if (model.CurrentGovernorate == "متوفر")
                {
                    query = query.Where(l => l.PersonalDetails.Any(pd => !string.IsNullOrEmpty(pd.CurrentGovernorate)));
                }
                else
                {
                    query = query.Where(l => l.PersonalDetails.Any(pd => pd.CurrentGovernorate == model.CurrentGovernorate));
                }
            }

            var lawyerDataList = await query.Select(l => new LawyerData
            {
                // حقول Lawyer
                IdNumber = l.IdNumber,
                FullName = l.FullName,
                ProfessionalStatus = l.ProfessionalStatus,
                PracticeDate = l.PracticeStartDate, // هذا هو السطر الذي كان يسبب الخطأ
                MembershipNumber = l.MembershipNumber,
                TrainingStartDate = l.TrainingStartDate,
                TrainerLawyerName = l.TrainerLawyerName,

                // حقول PersonalDetails
                Gender = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().Gender : null,
                EmailAddress = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().EmailAddress : null,
                OriginalGovernorate = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().OriginalGovernorate : null,
                CurrentGovernorate = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().CurrentGovernorate : null,
                AccommodationType = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().AccommodationType : null,
                FullAddress = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().FullAddress : null,
                MobileNumber = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().MobileNumber : null,
                AltMobileNumber1 = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().AltMobileNumber1 : null,
                AltMobileNumber2 = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().AltMobileNumber2 : null,
                WhatsAppNumber = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().WhatsAppNumber : null,
                LandlineNumber = l.PersonalDetails.Any() ? l.PersonalDetails.FirstOrDefault().LandlineNumber : null,

                // حقول FamilyDetails
                MaritalStatus = l.FamilyDetails.Any() ? l.FamilyDetails.FirstOrDefault().MaritalStatus : null,
                NumberOfSpouses = l.FamilyDetails.Any() ? l.FamilyDetails.FirstOrDefault().NumberOfSpouses : (int?)null,
                HasChildren = l.FamilyDetails.Any() ? l.FamilyDetails.FirstOrDefault().HasChildren : (bool?)null,
                NumberOfChildren = l.FamilyDetails.Any() ? l.FamilyDetails.FirstOrDefault().NumberOfChildren : (int?)null
            }).ToListAsync();

            var selectedColumns = model.AvailableColumns.Where(c => c.IsSelected).ToList();

            if (!lawyerDataList.Any() || !selectedColumns.Any())
            {
                TempData["ErrorMessage"] = "لا توجد بيانات للتصدير أو لم يتم اختيار أي أعمدة.";
                return RedirectToAction("Index");
            }

            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("تقرير المحامين");

                // Header
                for (int i = 0; i < selectedColumns.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = selectedColumns[i].DisplayName;
                }

                // Styling the header
                using (var rng = ws.Cells[1, 1, 1, selectedColumns.Count])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data rows
                int row = 2;
                foreach (var lawyerData in lawyerDataList.OrderBy(l => l.FullName))
                {
                    int col = 1;
                    foreach (var column in selectedColumns)
                    {
                        var value = GetPropertyValueFromLawyerData(lawyerData, column.ColumnName);

                        if (value is bool boolValue)
                        {
                            ws.Cells[row, col].Value = boolValue ? "نعم" : "لا";
                        }
                        else
                        {
                            ws.Cells[row, col].Value = value?.ToString();
                        }
                        col++;
                    }
                    row++;
                }

                ws.Cells.AutoFitColumns();

                byte[] fileBytes = pck.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"LawyerReport_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        private object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyPath))
            {
                return null;
            }

            var parts = propertyPath.Split('.');
            object currentObject = obj;

            foreach (var part in parts)
            {
                if (currentObject == null)
                {
                    return null;
                }

                var propertyInfo = currentObject.GetType().GetProperty(part);

                if (propertyInfo == null)
                {
                    return null;
                }
                if (typeof(System.Collections.ICollection).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var collection = propertyInfo.GetValue(currentObject, null) as System.Collections.ICollection;
                    if (collection == null || collection.Count == 0)
                    {
                        return null;
                    }

                    currentObject = collection.Cast<object>().FirstOrDefault();
                }
                else
                {
                    currentObject = propertyInfo.GetValue(currentObject, null);
                }
            }

            if (currentObject == null)
            {
                return null;
            }

            if (currentObject is bool boolValue)
            {
                return boolValue ? "نعم" : "لا";
            }

            if (currentObject is DateTime dateValue)
            {
                return dateValue.ToShortDateString();
            }

            // ✅ This is the corrected syntax for nullable DateTime
            if (currentObject is DateTime?)
            {
                DateTime? nullableDateValue = (DateTime?)currentObject;
                return nullableDateValue?.ToShortDateString();
            }

            return currentObject.ToString();
        }

        private object GetPropertyValueFromLawyerData(LawyerData data, string propertyPath)
        {
            if (data == null || string.IsNullOrWhiteSpace(propertyPath))
            {
                return null;
            }

            var propertyInfo = typeof(LawyerData).GetProperty(propertyPath);

            if (propertyInfo == null)
            {
                // إذا كان propertyPath هو PracticeDate، فيجب أن نبحث عن PracticeStartDate
                if (propertyPath == "PracticeDate")
                {
                    propertyInfo = typeof(LawyerData).GetProperty("PracticeStartDate");
                }
                else
                {
                    return null;
                }
            }

            var value = propertyInfo.GetValue(data, null);

            if (value is bool boolValue)
            {
                return boolValue ? "نعم" : "لا";
            }

            if (value is DateTime dateValue)
            {
                return dateValue.ToShortDateString();
            }

            // ✅ This is the corrected syntax for nullable DateTime
            if (value is DateTime?)
            {
                DateTime? nullableDateValue = (DateTime?)value;
                return nullableDateValue?.ToShortDateString();
            }

            return value?.ToString();
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