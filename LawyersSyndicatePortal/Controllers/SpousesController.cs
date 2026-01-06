// LawyersSyndicatePortal\Controllers\SpousesController.cs
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن عرض وإدارة بيانات زوجات المحامين.
    /// </summary>
    // تقييد الوصول لهذا المتحكم للمسؤولين فقط
    [Authorize(Roles = "Admin")]
    public class SpousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpousesController()
        {
            _context = new ApplicationDbContext();
            // ملاحظة: إذا كنت تستخدم EPPlus 5 أو أحدث، قد تحتاج إلى تعيين ترخيص غير تجاري
            // عن طريق إلغاء التعليق عن السطر التالي:
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: Spouses
        /// <summary>
        /// يعرض قائمة بجميع زوجات المحامين.
        /// </summary>
        /// <param name="lawyerIdNumber">رقم هوية المحامي للتصفية.</param>
        /// <param name="lawyerFullName">اسم المحامي للتصفية.</param>
        /// <param name="originalGovernorate">المحافظة الأصلية للتصفية.</param>
        /// <param name="currentGovernorate">محافظة التواجد حاليًا للتصفية.</param>
        /// <param name="professionalStatus">الحالة المهنية للتصفية.</param>
        [PermissionAuthorizationFilter("إدارة التقارير المتخصصة", "صلاحية عرض تقرير بيانات الزوجات")]
        public async Task<ActionResult> Index(string lawyerIdNumber, string lawyerFullName, string originalGovernorate, string currentGovernorate, string professionalStatus)
        {
            ViewBag.Title = "بيانات الزوجات";

            // جلب البيانات الأساسية للزوجات مع تضمين بيانات المحامي والتفاصيل الشخصية لتمكين البحث
            var spousesQuery = _context.FamilyDetails
                                       .Include(fd => fd.Lawyer.PersonalDetails)
                                       .SelectMany(fd => fd.Spouses)
                                       .AsQueryable();

            // تطبيق فلاتر البحث إذا كانت الحقول غير فارغة
            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.IdNumber.Contains(lawyerIdNumber));
            }

            if (!string.IsNullOrEmpty(lawyerFullName))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.FullName.Contains(lawyerFullName));
            }

            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.PersonalDetails.Any(pd => pd.OriginalGovernorate.Contains(originalGovernorate)));
            }

            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.PersonalDetails.Any(pd => pd.CurrentGovernorate.Contains(currentGovernorate)));
            }

            if (!string.IsNullOrEmpty(professionalStatus))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.ProfessionalStatus.Contains(professionalStatus));
            }

            var spousesData = await spousesQuery.OrderBy(s => s.FamilyDetail.Lawyer.IdNumber).ToListAsync();

            // تحويل البيانات إلى ViewModel
            var viewModelList = spousesData.Select(spouse => new SpouseReportViewModel
            {
                LawyerFullName = spouse.FamilyDetail.Lawyer.FullName,
                LawyerIdNumber = spouse.FamilyDetail.Lawyer.IdNumber,
                CurrentGovernorate = spouse.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.CurrentGovernorate,
                OriginalGovernorate = spouse.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.OriginalGovernorate,
                ProfessionalStatus = spouse.FamilyDetail.Lawyer.ProfessionalStatus,
                SpouseName = spouse.SpouseName,
                SpouseIdNumber = spouse.SpouseIdNumber,
                SpouseMobileNumber = spouse.SpouseMobileNumber
            }).ToList();

            // تمرير قيم التصفية الحالية وعدد النتائج إلى الـ View
            ViewBag.lawyerIdNumber = lawyerIdNumber;
            ViewBag.lawyerFullName = lawyerFullName;
            ViewBag.originalGovernorate = originalGovernorate;
            ViewBag.currentGovernorate = currentGovernorate;
            ViewBag.professionalStatus = professionalStatus;
            ViewBag.ResultsCount = viewModelList.Count;


            return View(viewModelList);
        }

        // GET: Spouses/ExportToExcel
        /// <summary>
        /// تصدير بيانات الزوجات المصفاة إلى ملف Excel.
        /// </summary>
        /// 
        [PermissionAuthorizationFilter("إدارة التقارير المتخصصة", "صلاحية تصدير تقرير بيانات الزوجات")]
        [AuditLog("تصدير", "تصدير تقرير بيانات الزوجات")]
        public async Task<ActionResult> ExportToExcel(string lawyerIdNumber, string lawyerFullName, string originalGovernorate, string currentGovernorate, string professionalStatus)
        {
            // نفس منطق الاستعلام المستخدم في الـ Index
            var spousesQuery = _context.FamilyDetails
                                       .Include(fd => fd.Lawyer.PersonalDetails)
                                       .SelectMany(fd => fd.Spouses);

            // تطبيق فلاتر البحث إذا كانت الحقول غير فارغة
            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.IdNumber.Contains(lawyerIdNumber));
            }

            if (!string.IsNullOrEmpty(lawyerFullName))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.FullName.Contains(lawyerFullName));
            }

            if (!string.IsNullOrEmpty(originalGovernorate))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.PersonalDetails.Any(pd => pd.OriginalGovernorate.Contains(originalGovernorate)));
            }

            if (!string.IsNullOrEmpty(currentGovernorate))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.PersonalDetails.Any(pd => pd.CurrentGovernorate.Contains(currentGovernorate)));
            }

            if (!string.IsNullOrEmpty(professionalStatus))
            {
                spousesQuery = spousesQuery.Where(s => s.FamilyDetail.Lawyer.ProfessionalStatus.Contains(professionalStatus));
            }

            var spousesData = await spousesQuery.ToListAsync();

            var viewModelList = spousesData.Select(spouse => new SpouseReportViewModel
            {
                LawyerFullName = spouse.FamilyDetail.Lawyer.FullName,
                LawyerIdNumber = spouse.FamilyDetail.Lawyer.IdNumber,
                CurrentGovernorate = spouse.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.CurrentGovernorate,
                OriginalGovernorate = spouse.FamilyDetail.Lawyer.PersonalDetails.FirstOrDefault()?.OriginalGovernorate,
                ProfessionalStatus = spouse.FamilyDetail.Lawyer.ProfessionalStatus,
                SpouseName = spouse.SpouseName,
                SpouseIdNumber = spouse.SpouseIdNumber,
                SpouseMobileNumber = spouse.SpouseMobileNumber
            }).ToList();

            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("بيانات الزوجات");

                // تعيين رؤوس الأعمدة باللغة العربية
                ws.Cells[1, 1].Value = "اسم المحامي";
                ws.Cells[1, 2].Value = "رقم هوية المحامي";
                ws.Cells[1, 3].Value = "محافظة التواجد حاليًا";
                ws.Cells[1, 4].Value = "المحافظة الأصلية";
                ws.Cells[1, 5].Value = "الحالة المهنية";
                ws.Cells[1, 6].Value = "اسم الزوجة";
                ws.Cells[1, 7].Value = "رقم هوية الزوجة";
                ws.Cells[1, 8].Value = "رقم جوال الزوجة";


                // تنسيق رؤوس الأعمدة
                using (var range = ws.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // تعبئة البيانات في الجدول
                var row = 2;
                foreach (var spouse in viewModelList)
                {
                    ws.Cells[row, 1].Value = spouse.LawyerFullName;
                    ws.Cells[row, 2].Value = spouse.LawyerIdNumber;
                    ws.Cells[row, 3].Value = spouse.CurrentGovernorate;
                    ws.Cells[row, 4].Value = spouse.OriginalGovernorate;
                    ws.Cells[row, 5].Value = spouse.ProfessionalStatus;
                    ws.Cells[row, 6].Value = spouse.SpouseName;
                    ws.Cells[row, 7].Value = spouse.SpouseIdNumber;
                    ws.Cells[row, 8].Value = spouse.SpouseMobileNumber;
                    row++;
                }

                // ضبط عرض الأعمدة تلقائياً
                ws.Cells.AutoFitColumns();
                ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                var fileBytes = pck.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "بيانات_الزوجات.xlsx");
            }
        }
        public async Task<ActionResult> FamilyDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // جلب تفاصيل العائلة مع تضمين الزوجات والأبناء
            // تأكد من استخدام Include() لجلب البيانات المرتبطة
            var familyDetail = await _context.FamilyDetails
                                             .Include(fd => fd.Spouses)
                                             .Include(fd => fd.Children)
                                             .FirstOrDefaultAsync(fd => fd.LawyerIdNumber == id);

            if (familyDetail == null)
            {
                // إذا لم يتم العثور على تفاصيل العائلة، يمكنك إرسال رسالة توضيحية.
                ViewBag.Message = "لا توجد تفاصيل عائلية مسجلة لهذا المحامي.";
                ViewBag.LawyerId = id; // للحفاظ على رقم هوية المحامي في العرض
                return View("FamilyDetails", new FamilyDetail()); // عرض الصفحة ببيانات فارغة
            }

            // إضافة اسم المحامي لعنوان الصفحة
            var lawyer = await _context.Lawyers.Include(l => l.PersonalDetails).FirstOrDefaultAsync(l => l.IdNumber == id);
            ViewBag.LawyerName = lawyer?.FullName ?? "غير معروف";
            ViewBag.CurrentGovernorate = lawyer?.PersonalDetails?.FirstOrDefault()?.CurrentGovernorate ?? "غير معروف";
            ViewBag.OriginalGovernorate = lawyer?.PersonalDetails?.FirstOrDefault()?.OriginalGovernorate ?? "غير معروف";
            ViewBag.ProfessionalStatus = lawyer?.ProfessionalStatus ?? "غير معروف";

            return View("FamilyDetails", familyDetail);
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
