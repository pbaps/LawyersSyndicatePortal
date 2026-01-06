using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Filters;
using System.Data.Entity;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LawyersSyndicatePortal.Controllers
{
    // تمت إزالة مرشح الصلاحيات العامة من هنا وتطبيقه بشكل فردي
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController()
        {
            _context = new ApplicationDbContext();
        }

        // صلاحية عرض لوحة التحكم الرئيسية
        [PermissionAuthorizationFilter("عرض لوحة تحكم المدير", "صلاحية عرض لوحة تحكم المدير")]
        [AuditLog("عرض", "عرض لوحة تحكم المدير")]
        public ActionResult Dashboard()
        {
            ViewBag.Message = "مرحباً بك في لوحة تحكم نظام الاختبارات!";
            return View();
        }

        // صلاحية عرض التقارير والاستعلامات
        [PermissionAuthorizationFilter("عرض التقارير والاستعلامات", "صلاحية عرض صفحة التقارير والاستعلامات")]
        [AuditLog("عرض", "عرض صفحة التقارير والاستعلامات")]
        public ActionResult ReportsAndQueries()
        {
            return View();
        }

        // صلاحية عرض الصفحة الرئيسية للمدير (مع إحصائيات)
        [PermissionAuthorizationFilter("عرض الصفحة الرئيسية للمدير", "صلاحية عرض الصفحة الرئيسية للمدير")]
        [AuditLog("عرض", "عرض الصفحة الرئيسية للمدير")]
        public ActionResult Index()
        {
            var viewModel = new NewLawyerReportViewModel();

            int totalLawyersCount = _context.Lawyers.Count();
            var professionalStatuses = new List<string> { "مزاول", "غير مزاول", "متدرب", "موقوف", "مشطوب", "متقاعد", "متوفي" };
            var statusCounts = new Dictionary<string, int>();
            foreach (var status in professionalStatuses)
            {
                statusCounts[status] = _context.Lawyers.Count(l => l.ProfessionalStatus == status);
            }
            ViewBag.TotalLawyersCount = totalLawyersCount;
            ViewBag.StatusCounts = statusCounts;

            viewModel.LatestAuditLogs = _context.AuditLogs
                                                 .OrderByDescending(log => log.Timestamp)
                                                 .Take(5)
                                                 .ToList();

            return View(viewModel);
        }

        // صلاحية إضافة بيانات جديدة
        [PermissionAuthorizationFilter("إضافة بيانات", "صلاحية إضافة بيانات جديدة")]
        [AuditLog("إضافة", "إضافة بيانات")]
        public ActionResult Create()
        {
            // منطق الإضافة
            return View();
        }

        // صلاحية تعديل البيانات
        [PermissionAuthorizationFilter("تعديل بيانات", "صلاحية تعديل البيانات")]
        [AuditLog("تعديل", "تعديل بيانات")]
        public ActionResult Edit(int id)
        {
            // منطق التعديل
            return View();
        }

        // صلاحية حذف البيانات
        [PermissionAuthorizationFilter("حذف بيانات", "صلاحية حذف البيانات")]
        [AuditLog("حذف", "حذف بيانات")]
        public ActionResult Delete(int id)
        {
            // منطق الحذف
            return RedirectToAction("Index");
        }

        // صلاحية تصدير البيانات
        [PermissionAuthorizationFilter("تصدير بيانات", "صلاحية تصدير البيانات")]
        [AuditLog("تصدير", "تصدير بيانات")]
        public ActionResult Export()
        {
            // منطق التصدير
            return View();
        }

        // صلاحية استيراد البيانات
        [PermissionAuthorizationFilter("استيراد بيانات", "صلاحية استيراد البيانات")]
        [AuditLog("استيراد", "استيراد بيانات")]
        public ActionResult Import()
        {
            // منطق الاستيراد
            return View();
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
