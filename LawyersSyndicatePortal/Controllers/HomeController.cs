using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels; // تأكد من تضمين هذا الـ using
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Utility;

namespace LawyersSyndicatePortal.Controllers
{
 
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController()
        {
            _context = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
        }

        // GET: Home/Index (يمكن أن تكون هذه صفحة عامة أو صفحة لوحة تحكم افتراضية)
        [AllowAnonymous] // للسماح بالوصول بدون تسجيل دخول إذا كانت صفحة عامة
        // AuditLogAttribute هو مرشح صالح ولا يسبب أي خطأ
        public ActionResult Index()
        {
            return View();
        }

        // GET: Home/About
        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "وصف تطبيقك.";
            return View();
        }

        // GET: Home/Contact
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "صفحة الاتصال الخاصة بك.";
            return View();
        }

        // هذا هو الإجراء المطلوب.
        [AllowAnonymous]
        public ActionResult Unauthorized()
        {
            ViewBag.Message = "عذراً، لا تملك الصلاحيات اللازمة للوصول إلى هذه الصفحة.";
            return View();
        }


        // تأكد أن هذا الإجراء يتطلب صلاحية المستخدم
        [Authorize]
        [PermissionAuthorizationFilter("عرض لوحة تحكم المستخدم", "صلاحية الوصول إلى لوحة تحكم المستخدم بعد تسجيل الدخول.")]
        public async Task<ActionResult> UserDashboard()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                // إذا لم يتم العثور على المستخدم (مثلاً، جلسة منتهية)، أعد التوجيه إلى صفحة تسجيل الدخول
                return RedirectToAction("Login", "Account");
            }

            // جلب التعميمات الحديثة (مثلاً، آخر 5 تعميمات)
            var recentBroadcasts = await _context.Broadcasts
                                                     .OrderByDescending(b => b.SentDate)
                                                     .Take(5)
                                                     .ToListAsync();

            // حساب الرسائل والتعميمات غير المقروءة
            int unreadMessagesCount = await _context.Messages
                                                     .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                                                     .CountAsync();

            // حساب التعميمات غير المقروءة للمستخدم الحالي
            // يجب أن نتحقق من UserBroadcastReadStatuses
            int unreadBroadcastsCount = await _context.UserBroadcastReadStatuses
                                                           .Where(ubrs => ubrs.UserId == currentUserId && !ubrs.IsRead)
                                                           .CountAsync();

            bool hasNewBroadcasts = await _context.UserBroadcastReadStatuses
                                                         .AnyAsync(ubrs => ubrs.UserId == currentUserId && !ubrs.IsRead);

            // NEW: جلب الاختبارات المتاحة للمستخدم الحالي بناءً على الشروط
            ICollection<ExamAttendee> availableExams = new List<ExamAttendee>();
            if (!string.IsNullOrEmpty(currentUser.LinkedLawyerIdNumber))
            {
                availableExams = await _context.ExamAttendees
                                                 .Include(ea => ea.Exam) // تأكد من تحميل بيانات الاختبار
                                                 .Where(ea => ea.LawyerIdNumber == currentUser.LinkedLawyerIdNumber &&
                                                              ea.CanAttend &&
                                                              ea.IsExamVisible &&
                                                              !ea.IsCompleted && // لم يكمل الاختبار بعد
                                                              ea.Exam.IsPublished) // الاختبار نفسه نشط
                                                 .ToListAsync();
            }

            // إنشاء كائن UserDashboardViewModel وتعبئة خصائصه
            var model = new UserDashboardViewModel
            {
                User = currentUser, // تعيين كائن ApplicationUser بالكامل
                RecentBroadcasts = recentBroadcasts, // تعيين قائمة التعميمات الحديثة
                FullName = currentUser.FullName, // تعيين الاسم الرباعي
                Email = currentUser.Email,        // تعيين البريد الإلكتروني
                IdNumber = currentUser.IdNumber, // تعيين رقم الهوية
                UnreadMessagesCount = unreadMessagesCount,
                UnreadBroadcastsCount = unreadBroadcastsCount,
                HasNewBroadcasts = hasNewBroadcasts,
                AvailableExams = availableExams // NEW: تعيين قائمة الاختبارات المتاحة
            };

            return View(model);
        }

        // Dispose method to release resources
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
