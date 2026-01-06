using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using LawyersSyndicatePortal.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LawyersSyndicatePortal.Controllers
{
    // تمت إزالة مرشح الصلاحيات من هنا لأن دالة البناء الخاصة به تتطلب معلمات.
    public class BaseController : Controller
    {
        private ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        public BaseController()
        {
            _context = new ApplicationDbContext();
            _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(_context));
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // هذا المنطق لتعيين اسم العرض للمستخدم بعد أن يكون مرشح الصلاحيات قد تحقق بالفعل من الوصول.
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    // ملاحظة: لا يمكننا استخدام await داخل OnActionExecuting
                    // لذلك سنقوم بتنفيذ FindByIdAsync بشكل متزامن.
                    var userId = User.Identity.GetUserId();
                    var user = _userManager.FindById(userId);
                    if (user != null)
                    {
                        ViewData["DisplayName"] = user.FullName;
                    }
                }
                catch (Exception ex)
                {
                    // سجل الخطأ ولكن لا تمنع الإجراء من التنفيذ
                    Debug.WriteLine($"Error getting user display name: {ex.Message}");
                }
            }
            // من المهم استدعاء الطريقة الأساسية في النهاية.
            base.OnActionExecuting(filterContext);
        }

        // دالة Dispose لتحرير الموارد
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                }
                if (_userManager != null)
                {
                    _userManager.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
