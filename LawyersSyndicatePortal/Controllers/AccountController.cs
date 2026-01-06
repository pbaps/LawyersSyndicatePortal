using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using LawyersSyndicatePortal.Models;
using System.Data.Entity;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    // مرشح للتحقق من الأذونات وتسجيل الأحداث
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;

        // المُنشئ الافتراضي، يُستخدم عندما لا يتم حقن التبعيات
        public AccountController()
        {
            _context = new ApplicationDbContext();
        }

        // المُنشئ الذي يُستخدم مع حقن التبعيات (Dependency Injection)
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationDbContext context)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _context = context;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [AuditLog("دخول", "دخول محامي")]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            // تحقق من صحة البيانات المرسلة من النموذج
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting login for IdNumber: {model.IdNumber}");

                // البحث عن المستخدم باستخدام رقم الهوية بدلاً من اسم المستخدم
                var user = await UserManager.Users.SingleOrDefaultAsync(u => u.IdNumber == model.IdNumber);

                // التحقق مما إذا كان المستخدم موجودًا.
                if (user == null)
                {
                    // رسالة خطأ عامة لتجنب الكشف عن وجود حساب من عدمه
                    System.Diagnostics.Debug.WriteLine($"User not found with IdNumber: {model.IdNumber}");
                    ModelState.AddModelError("", "فشل في محاولة تسجيل الدخول. رقم الهوية أو كلمة المرور غير صحيحة.");
                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine($"User found. UserName: '{user.UserName}', Email: '{user.Email}'");

                // محاولة تسجيل دخول المستخدم باستخدام اسم المستخدم وكلمة المرور
                var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);

                System.Diagnostics.Debug.WriteLine($"SignInResult: {result}");

                switch (result)
                {
                    case SignInStatus.Success:
                        // توجيه المستخدم بناءً على الدور بعد تسجيل الدخول بنجاح
                        System.Diagnostics.Debug.WriteLine("Login successful. Checking roles...");
                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (await UserManager.IsInRoleAsync(user.Id, "Corrector"))
                        {
                            return RedirectToAction("CorrectorDashboard", "Corrector");
                        }
                        else
                        {
                            return RedirectToAction("UserDashboard", "Home");
                        }

                    case SignInStatus.LockedOut:
                        System.Diagnostics.Debug.WriteLine("Login failed. User is locked out.");
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        System.Diagnostics.Debug.WriteLine("Login failed. Requires verification.");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        System.Diagnostics.Debug.WriteLine("Login failed. Invalid password or username.");
                        ModelState.AddModelError("", "فشل في محاولة تسجيل الدخول. رقم الهوية أو كلمة المرور غير صحيحة.");
                        return View(model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An unexpected error occurred during login: {ex.Message}");
                // إضافة معالجة عامة للأخطاء غير المتوقعة
                // يمكن استخدام نظام تسجيل الأخطاء (logging) هنا لتسجيل ex
                ModelState.AddModelError("", "حدث خطأ غير متوقع أثناء محاولة تسجيل الدخول. يرجى المحاولة مرة أخرى.");
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    // توجيه المستخدم بناءً على الدور بعد المصادقة الثنائية بنجاح
                    var userId = await SignInManager.GetVerifiedUserIdAsync();
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (await UserManager.IsInRoleAsync(user.Id, "Corrector"))
                        {
                            return RedirectToAction("Index", "Corrector");
                        }
                        else
                        {
                            return RedirectToAction("UserDashboard", "Home");
                        }
                    }
                    return RedirectToLocal(model.ReturnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "رمز غير صالح.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [Authorize(Roles = "Admin")]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تسجيل حساب جديد", "عملية تسجيل حساب جديد للمستخدمين")]
        [AuditLog("تسجيل", "تسجيل مستخدم جديد")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Lawyer");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("UserDashboard", "Home");
                }
                AddErrors(result);
            }
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.IdNumber);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return View("ForgotPasswordConfirmation");
                }
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.IdNumber);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // طلب إعادة التوجيه إلى مزود تسجيل الدخول الخارجي
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindAsync(loginInfo.Login);

                    if (user != null)
                    {
                        // توجيه المستخدم بناءً على الدور
                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (await UserManager.IsInRoleAsync(user.Id, "Corrector"))
                        {
                            return RedirectToAction("Index", "Corrector");
                        }
                        else
                        {
                            return RedirectToAction("UserDashboard", "Home");
                        }
                    }
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    return RedirectToAction("Login");
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تسجيل الخروج", "عملية تسجيل خروج المستخدم من النظام")]
        [AuditLog("تسجيل خروج", "تسجيل خروج محامي")]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        // يُستخدم لتنظيف الموارد
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
