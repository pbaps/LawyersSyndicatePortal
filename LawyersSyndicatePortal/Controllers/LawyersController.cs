using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using System.IO;
using OfficeOpenXml; // EPPlus library
using OfficeOpenXml.Style; // لتنسيق خلايا Excel
using System.Web; // لـ HttpPostedFileBase
using System.Threading.Tasks; // يجب أن يكون هذا موجودًا لدعم async/await
using System.Data.Entity.Validation; // لإضافة DbEntityValidationException
using System.Diagnostics; // لإضافة Debug.WriteLine
using LawyersSyndicatePortal.Filters;
using Microsoft.AspNet.Identity;

namespace LawyersSyndicatePortal.Controllers
{
    // تقييد الوصول لهذا الـ Controller للمسؤولين فقط
    // [PermissionAuthorizationFilter]  <-- تم نقل هذا الفلتر إلى الدوال الفردية ليتوافق مع "إجراء محمد"
    [Authorize(Roles = "Admin")]
    public class LawyersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LawyersController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Lawyers
        // يعرض قائمة بالمحامين مع دعم البحث والترقيم
        [PermissionAuthorizationFilter("عرض المحامين", "صلاحية عرض قائمة المحامين المتاحة")]
        [AuditLog("عرض قائمة المحامين", "عرض قائمة المحامين")]
        public async Task<ActionResult> Index(string searchString, int? page, int? pageSize)
        {
            // ... (الكود الأصلي للدالة) ...
            int currentPage = page ?? 1;
            int current_pageSize = pageSize ?? 10;

            var pageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" },
                new SelectListItem { Value = "100", Text = "100" }
            };

            ViewBag.PageSizes = pageSizes;
            ViewBag.PageSize = current_pageSize;

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            var lawyersQuery = from l in _context.Lawyers
                               select l;

            if (!string.IsNullOrEmpty(searchString))
            {
                lawyersQuery = lawyersQuery.Where(l => l.FullName.Contains(searchString) ||
                                                       l.IdNumber.Contains(searchString) ||
                                                       l.ProfessionalStatus.Contains(searchString) ||
                                                       l.MembershipNumber.Contains(searchString));
            }

            int totalRecords = await lawyersQuery.CountAsync();

