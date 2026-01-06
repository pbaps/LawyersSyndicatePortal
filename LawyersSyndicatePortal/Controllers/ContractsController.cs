using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Contracts
        [PermissionAuthorizationFilter("عرض العقود", "صلاحية عرض صفحة العقود")]
        [AuditLog("عرض", "عرض صفحة العقود")]
        public ActionResult Index()
        {
            var contracts = db.PbaContracts.Include(c => c.Lawyer);
            return View(contracts.ToList());
        }

        // GET: Contracts/Details/5
        [PermissionAuthorizationFilter("عرض تفاصيل العقد", "صلاحية عرض تفاصيل العقد")]
        [AuditLog("عرض", "عرض تفاصيل العقد")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PbaContract contract = db.PbaContracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            return View(contract);
        }

        // GET: Contracts/Create
        [PermissionAuthorizationFilter("إنشاء عقد جديد", "صلاحية عرض صفحة إنشاء عقد")]
        [AuditLog("عرض", "عرض صفحة إنشاء عقد جديد")]
        public ActionResult Create()
        {
            var contract = new PbaContract();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            var user = userManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                contract.EmployeeName = user.FullName;
            }

            // تم إزالة قيمة تاريخ المصادقة لضمان أنها فارغة في النموذج الجديد
            contract.ContractAuthenticationDate = DateTime.Now;

            ViewBag.LawyerId = new SelectList(db.Lawyers, "IdNumber", "FullName");
            return View(contract);
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إنشاء عقد جديد", "صلاحية إنشاء عقد جديد")]
        [AuditLog("إنشاء", "إنشاء عقد جديد")]
        public ActionResult Create([Bind(Include = "ContractId,LawyerId,ReceiptNumber,FileName,LawyerName,LawyerMembershipNumber,DeedType,FirstPartyName,FirstPartyId,SecondPartyName,SecondPartyId,ContractAuthenticationDate,AuthenticatorName,SyndicateBranch,EmployeeName")] PbaContract contract)
        {
            if (ModelState.IsValid)
            {
                // CORRECTED: يتم تعيين اسم الموظف المدخل من الاسم الكامل المخزن في المستخدم
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var user = userManager.FindById(User.Identity.GetUserId());
                if (user != null)
                {
                    contract.EmployeeName = user.FullName;
                }

                // NEW: التحقق من قيمة تاريخ المصادقة قبل الحفظ
                // إذا كانت القيمة تساوي القيمة الافتراضية، يتم تعيينها إلى null لتجنب خطأ النطاق
                if (contract.ContractAuthenticationDate == DateTime.MinValue)
                {
                    // هذا سيؤدي إلى خطأ إذا كان حقل ContractAuthenticationDate في قاعدة البيانات غير قابل للقيم الفارغة
                    // الحل الأفضل هو إما: 1. جعل الحقل في قاعدة البيانات nullable (يمكن أن يكون فارغًا)
                    // أو 2. توفير قيمة افتراضية صالحة (مثل DateTime.Now)
                    // بما أن حقل التاريخ مطلوب عادة، دعنا نتأكد من أنه يجب إدخاله
                    ModelState.AddModelError("ContractAuthenticationDate", "تاريخ المصادقة مطلوب.");
                    ViewBag.LawyerId = new SelectList(db.Lawyers, "IdNumber", "FullName", contract.LawyerId);
                    return View(contract);
                }

                db.PbaContracts.Add(contract);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LawyerId = new SelectList(db.Lawyers, "IdNumber", "FullName", contract.LawyerId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        [PermissionAuthorizationFilter("تعديل عقد", "صلاحية عرض صفحة تعديل عقد")]
        [AuditLog("عرض", "عرض صفحة تعديل عقد")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PbaContract contract = db.PbaContracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            ViewBag.LawyerId = new SelectList(db.Lawyers, "IdNumber", "FullName", contract.LawyerId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        // تمت إزالة EmployeeName من قائمة الحقول المراد ربطها
        // هذا يضمن أن قيمة EmployeeName الأصلية في قاعدة البيانات لن تتغير
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تعديل عقد", "صلاحية تعديل عقد")]
        [AuditLog("تعديل", "تعديل عقد")]
        public ActionResult Edit([Bind(Include = "ContractId,LawyerId,ReceiptNumber,FileName,LawyerName,LawyerMembershipNumber,DeedType,FirstPartyName,FirstPartyId,SecondPartyName,SecondPartyId,ContractAuthenticationDate,AuthenticatorName,SyndicateBranch")] PbaContract contract)
        {
            if (ModelState.IsValid)
            {
                // يجب جلب الكائن الأصلي من قاعدة البيانات أولاً للحفاظ على جميع القيم
                // لكن في هذه الحالة، إزالة الحقل من Bind(Include) هي أبسط طريقة
                // db.Entry(contract).State = EntityState.Modified;
                // db.SaveChanges();
                // يمكن استخدام الطريقة التالية ليكون التحديث أكثر دقة:
                var existingContract = db.PbaContracts.Find(contract.ContractId);

                // تحديث القيم التي تم إرسالها فقط
                existingContract.LawyerId = contract.LawyerId;
                existingContract.ReceiptNumber = contract.ReceiptNumber;
                existingContract.FileName = contract.FileName;
                existingContract.LawyerName = contract.LawyerName;
                existingContract.LawyerMembershipNumber = contract.LawyerMembershipNumber;
                existingContract.DeedType = contract.DeedType;
                existingContract.FirstPartyName = contract.FirstPartyName;
                existingContract.FirstPartyId = contract.FirstPartyId;
                existingContract.SecondPartyName = contract.SecondPartyName;
                existingContract.SecondPartyId = contract.SecondPartyId;
                existingContract.ContractAuthenticationDate = contract.ContractAuthenticationDate;
                existingContract.AuthenticatorName = contract.AuthenticatorName;
                existingContract.SyndicateBranch = contract.SyndicateBranch;
                // ملاحظة: حقل EmployeeName لم يتم تضمينه في التحديث

                db.Entry(existingContract).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.LawyerId = new SelectList(db.Lawyers, "IdNumber", "FullName", contract.LawyerId);
            return View(contract);
        }
        // GET: Contracts/Delete/5
        [PermissionAuthorizationFilter("حذف عقد", "صلاحية عرض صفحة حذف عقد")]
        [AuditLog("عرض", "عرض صفحة حذف عقد")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PbaContract contract = db.PbaContracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تاكيد حذف عقد", "صلاحية حذف عقد")]
        [AuditLog("حذف", "حذف عقد")]
        public ActionResult DeleteConfirmed(int id)
        {
            PbaContract contract = db.PbaContracts.Find(id);
            db.PbaContracts.Remove(contract);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Contracts/ExportToExcel
        [PermissionAuthorizationFilter("تصدير العقود", "صلاحية تصدير بيانات العقود إلى Excel")]
        [AuditLog("تصدير", "تصدير بيانات العقود إلى Excel")]
        public ActionResult ExportToExcel()
        {
            var contracts = db.PbaContracts.ToList();

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Contracts");

            // إضافة العناوين الجديدة
            ws.Cells["A1"].Value = "اسم الموظف";
            ws.Cells["B1"].Value = "رقم الإيصال";
            ws.Cells["C1"].Value = "اسم الملف";
            ws.Cells["D1"].Value = "اسم المحامي";
            ws.Cells["E1"].Value = "رقم عضوية المحامي";
            ws.Cells["F1"].Value = "نوع السند";
            ws.Cells["G1"].Value = "اسم الطرف الأول";
            ws.Cells["H1"].Value = "هوية الطرف الأول";
            ws.Cells["I1"].Value = "اسم الطرف الثاني";
            ws.Cells["J1"].Value = "هوية الطرف الثاني";
            ws.Cells["K1"].Value = "تاريخ المصادقة";
            ws.Cells["L1"].Value = "اسم المصادق";
            ws.Cells["M1"].Value = "فرع النقابة";

            // إضافة البيانات
            int row = 2;
            foreach (var contract in contracts)
            {
                ws.Cells[string.Format("A{0}", row)].Value = contract.EmployeeName;
                ws.Cells[string.Format("B{0}", row)].Value = contract.ReceiptNumber;
                ws.Cells[string.Format("C{0}", row)].Value = contract.FileName;
                ws.Cells[string.Format("D{0}", row)].Value = contract.LawyerName;
                ws.Cells[string.Format("E{0}", row)].Value = contract.LawyerMembershipNumber;
                ws.Cells[string.Format("F{0}", row)].Value = contract.DeedType;
                ws.Cells[string.Format("G{0}", row)].Value = contract.FirstPartyName;
                ws.Cells[string.Format("H{0}", row)].Value = contract.FirstPartyId;
                ws.Cells[string.Format("I{0}", row)].Value = contract.SecondPartyName;
                ws.Cells[string.Format("J{0}", row)].Value = contract.SecondPartyId;
                ws.Cells[string.Format("K{0}", row)].Value = contract.ContractAuthenticationDate.ToShortDateString();
                ws.Cells[string.Format("L{0}", row)].Value = contract.AuthenticatorName;
                ws.Cells[string.Format("M{0}", row)].Value = contract.SyndicateBranch;
                row++;
            }

            ws.Cells["A:M"].AutoFitColumns();

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=Contracts.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();

            return View();
        }



        // GET: Contracts/ImportFromExcel
        [PermissionAuthorizationFilter("استيراد العقود", "صلاحية عرض صفحة استيراد العقود")]
        [AuditLog("عرض", "عرض صفحة استيراد العقود")]
        public ActionResult ImportFromExcel()
        {
            return View();
        }

        // POST: Contracts/ImportFromExcel
        [HttpPost]
        [PermissionAuthorizationFilter("استيراد العقود", "صلاحية استيراد بيانات العقود من Excel")]
        [AuditLog("استيراد", "استيراد بيانات العقود من Excel")]
        public ActionResult ImportFromExcel(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    var package = new ExcelPackage(file.InputStream);
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var contract = new PbaContract();

                        // NEW: استخدام DateTime.TryParse للتحقق من صحة القيمة قبل التحويل
                        DateTime tempDate;
                        if (DateTime.TryParse(worksheet.Cells[row, 10].Value?.ToString(), out tempDate))
                        {
                            contract.ContractAuthenticationDate = tempDate;
                        }
                        else
                        {
                            // التعامل مع القيم الفارغة أو غير الصالحة: يمكن تعيين قيمة افتراضية صالحة أو تخطي السطر
                            // هنا نقوم بتعيين قيمة اليوم
                            contract.ContractAuthenticationDate = DateTime.Now;
                        }

                        contract.EmployeeName = User.Identity.GetUserName();
                        contract.ReceiptNumber = worksheet.Cells[row, 1].Value?.ToString().Trim();
                        contract.FileName = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        contract.LawyerName = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        contract.LawyerMembershipNumber = worksheet.Cells[row, 4].Value?.ToString().Trim();
                        contract.DeedType = worksheet.Cells[row, 5].Value?.ToString().Trim();
                        contract.FirstPartyName = worksheet.Cells[row, 6].Value?.ToString().Trim();
                        contract.FirstPartyId = worksheet.Cells[row, 7].Value?.ToString().Trim();
                        contract.SecondPartyName = worksheet.Cells[row, 8].Value?.ToString().Trim();
                        contract.SecondPartyId = worksheet.Cells[row, 9].Value?.ToString().Trim();
                        contract.AuthenticatorName = worksheet.Cells[row, 11].Value?.ToString().Trim();
                        contract.SyndicateBranch = worksheet.Cells[row, 12].Value?.ToString().Trim();

                        db.PbaContracts.Add(contract);
                    }
                    db.SaveChanges();
                    ViewBag.Message = "تم استيراد البيانات بنجاح!";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "حدث خطأ أثناء الاستيراد: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "الرجاء تحديد ملف Excel لتحميله.";
            }

            return View("ImportFromExcel");
        }

        // GET: Contracts/Search
        [PermissionAuthorizationFilter("بحث في العقود", "صلاحية البحث في العقود")]
        [AuditLog("بحث", "البحث في العقود")]
        public ActionResult Search(string employeeName, string receiptNumber, string fileName, string lawyerName, string lawyerMembershipNumber, string deedType, string firstPartyName, string secondPartyName, DateTime? contractAuthenticationDateFrom, DateTime? contractAuthenticationDateTo, string authenticatorName, string syndicateBranch)
        {
            // بناء استعلام LINQ ديناميكي
            var contracts = db.PbaContracts.Include(c => c.Lawyer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(employeeName))
            {
                contracts = contracts.Where(c => c.EmployeeName.Contains(employeeName));
            }
            if (!string.IsNullOrWhiteSpace(receiptNumber))
            {
                contracts = contracts.Where(c => c.ReceiptNumber.Contains(receiptNumber));
            }
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                contracts = contracts.Where(c => c.FileName.Contains(fileName));
            }
            if (!string.IsNullOrWhiteSpace(lawyerName))
            {
                contracts = contracts.Where(c => c.LawyerName.Contains(lawyerName));
            }
            if (!string.IsNullOrWhiteSpace(lawyerMembershipNumber))
            {
                contracts = contracts.Where(c => c.LawyerMembershipNumber.Contains(lawyerMembershipNumber));
            }
            if (!string.IsNullOrWhiteSpace(deedType))
            {
                contracts = contracts.Where(c => c.DeedType.Contains(deedType));
            }
            if (!string.IsNullOrWhiteSpace(firstPartyName))
            {
                contracts = contracts.Where(c => c.FirstPartyName.Contains(firstPartyName));
            }
            if (!string.IsNullOrWhiteSpace(secondPartyName))
            {
                contracts = contracts.Where(c => c.SecondPartyName.Contains(secondPartyName));
            }
            if (contractAuthenticationDateFrom.HasValue)
            {
                contracts = contracts.Where(c => c.ContractAuthenticationDate >= contractAuthenticationDateFrom.Value);
            }
            if (contractAuthenticationDateTo.HasValue)
            {
                contracts = contracts.Where(c => c.ContractAuthenticationDate <= contractAuthenticationDateTo.Value);
            }
            if (!string.IsNullOrWhiteSpace(authenticatorName))
            {
                contracts = contracts.Where(c => c.AuthenticatorName.Contains(authenticatorName));
            }
            if (!string.IsNullOrWhiteSpace(syndicateBranch))
            {
                contracts = contracts.Where(c => c.SyndicateBranch.Contains(syndicateBranch));
            }

            // يتم إرسال نتائج البحث إلى View
            return View("Index", contracts.ToList());
        }

        // POST: Contracts/SearchLawyers
        // تقوم هذه الطريقة بالبحث عن المحامين في قاعدة البيانات وإرجاع النتائج بتنسيق JSON.
        // تمكننا هذه الطريقة من استخدام AJAX للبحث دون الحاجة لإعادة تحميل الصفحة.
        [HttpPost]
        public JsonResult SearchLawyers(string searchTerm)
        {
            // يتم استخدام AsQueryable لتمكين الاستعلامات الديناميكية
            var query = db.Lawyers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l => l.FullName.Contains(searchTerm) || l.MembershipNumber.Contains(searchTerm));
            }

            // يتم اختيار الحقول المطلوبة فقط لتخفيف حجم البيانات
            var lawyers = query.Select(l => new {
                fullName = l.FullName,
                membershipNumber = l.MembershipNumber,
                idNumber = l.IdNumber
            }).ToList();

            return Json(lawyers);
        }

        public ActionResult DownloadImportTemplate()
        {
            // يمكنك هنا إضافة الحقول التي تريدها في النموذج بنفس ترتيب الجدول
            var headers = new List<string>
    {
        "EmployeeName",
        "ReceiptNumber",
        "FileName",
        "LawyerName",
        "DeedType",
        "FirstPartyName",
        "SecondPartyName",
        "ContractAuthenticationDate",
        "SyndicateBranch"
    };

            // هذا مجرد مثال على كيفية استخدام EPPlus لإنشاء الملف
            // يجب أن تكون المكتبة مثبتة في مشروعك
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Import Template");

                // إضافة الرؤوس إلى الصف الأول
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // إعداد استجابة الملف للتنزيل
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ContractImportTemplate.xlsx");
            }
        }

        // هذه الدالة تعرض صفحة البحث والنتائج
        // GET: Contracts/SearchContracts
        // هذه الدالة تعرض صفحة البحث والنتائج
        [PermissionAuthorizationFilter("بحث في العقود للمحامين", "صلاحية البحث في العقود للمحامين")]
        [AuditLog("بحث عقود للمحامين", "البحث في العقود للمحامين")]
        public ActionResult SearchContracts(string employeeName, string receiptNumber, string lawyerName,
                                            string deedType, string firstPartyName, string secondPartyName,
                                            DateTime? contractAuthenticationDateFrom, DateTime? contractAuthenticationDateTo,
                                            string syndicateBranch)
        {
            // تحقق مما إذا كان هناك أي حقول بحث مدخلة
            bool isSearching = !string.IsNullOrEmpty(employeeName) ||
                               !string.IsNullOrEmpty(receiptNumber) ||
                               !string.IsNullOrEmpty(lawyerName) ||
                               !string.IsNullOrEmpty(deedType) ||
                               !string.IsNullOrEmpty(firstPartyName) ||
                               !string.IsNullOrEmpty(secondPartyName) ||
                               contractAuthenticationDateFrom.HasValue ||
                               contractAuthenticationDateTo.HasValue ||
                               !string.IsNullOrEmpty(syndicateBranch);

            if (!isSearching)
            {
                // إذا لم يتم إدخال أي حقول بحث، أرجع قائمة فارغة
                // هذا يمنع عرض الجدول قبل عملية البحث الأولى
                return View(new List<PbaContract>());
            }

            // استعلام قاعدة البيانات للحصول على كل العقود
            var contracts = db.PbaContracts.AsQueryable();

            // تطبيق عوامل التصفية (Filters) بناءً على الحقول التي تم إدخالها في نموذج البحث
            if (!string.IsNullOrEmpty(employeeName))
            {
                contracts = contracts.Where(c => c.EmployeeName.Contains(employeeName));
            }

            if (!string.IsNullOrEmpty(receiptNumber))
            {
                contracts = contracts.Where(c => c.ReceiptNumber.Contains(receiptNumber));
            }

            if (!string.IsNullOrEmpty(lawyerName))
            {
                contracts = contracts.Where(c => c.LawyerName.Contains(lawyerName));
            }

            if (!string.IsNullOrEmpty(deedType))
            {
                contracts = contracts.Where(c => c.DeedType.Contains(deedType));
            }

            if (!string.IsNullOrEmpty(firstPartyName))
            {
                contracts = contracts.Where(c => c.FirstPartyName.Contains(firstPartyName));
            }

            if (!string.IsNullOrEmpty(secondPartyName))
            {
                contracts = contracts.Where(c => c.SecondPartyName.Contains(secondPartyName));
            }

            if (contractAuthenticationDateFrom.HasValue)
            {
                contracts = contracts.Where(c => c.ContractAuthenticationDate >= contractAuthenticationDateFrom.Value);
            }

            if (contractAuthenticationDateTo.HasValue)
            {
                // إضافة يوم واحد لتغطية نهاية اليوم المحدد
                contracts = contracts.Where(c => c.ContractAuthenticationDate <= contractAuthenticationDateTo.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(syndicateBranch))
            {
                contracts = contracts.Where(c => c.SyndicateBranch.Contains(syndicateBranch));
            }

            // إرجاع قائمة العقود المفلترة إلى View
            return View(contracts.ToList());
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
