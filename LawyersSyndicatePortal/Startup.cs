using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;
using LawyersSyndicatePortal.Models;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartupAttribute(typeof(LawyersSyndicatePortal.Startup))]
namespace LawyersSyndicatePortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // هذا السطر هو المسؤول عن تهيئة مكتبة SignalR في التطبيق وتفعيلها.
            // يجب وضعه قبل أي تهيئة أخرى للمصادقة.
            app.MapSignalR();

            ConfigureAuth(app);
        }

        // لمزيد من المعلومات حول إعداد المصادقة، يرجى زيارة https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // قم بتهيئة db context، وuser manager، و signin manager لاستخدام نسخة واحدة لكل طلب
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            // تمكين التطبيق من استخدام ملف تعريف الارتباط (cookie) لتخزين معلومات المستخدم المسجل دخوله
            // واستخدام ملف تعريف ارتباط مؤقت لتخزين معلومات حول مستخدم يسجل دخوله باستخدام مزود تسجيل دخول تابع لجهة خارجية
            // قم بتهيئة ملف تعريف الارتباط لتسجيل الدخول
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // يتيح للتطبيق التحقق من ختم الأمان عند تسجيل دخول المستخدم.
                    // هذه ميزة أمان تُستخدم عند تغيير كلمة مرور أو إضافة تسجيل دخول خارجي إلى حسابك.
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // يتيح للتطبيق تخزين معلومات المستخدم مؤقتًا عند التحقق من العامل الثاني في عملية المصادقة ثنائية العوامل.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // يتيح للتطبيق تذكر العامل الثاني لتسجيل الدخول، مثل الهاتف أو البريد الإلكتروني.
            // بمجرد التحقق من هذا الخيار، سيتم تذكر خطوة التحقق الثانية أثناء عملية تسجيل الدخول على الجهاز الذي قمت بتسجيل الدخول منه.
            // هذا مشابه لخيار "RememberMe" عند تسجيل الدخول.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // ملاحظة: تم نقل منطق Seeding إلى ملف Configuration.cs
            // لأن هذا هو المكان الصحيح له في Entity Framework.
        }
    }
}
