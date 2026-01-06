using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

// تأكد من وجود هذا الـ 'using' للوصول إلى ApplicationUser و ApplicationDbContext من مجلد Models
using LawyersSyndicatePortal.Models;

// هذا هو الجزء الأهم: مساحة الاسم (namespace) للملف
// يجب أن تكون هذه هي مساحة الاسم الجذر لمشروعك (عادةً تكون نفس اسم المشروع).
namespace LawyersSyndicatePortal
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                //RequireNonAlphanumeric = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        // -----------------------------------------------------------------------
        // هذا هو المكان الذي يجب أن تضيف فيه الدالة الجديدة.
        // قمنا بإنشاء دالة مخصصة للبحث عن المستخدم باستخدام رقم الهوية.
        // -----------------------------------------------------------------------
        public Task<ApplicationUser> FindByIdNumberAsync(string idNumber)
        {
            // نستخدم هذا الاستعلام للبحث عن مستخدم برقم الهوية
            // SingleOrDefaultAsync تعيد أول عنصر يطابق الشرط أو قيمة null إذا لم تجد أي عنصر
            return this.Users.SingleOrDefaultAsync(u => u.IdNumber == idNumber);
        }
    }

    //
    // Configure the application sign-in manager which is used in this application.
    //
    //
    // Configure the application sign-in manager which is used in this application.
    //
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        // المشكلة هنا: يجب أن يكون النوع الممرر إلى المُنشئ هو Microsoft.Owin.Security.IAuthenticationManager
        public ApplicationSignInManager(ApplicationUserManager userManager, Microsoft.Owin.Security.IAuthenticationManager authenticationManager) // <--- تم التعديل هنا
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    // ** هذا هو تعريف ApplicationRoleManager **
    // يستخدم لإدارة الأدوار (Roles) في ASP.NET Identity (إضافة، حذف، تعيين أدوار للمستخدمين)
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
        }
    }

    // يمكنك إضافة خدمة البريد الإلكتروني الخاصة بك هنا لتوصيلها بالتطبيق عبر SMS والبريد الإلكتروني
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // قم بتوصيل خدمة البريد الإلكتروني الخاصة بك هنا لإرسال بريد إلكتروني.
            // مثال بسيط لإرسال بريد إلكتروني (يتطلب إعدادات SMTP):
            /*
            var client = new System.Net.Mail.SmtpClient("smtp.your-email.com", 587); // استبدل ببيانات خادم SMTP الخاص بك
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("your-email@example.com", "your-email-password");
            client.EnableSsl = true; // تمكين SSL

            var mail = new System.Net.Mail.MailMessage("your-email@example.com", message.Destination, message.Subject, message.Body);
            return client.SendMailAsync(mail);
            */
            return Task.FromResult(0); // في الوقت الحالي، لا تفعل شيئًا سوى إرجاع مهمة مكتملة
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // قم بتوصيل خدمة الرسائل القصيرة (SMS) الخاصة بك هنا لإرسال رسالة نصية.
            // يتطلب دمجًا مع خدمة SMS خارجية (مثل Twilio, Nexmo, إلخ).
            return Task.FromResult(0); // في الوقت الحالي، لا تفعل شيئًا سوى إرجاع مهمة مكتملة
        }
    }
}