            var lawyers = await lawyersQuery
                                 .OrderBy(l => l.FullName)
                                 .Skip((currentPage - 1) * current_pageSize)
                                 .Take(current_pageSize)
                                 .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize);

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentFilter = searchString;

            return View(lawyers);
        }

        // GET: Lawyers/Details/5
        // يعرض تفاصيل محامي معين
        [PermissionAuthorizationFilter("عرض تفاصيل المحامي", "صلاحية عرض تفاصيل محامي محدد")]
        [AuditLog("عرض تفاصيل المحامي", "عرض تفاصيل المحامي")]
        public async Task<ActionResult> Details(string id)
        {
            // ... (الكود الأصلي للدالة) ...
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var lawyer = await _context.Lawyers.Include(l => l.LinkedUsers)
                                               .FirstOrDefaultAsync(m => m.IdNumber == id);
            if (lawyer == null)
            {
                return HttpNotFound();
            }
            return View(lawyer);
        }

        // GET: Lawyers/Create
        // يعرض صفحة لإنشاء محامٍ جديد
        [PermissionAuthorizationFilter("إنشاء محامي جديد", "صلاحية عرض صفحة إنشاء محامي جديد")]
        [AuditLog("عرض صفحة إنشاء محامي", "عرض صفحة إنشاء محامي")]
        public ActionResult Create()
        {
            // ... (الكود الأصلي للدالة) ...
            PopulateProfessionalStatuses();
            return View();
        }

        // POST: Lawyers/Create
        // يستقبل بيانات المحامي الجديد ويضيفها إلى قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ محامي جديد", "صلاحية حفظ محامي جديد في قاعدة البيانات")]
        [AuditLog("انشاء محامي جديد", "انشاء محامي جديد")]
        public async Task<ActionResult> Create([Bind(Include = "IdNumber,FullName,ProfessionalStatus,MembershipNumber,TrainingStartDate,PracticeStartDate,Gender,IsActive,IsTrainee,TrainerLawyerName")] Lawyer lawyer)
        {
            // ... (الكود الأصلي للدالة) ...
            PopulateProfessionalStatuses();

            if (ModelState.IsValid)
            {
                if (await _context.Lawyers.AnyAsync(l => l.IdNumber == lawyer.IdNumber))
                {
                    ModelState.AddModelError("IdNumber", "رقم هوية المحامي هذا موجود بالفعل. يرجى إدخال رقم هوية فريد.");
                    return View(lawyer);
                }

                lawyer.IsTrainee = (lawyer.ProfessionalStatus == "متدرب");

                try
                {
                    _context.Lawyers.Add(lawyer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم إضافة المحامي بنجاح.";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    ModelState.AddModelError("", $"خطأ في التحقق من صحة البيانات: {fullErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"DbEntityValidationException: {fullErrorMessage}");
                }
                catch (DbUpdateException ex)
                {
                    var innerException = ex.InnerException?.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"خطأ في تحديث قاعدة البيانات: {innerException}");
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {innerException}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
                }
            }
            return View(lawyer);
        }

        // GET: Lawyers/Edit/5
        // يعرض صفحة لتعديل محامي موجود
        [PermissionAuthorizationFilter("تعديل محامي", "صلاحية عرض صفحة تعديل محامي موجود")]
        [AuditLog("عرض صفحة تعديل محامي", "عرض صفحة تعديل محامي")]
        public async Task<ActionResult> Edit(string id)
        {
            // ... (الكود الأصلي للدالة) ...
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var lawyer = await _context.Lawyers.FindAsync(id);
            if (lawyer == null)
            {
                return HttpNotFound();
            }

            PopulateProfessionalStatuses();
            return View(lawyer);
        }

        // POST: Lawyers/Edit/5
        // يستقبل بيانات المحامي المعدلة ويحدثها في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ تعديلات المحامي", "صلاحية حفظ التغييرات على محامي موجود")]
        [AuditLog("تعديل محامي", "تعديل محامي")]
        public async Task<ActionResult> Edit(string id, [Bind(Include = "IdNumber,FullName,ProfessionalStatus,MembershipNumber,TrainingStartDate,PracticeStartDate,Gender,IsActive,IsTrainee,TrainerLawyerName")] Lawyer lawyer)
        {
            // ... (الكود الأصلي للدالة) ...
            PopulateProfessionalStatuses();

            if (id != lawyer.IdNumber)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lawyer.IsTrainee = (lawyer.ProfessionalStatus == "متدرب");
                    _context.Entry(lawyer).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث بيانات المحامي بنجاح.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LawyerExists(lawyer.IdNumber))
                    {
                        return HttpNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    ModelState.AddModelError("", $"خطأ في التحقق من صحة البيانات: {fullErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"DbEntityValidationException: {fullErrorMessage}");
                }
                catch (DbUpdateException ex)
                {
                    var innerException = ex.InnerException?.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"خطأ في تحديث قاعدة البيانات: {innerException}");
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {innerException}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
                }
            }
            return View(lawyer);
        }

        // GET: Lawyers/Delete/5
        // يعرض صفحة لتأكيد حذف محامي
        [PermissionAuthorizationFilter("حذف محامي", "صلاحية عرض صفحة تأكيد حذف محامي")]
        [AuditLog("عرض صفحة تأكيد حذف محامي", "عرض صفحة تأكيد حذف محامي")]
        public async Task<ActionResult> Delete(string id)
        {
            // ... (الكود الأصلي للدالة) ...
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var lawyer = await _context.Lawyers.Include(l => l.LinkedUsers)
                                               .FirstOrDefaultAsync(m => m.IdNumber == id);
            if (lawyer == null)
            {
                return HttpNotFound();
            }
            return View(lawyer);
        }

        // POST: Lawyers/Delete/5
        // يحذف المحامي من قاعدة البيانات
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تأكيد حذف محامي", "صلاحية حذف محامي من قاعدة البيانات بشكل نهائي")]
        [AuditLog("حذف محامي", "حذف محامي")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            // ... (الكود الأصلي للدالة) ...
            try
            {
                var lawyer = await _context.Lawyers.FindAsync(id);
                if (lawyer == null)
                {
                    return HttpNotFound();
                }

                var linkedUsers = await _context.Users
                                                .Where(u => u.LinkedLawyerIdNumber == id)
                                                .ToListAsync();

                foreach (var user in linkedUsers)
                {
                    user.LinkedLawyerIdNumber = null;
                    _context.Entry(user).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();

                _context.Lawyers.Remove(lawyer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف المحامي بنجاح.";
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"خطأ في حذف المحامي من قاعدة البيانات: {innerException}";
                System.Diagnostics.Debug.WriteLine($"DbUpdateException (Delete): {innerException}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء حذف المحامي: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception (Delete): {ex.ToString()}");
            }

            return RedirectToAction("Index");
        }

        // ... (بقية الدوال المساعدة) ...

        // جديد: تصدير إلى Excel
        [PermissionAuthorizationFilter("تصدير بيانات المحامين", "صلاحية تصدير بيانات المحامين إلى ملف Excel")]
        [AuditLog("تصدير قائمة المحامين", "تصدير بيانات المحامين إلى ملف Excel")]
        public async Task<FileResult> ExportToExcel()
        {
            // ... (الكود الأصلي للدالة) ...
            var lawyers = await _context.Lawyers.ToListAsync();

            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("المحامون");

                string[] headers = {
                    "رقم الهوية",
                    "الاسم الكامل",
                    "الحالة المهنية",
                    "رقم العضوية",
                    "تاريخ بدء التدريب",
                    "تاريخ بدء المزاولة",
                    "الجنس",
                    "نشط",
                    "اسم المحامي المدرب"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[1, i + 1].Value = headers[i];
                }

                int row = 2;
                foreach (var lawyer in lawyers)
                {
                    ws.Cells[row, 1].Value = lawyer.IdNumber;
                    ws.Cells[row, 2].Value = lawyer.FullName;
                    ws.Cells[row, 3].Value = lawyer.ProfessionalStatus;
                    ws.Cells[row, 4].Value = lawyer.MembershipNumber;
                    ws.Cells[row, 5].Value = lawyer.TrainingStartDate?.ToOADate();
                    ws.Cells[row, 6].Value = lawyer.PracticeStartDate?.ToOADate();
                    ws.Cells[row, 7].Value = lawyer.Gender;
                    ws.Cells[row, 8].Value = lawyer.IsActive;
                    ws.Cells[row, 9].Value = lawyer.TrainerLawyerName;

                    ws.Cells[row, 5].Style.Numberformat.Format = "yyyy-MM-dd";
                    ws.Cells[row, 6].Style.Numberformat.Format = "yyyy-MM-dd";

                    row++;
                }

                using (ExcelRange rng = ws.Cells[1, 1, 1, headers.Length])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                ws.Cells.AutoFitColumns();

                byte[] fileBytes = pck.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Lawyers_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
        }

        // جديد: استيراد من Excel (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("استيراد بيانات المحامين", "صلاحية استيراد بيانات المحامين من ملف Excel")]
        [AuditLog("استيراد محامي من ملف", "استيراد بيانات المحامين من ملف Excel")]
        public async Task<ActionResult> ImportExcel(HttpPostedFileBase file)
        {
            // ... (الكود الأصلي للدالة) ...
            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "الرجاء تحديد ملف Excel للاستيراد.";
                return RedirectToAction("Index");
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "الرجاء تحميل ملف Excel بصيغة .xlsx فقط.";
                return RedirectToAction("Index");
            }

            List<Lawyer> newLawyersToInsert = new List<Lawyer>();
            List<string> errorMessages = new List<string>();
            HashSet<string> processedIdNumbersInBatch = new HashSet<string>();

            try
            {
                using (var stream = file.InputStream)
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "الملف لا يحتوي على أي ورقة عمل.";
                            return RedirectToAction("Index");
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        if (rowCount < 2)
                        {
                            TempData["ErrorMessage"] = "الملف فارغ أو يحتوي على رؤوس فقط.";
                            return RedirectToAction("Index");
                        }

                        var expectedHeaders = new Dictionary<string, int>
                        {
                            { "رقم الهوية", 1 },
                            { "الاسم الكامل", 2 },
                            { "الحالة المهنية", 3 },
                            { "رقم العضوية", 4 },
                            { "تاريخ بدء التدريب", 5 },
                            { "تاريخ بدء المزاولة", 6 },
                            { "الجنس", 7 },
                            { "نشط", 8 },
                            { "اسم المحامي المدرب", 9 }
                        };

                        bool headersValid = true;
                        foreach (var header in expectedHeaders)
                        {
                            if (worksheet.Cells[1, header.Value].Text.Trim() != header.Key)
                            {
                                errorMessages.Add($"العمود المتوقع '{header.Key}' غير موجود أو غير صحيح في العمود رقم {header.Value}.");
                                headersValid = false;
                            }
                        }

                        if (!headersValid)
                        {
                            TempData["ErrorMessage"] = "خطأ في رؤوس الأعمدة:<br/>" + string.Join("<br/>", errorMessages);
                            return RedirectToAction("Index");
                        }

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                string idNumber = worksheet.Cells[row, expectedHeaders["رقم الهوية"]].Text?.Trim();

                                if (string.IsNullOrEmpty(idNumber))
                                {
                                    errorMessages.Add($"الصف {row}: رقم الهوية مطلوب ولا يمكن أن يكون فارغًا.");
                                    continue;
                                }

                                if (processedIdNumbersInBatch.Contains(idNumber))
                                {
                                    errorMessages.Add($"الصف {row}: رقم الهوية '{idNumber}' مكرر في ملف Excel. سيتم تجاهل هذا السجل.");
                                    continue;
                                }

                                processedIdNumbersInBatch.Add(idNumber);

                                string fullName = worksheet.Cells[row, expectedHeaders["الاسم الكامل"]].Text?.Trim();
                                string professionalStatus = worksheet.Cells[row, expectedHeaders["الحالة المهنية"]].Text?.Trim();
                                string membershipNumber = worksheet.Cells[row, expectedHeaders["رقم العضوية"]].Text?.Trim();
                                string trainerLawyerName = worksheet.Cells[row, expectedHeaders["اسم المحامي المدرب"]].Text?.Trim();

                                DateTime? trainingStartDate = null;
                                if (worksheet.Cells[row, expectedHeaders["تاريخ بدء التدريب"]].Value is double oaDate1)
                                {
                                    trainingStartDate = DateTime.FromOADate(oaDate1);
                                }
                                else if (worksheet.Cells[row, expectedHeaders["تاريخ بدء التدريب"]].Value != null && DateTime.TryParse(worksheet.Cells[row, expectedHeaders["تاريخ بدء التدريب"]].Text, out DateTime parsedDate1))
                                {
                                    trainingStartDate = parsedDate1;
                                }

                                DateTime? practiceStartDate = null;
                                if (worksheet.Cells[row, expectedHeaders["تاريخ بدء المزاولة"]].Value is double oaDate2)
                                {
                                    practiceStartDate = DateTime.FromOADate(oaDate2);
                                }
                                else if (worksheet.Cells[row, expectedHeaders["تاريخ بدء المزاولة"]].Value != null && DateTime.TryParse(worksheet.Cells[row, expectedHeaders["تاريخ بدء المزاولة"]].Text, out DateTime parsedDate2))
                                {
                                    practiceStartDate = parsedDate2;
                                }

                                string gender = worksheet.Cells[row, expectedHeaders["الجنس"]].Text?.Trim();
                                bool isActive = false;
                                if (worksheet.Cells[row, expectedHeaders["نشط"]].Value != null)
                                {
                                    string isActiveText = worksheet.Cells[row, expectedHeaders["نشط"]].Text?.Trim().ToLower();
                                    if (isActiveText == "true" || isActiveText == "صحيح" || isActiveText == "نعم" || isActiveText == "1")
                                    {
                                        isActive = true;
                                    }
                                    else if (isActiveText == "false" || isActiveText == "خطأ" || isActiveText == "لا" || isActiveText == "0")
                                    {
                                        isActive = false;
                                    }
                                }

                                if (string.IsNullOrEmpty(fullName))
                                {
                                    errorMessages.Add($"الصف {row}: الاسم الكامل مطلوب.");
                                    continue;
                                }

                                var existingLawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == idNumber);
                                if (existingLawyer != null)
                                {
                                    existingLawyer.FullName = fullName;
                                    existingLawyer.ProfessionalStatus = professionalStatus;
                                    existingLawyer.MembershipNumber = membershipNumber;
                                    existingLawyer.TrainingStartDate = trainingStartDate;
                                    existingLawyer.PracticeStartDate = practiceStartDate;
                                    existingLawyer.Gender = gender;
                                    existingLawyer.IsActive = isActive;
                                    existingLawyer.IsTrainee = (professionalStatus == "متدرب");
                                    existingLawyer.TrainerLawyerName = trainerLawyerName;
                                }
                                else
                                {
                                    newLawyersToInsert.Add(new Lawyer
                                    {
                                        IdNumber = idNumber,
                                        FullName = fullName,
                                        ProfessionalStatus = professionalStatus,
                                        MembershipNumber = membershipNumber,
                                        TrainingStartDate = trainingStartDate,
                                        PracticeStartDate = practiceStartDate,
                                        Gender = gender,
                                        IsActive = isActive,
                                        IsTrainee = (professionalStatus == "متدرب"),
                                        TrainerLawyerName = trainerLawyerName
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                errorMessages.Add($"خطأ في قراءة الصف {row}: {ex.Message}");
                                System.Diagnostics.Debug.WriteLine($"Error reading row {row}: {ex.ToString()}");
                            }
                        }
                    }
                }

                if (newLawyersToInsert.Any())
                {
                    _context.Lawyers.AddRange(newLawyersToInsert);
                }

                await _context.SaveChangesAsync();

                if (errorMessages.Any())
                {
                    TempData["ErrorMessage"] = "تم استيراد البيانات مع بعض الأخطاء:<br/>" + string.Join("<br/>", errorMessages);
                }
                else
                {
                    TempData["SuccessMessage"] = "تم استيراد بيانات المحامين بنجاح.";
                }
            }
            catch (DbEntityValidationException ex)
            {
                var validationErrors = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", validationErrors);
                TempData["ErrorMessage"] = $"خطأ في التحقق من صحة البيانات أثناء الاستيراد: {fullErrorMessage}";
                System.Diagnostics.Debug.WriteLine($"DbEntityValidationException (Import): {fullErrorMessage}");
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"خطأ في تحديث قاعدة البيانات أثناء الاستيراد: {innerException}";
                System.Diagnostics.Debug.WriteLine($"DbUpdateException (Import): {innerException}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء الاستيراد: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception (Import): {ex.ToString()}");
            }

            return RedirectToAction("Index");
        }

        // جديد: تنزيل نموذج Excel
        [PermissionAuthorizationFilter("تنزيل نموذج استيراد", "صلاحية تنزيل نموذج ملف Excel لاستيراد بيانات المحامين")]
        [AuditLog("تنزيل نموذج المحامين", "تنزيل نموذج ملف Excel")]
        public FileResult DownloadExcelTemplate()
        {
            // ... (الكود الأصلي للدالة) ...
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("نموذج المحامين");

                string[] headers = {
                    "رقم الهوية",
                    "الاسم الكامل",
                    "الحالة المهنية",
                    "رقم العضوية",
                    "تاريخ بدء التدريب",
                    "تاريخ بدء المزاولة",
                    "الجنس",
                    "نشط",
                    "اسم المحامي المدرب"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[1, i + 1].Value = headers[i];
                }

                using (ExcelRange rng = ws.Cells[1, 1, 1, headers.Length])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                ws.Cells.AutoFitColumns();

                byte[] fileBytes = pck.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Lawyers_Template.xlsx");
            }
        }

        // ... (بقية الدوال المساعدة) ...

        private bool LawyerExists(string id)
        {
            return _context.Lawyers.Any(e => e.IdNumber == id);
        }

        private void PopulateProfessionalStatuses()
        {
            var professionalStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "مزاول", Text = "مزاول" },
                new SelectListItem { Value = "غير مزاول", Text = "غير مزاول" },
                new SelectListItem { Value = "متدرب", Text = "متدرب" },
                new SelectListItem { Value = "موقوف", Text = "موقوف" },
                new SelectListItem { Value = "مشطوب", Text = "مشطوب" },
                new SelectListItem { Value = "متقاعد", Text = "متقاعد" },
                new SelectListItem { Value = "متوفي", Text = "متوفي" }
            };
            ViewBag.ProfessionalStatuses = professionalStatuses;
        }

        // في ملف LawyersController.cs

        // ... (الكود الموجود سابقًا) ...

        // GET: Lawyers/ManagePermissions/5
        // يعرض صفحة لإدارة صلاحيات المستخدم المرتبط بالمحامي
        [PermissionAuthorizationFilter("إدارة صلاحيات المحامي", "صلاحية إدارة أدوار المستخدم المرتبط بالمحامي")]
        [AuditLog("عرض صفحة إدارة صلاحيات المحامي", "عرض صفحة لإدارة صلاحيات المحامي")]
        public async Task<ActionResult> ManagePermissions(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "رقم الهوية غير محدد.";
                return RedirectToAction("Index");
            }

            var lawyer = await _context.Lawyers.Include(l => l.LinkedUsers).FirstOrDefaultAsync(l => l.IdNumber == id);
            if (lawyer == null)
            {
                TempData["ErrorMessage"] = $"لم يتم العثور على المحامي برقم الهوية: {id}";
                return RedirectToAction("Index");
            }

            var user = lawyer.LinkedUsers.FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = $"لا يوجد مستخدم مرتبط بالمحامي {lawyer.FullName}.";
                return RedirectToAction("Details", new { id = lawyer.IdNumber });
            }

            // الخطوة 1: الحصول على UserManager
            var userManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(_context));

            // الخطوة 2: الحصول على الأدوار الحالية للمستخدم
            var userRoles = await userManager.GetRolesAsync(user.Id); // استخدام GetRolesAsync لضمان التزامن

            // الخطوة 3: الحصول على جميع الأدوار المتاحة
            var roleManager = new Microsoft.AspNet.Identity.RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(new Microsoft.AspNet.Identity.EntityFramework.RoleStore<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(_context));
            var allRoles = await roleManager.Roles.ToListAsync(); // استخدام ToListAsync لضمان التزامن

            // الخطوة 4: إنشاء قائمة SelectListItem مع تحديد الأدوار الحالية
            var rolesList = allRoles.Select(role => new SelectListItem
            {
                Value = role.Name,
                Text = role.Name,
                Selected = userRoles.Contains(role.Name) // هذا هو الجزء المهم
            }).ToList();

            ViewBag.Roles = rolesList;
            ViewBag.UserId = user.Id;

            return View(lawyer);
        }
        // POST: Lawyers/UpdatePermissions
        // تحديث صلاحيات المستخدم المرتبط بالمحامي
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تحديث صلاحيات المحامي", "صلاحية تحديث أدوار المستخدم المرتبط بالمحامي")]
        [AuditLog("تحديث صلاحيات المحامي", "تحديث صلاحيات المحامي")]
        public async Task<ActionResult> UpdatePermissions(string idNumber, string[] selectedRoles)
        {
            if (string.IsNullOrEmpty(idNumber))
            {
                TempData["ErrorMessage"] = "رقم الهوية غير محدد.";
                return RedirectToAction("Index");
            }

            var lawyer = await _context.Lawyers.Include(l => l.LinkedUsers).FirstOrDefaultAsync(l => l.IdNumber == idNumber);
            if (lawyer == null)
            {
                TempData["ErrorMessage"] = $"لم يتم العثور على المحامي برقم الهوية: {idNumber}";
                return RedirectToAction("Index");
            }

            var user = lawyer.LinkedUsers.FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = $"لا يوجد مستخدم مرتبط بالمحامي {lawyer.FullName}.";
                return RedirectToAction("Details", new { id = lawyer.IdNumber });
            }

            try
            {
                var userManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(_context));
                var currentRoles = userManager.GetRoles(user.Id).ToList();

                selectedRoles = selectedRoles ?? new string[] { };

                // إضافة الأدوار الجديدة التي لم تكن موجودة
                var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    await userManager.AddToRolesAsync(user.Id, rolesToAdd.ToArray());
                }

                // إزالة الأدوار القديمة التي لم يتم تحديدها
                var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
                if (rolesToRemove.Any())
                {
                    await userManager.RemoveFromRolesAsync(user.Id, rolesToRemove.ToArray());
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"تم تحديث صلاحيات المحامي {lawyer.FullName} بنجاح.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تحديث الصلاحيات: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error updating permissions: {ex.ToString()}");
            }

            return RedirectToAction("Details", new { id = lawyer.IdNumber });
        }

        // ... (بقية الكود الموجود سابقًا) ...

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