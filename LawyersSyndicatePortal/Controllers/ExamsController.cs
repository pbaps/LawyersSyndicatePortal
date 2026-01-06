using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم الخاص بإدارة الامتحانات.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    public class ExamsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Exams
        // يعرض قائمة بجميع الامتحانات
        [PermissionAuthorizationFilter("عرض الامتحانات", "صلاحية عرض قائمة الامتحانات المتاحة")]
        [AuditLog("عرض قائمة الامتحانات", "عرض قائمة الامتحانات")]
        public async Task<ActionResult> Index()
        {
            return View(await db.Exams.ToListAsync());
        }

        // GET: Exams/Details/5
        // يعرض تفاصيل امتحان معين
        [PermissionAuthorizationFilter("عرض تفاصيل الامتحان", "صلاحية عرض تفاصيل امتحان محدد")]
        [AuditLog("عرض تفاصيل الامتحان", "عرض تفاصيل الامتحان")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exam exam = await db.Exams.FindAsync(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // GET: Exams/Create
        // يعرض صفحة لإنشاء امتحان جديد
        [PermissionAuthorizationFilter("إنشاء امتحان جديد", "صلاحية عرض صفحة إنشاء امتحان جديد")]
        [AuditLog("عرض صفحة إنشاء امتحان", "عرض صفحة إنشاء امتحان")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Exams/Create
        // يستقبل بيانات الامتحان الجديد ويضيفها إلى قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ امتحان جديد", "صلاحية حفظ امتحان جديد في قاعدة البيانات")]
        [AuditLog("انشاء اختبار", "انشاء اختبار")]
        public async Task<ActionResult> Create([Bind(Include = "Title,Description,ExamDateTime,DurationMinutes,ExamType,PassingScorePercentage,MaxScore,IsPublished,IsRandomized,CanRetake,RetakeDelayDays,IsResultVisible")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                db.Exams.Add(exam);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(exam);
        }

        // GET: Exams/Edit/5
        // يعرض صفحة لتعديل امتحان موجود
        [PermissionAuthorizationFilter("تعديل امتحان", "صلاحية عرض صفحة تعديل امتحان موجود")]
        [AuditLog("عرض صفحة تعديل امتحان", "عرض صفحة تعديل امتحان")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exam exam = await db.Exams.FindAsync(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // POST: Exams/Edit/5
        // يستقبل بيانات الامتحان المعدلة ويحدثها في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ تعديلات الامتحان", "صلاحية حفظ التغييرات على امتحان موجود")]
        [AuditLog("تعديل اختبار", "تعديل اختبار")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,ExamDateTime,DurationMinutes,ExamType,PassingScorePercentage,MaxScore,IsPublished,IsRandomized,CanRetake,RetakeDelayDays,IsResultVisible")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                db.Entry(exam).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(exam);
        }

        // GET: Exams/Delete/5
        // يعرض صفحة لتأكيد حذف امتحان
        [PermissionAuthorizationFilter("حذف امتحان", "صلاحية عرض صفحة تأكيد حذف امتحان")]
        [AuditLog("عرض صفحة تأكيد حذف امتحان", "عرض صفحة تأكيد حذف امتحان")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exam exam = await db.Exams.FindAsync(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // POST: Exams/Delete/5
        // يحذف الامتحان من قاعدة البيانات
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تأكيد حذف امتحان", "صلاحية حذف امتحان من قاعدة البيانات بشكل نهائي")]
        [AuditLog("حذف اختبار", "حذف اختبار")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Exam exam = await db.Exams.FindAsync(id);
            db.Exams.Remove(exam);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
