// LawyersSyndicatePortal\\Controllers\\AuditLogsController.cs
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LawyersSyndicatePortal.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن عرض وإدارة سجلات التدقيق والتحديث (Audit Logs).
    /// </summary>

    // تأكد من أن هذا المتحكم متاح فقط للمسؤولين
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: AuditLogs
        // يعرض قائمة بجميع سجلات التدقيق مع إمكانيات البحث والتصفية والترقيم.
        public ActionResult Index(string searchString, DateTime? startDate, DateTime? endDate, int? page, int? pageSize, string sortOrder)
        {
            // حفظ قيم البحث والتصفية والترتيب وحجم الصفحة في ViewBag لإعادة عرضها في حقول الإدخال
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPageSize = pageSize ?? 10;
            ViewBag.CurrentSortOrder = sortOrder;

            // إعداد الترتيب الافتراضي حسب التاريخ من الأحدث للأقدم
            ViewBag.TimestampSortParam = string.IsNullOrEmpty(sortOrder) ? "TimestampAsc" : "";

            // جلب جميع السجلات من قاعدة البيانات
            var auditLogs = from l in _context.AuditLogs
                            select l;

            // تطبيق التصفية حسب سلسلة البحث إذا كانت موجودة
            if (!string.IsNullOrEmpty(searchString))
            {
                auditLogs = auditLogs.Where(l => l.AdminName.Contains(searchString) || l.Action.Contains(searchString) || l.Details.Contains(searchString));
            }

            // تطبيق التصفية حسب تاريخ البدء
            if (startDate.HasValue)
            {
                auditLogs = auditLogs.Where(l => l.Timestamp >= startDate.Value);
            }

            // تطبيق التصفية حسب تاريخ الانتهاء
            if (endDate.HasValue)
            {
                // AddDays(1) to include the full day
                auditLogs = auditLogs.Where(l => l.Timestamp <= endDate.Value.AddDays(1));
            }

            // تطبيق الترتيب حسب الطلب
            switch (sortOrder)
            {
                case "TimestampAsc":
                    auditLogs = auditLogs.OrderBy(l => l.Timestamp);
                    break;
                default: // الترتيب الافتراضي
                    auditLogs = auditLogs.OrderByDescending(l => l.Timestamp);
                    break;
            }

            // إعداد الترقيم
            int pageNumber = (page ?? 1);
            int effectivePageSize = pageSize ?? 10;

            ViewBag.Title = "سجلات التدقيق";
            // إرجاع قائمة مرقمة إلى العرض
            return View(auditLogs.ToList().ToPagedList(pageNumber, effectivePageSize));
        }

        // GET: AuditLogs/Details/5
        // يعرض تفاصيل سجل تدقيق محدد بناءً على الـ ID.
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var auditLog = _context.AuditLogs.Find(id);
            if (auditLog == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = $"تفاصيل سجل التدقيق #{auditLog.Id}";
            return View(auditLog);
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
