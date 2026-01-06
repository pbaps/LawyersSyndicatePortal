using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LawyersSyndicatePortal.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Net;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    [AllowAnonymous]
    public class LawyerPasswordRecoveryController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public LawyerPasswordRecoveryController() { }

        public LawyerPasswordRecoveryController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
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







        //////////////////////////////////
        ///
        [Authorize(Roles = "Admin")]
 
        [AuditLog("صفحة تعديل كلمة مرور للمستخدم", "صلاحية عرض لوحة تعديل كلمة مرور المستخدم")]
        public ActionResult AdminRecoveryRequest()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminRecoveryRequest(ForgotPasswordViewModel model)
        {
            // التحقق من صلاحية النموذج المرسل
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // البحث عن المستخدم باستخدام رقم الهوية
            var user = await UserManager.FindByIdNumberAsync(model.IdNumber);

            // إذا لم يتم العثور على المستخدم
            if (user == null)
            {
                ModelState.AddModelError("", "رقم الهوية غير موجود في النظام.");
                return View(model);
            }

            // إنشاء رمز إعادة تعيين كلمة المرور
            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // إعادة توجيه المستخدم إلى صفحة إعادة تعيين كلمة المرور مع تمرير المعرف الفريد (Primary Key)
            // ورمز إعادة التعيين ورقم الهوية كمعلمات في الرابط.
            return RedirectToAction("AdminResetPassword", new { userId = user.Id, code = code, idNumber = user.IdNumber });
        }

        public ActionResult AdminRecoveryConfirmation()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdminResetPassword(string userId, string code, string idNumber)
        {
            // التحقق من وجود المعرف الفريد والرمز
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "رمز إعادة التعيين غير صالح أو مفقود.";
                return RedirectToAction("AdminRecoveryRequest");
            }

            // تهيئة النموذج بناءً على البيانات المستلمة من الرابط
            var model = new ResetPasswordViewModel { Code = code, UserId = userId, IdNumber = idNumber };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminResetPassword(ResetPasswordViewModel model)
        {
            // التحقق من صلاحية النموذج
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // البحث عن المستخدم
            var user = await UserManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "المستخدم غير موجود أو الرابط غير صالح.");
                return View(model);
            }

            // استخدام المعرف الفريد (user.Id) لتحديث كلمة المرور
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)
            {
                // تم التحديث بنجاح، أرسل رسالة النجاح وحوّل المستخدم إلى صفحة التأكيد
                TempData["SuccessMessage"] = "تمت إعادة تعيين كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول.";
                return RedirectToAction("AdminResetPasswordConfirmation");
            }
            else
            {
                // إذا فشلت العملية، أضف الأخطاء إلى ModelState
                AddErrors(result);
                return View(model);
            }
        }

        public ActionResult AdminResetPasswordConfirmation()
        {
            // ببساطة قم بعرض الصفحة التي ستعرض رسالة النجاح
            // الرسالة موجودة بالفعل في TempData وسيتم عرضها تلقائياً
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        // **هذه الوظيفة الإضافية مفيدة لإدارة الأخطاء**



        /// <summary>
        /// /////////////////////////////////جديد ايميل
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult VerifyIdentity()
        {
            return View();
        }

        // POST: LawyerPasswordRecovery/RecoveryRequest
        // This action handles the submission of the identity verification form.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecoveryRequest(VerifyIdentityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Explicitly return the VerifyIdentity view with the correct model
                return View("VerifyIdentity", model);
            }

            var user = await db.Users.Include(u => u.LinkedLawyer)
                                     .FirstOrDefaultAsync(u => u.LinkedLawyer.IdNumber == model.IdNumber);

            if (user == null)
            {
                TempData["ErrorMessage"] = "رقم الهوية غير موجود في النظام.";
                return View("VerifyIdentity", model);
            }

            bool dateMatch = (user.LinkedLawyer.PracticeStartDate.HasValue && user.LinkedLawyer.PracticeStartDate.Value.Date == model.VerificationDate.Value.Date) ||
                             (user.LinkedLawyer.TrainingStartDate.HasValue && user.LinkedLawyer.TrainingStartDate.Value.Date == model.VerificationDate.Value.Date);

            if (!dateMatch)
            {
                TempData["ErrorMessage"] = "تاريخ المزاولة أو التدريب غير صحيح.";
                return View("VerifyIdentity", model);
            }

            TempData["SuccessMessage"] = "تم التحقق بنجاح، يرجى تأكيد البريد الإلكتروني لاستعادة كلمة المرور.";
            return RedirectToAction("ConfirmEmail", new { id = user.Id });
        }

        // GET: LawyerPasswordRecovery/ConfirmEmail
        // This action displays the user's registered email for confirmation.
        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "معلومات المستخدم مفقودة.";
                return RedirectToAction("VerifyIdentity");
            }

            // Retrieve the user to get their email address.
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                TempData["ErrorMessage"] = "لم يتم العثور على بريد إلكتروني مسجل. يرجى تعديله.";
                return RedirectToAction("UpdateEmail", new { id = id });
            }

            ViewBag.EmailAddress = user.Email;
            ViewBag.UserId = user.Id;
            return View();
        }

        // POST: LawyerPasswordRecovery/SendResetLink
        // This action sends the password reset link via email.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendResetLink(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود أو الرابط غير صالح.";
                return RedirectToAction("VerifyIdentity");
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "LawyerPasswordRecovery", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

            var emailBody = $"<p>مرحباً بك،</p><p>لقد طلبنا إعادة تعيين كلمة المرور الخاصة بك. يرجى الضغط على الرابط التالي لإتمام العملية:</p><a href='{callbackUrl}'>إعادة تعيين كلمة المرور</a>";
            await SendEmailAsync(user.Email, "إعادة تعيين كلمة المرور", emailBody);

            TempData["SuccessMessage"] = "تم إرسال رابط استعادة كلمة المرور إلى بريدك الإلكتروني بنجاح.";
            return RedirectToAction("ResetPasswordConfirmation");
        }

        // GET: LawyerPasswordRecovery/UpdateEmail
        // This action displays the form to update the user's email.
        [HttpGet]
        public async Task<ActionResult> UpdateEmail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "معلومات المستخدم مفقودة.";
                return RedirectToAction("VerifyIdentity");
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود.";
                return RedirectToAction("VerifyIdentity");
            }

            var model = new UpdateEmailViewModel { IdNumber = user.LinkedLawyer.IdNumber };
            return View(model);
        }

        // POST: LawyerPasswordRecovery/UpdateEmail
        // This action handles the email update logic.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateEmail(UpdateEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // First, find the user based on the provided IdNumber from the model.
            var user = await db.Users.FirstOrDefaultAsync(u => u.LinkedLawyer.IdNumber == model.IdNumber);

            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود.";
                return View(model);
            }

            // Check if the new email is already in use by a different user.
            var existingUserWithNewEmail = await db.Users.FirstOrDefaultAsync(u => u.Email == model.NewEmail && u.Id != user.Id);
            if (existingUserWithNewEmail != null)
            {
                TempData["ErrorMessage"] = "هذا البريد الإلكتروني مسجل بالفعل لمستخدم آخر.";
                return View(model);
            }

            // Update the user's email and username fields.
            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث البريد الإلكتروني بنجاح. يمكنك الآن المتابعة.";
            return RedirectToAction("ConfirmEmail", new { id = user.Id });
        }

        // GET: LawyerPasswordRecovery/ResetPasswordConfirmation
        // This action displays the password reset confirmation page.
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }



        /// <summary>
        /// ///////////////////////////
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string code, string userId)
        {
            if (code == null || userId == null)
            {
                TempData["ErrorMessage"] = "الرابط غير صالح. يرجى المحاولة مرة أخرى.";
                return RedirectToAction("VerifyIdentity");
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود.";
                return RedirectToAction("VerifyIdentity");
            }

            var model = new ResetPasswordEmailViewModel
            {
                Code = code,
                Email = user.Email
            };

            return View(model);
        }

        // POST: LawyerPasswordRecovery/ResetPassword
        // This action handles the password reset submission.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddErrors(result);
            return View();
        }







        ////////////////////////











        // This is a helper function to add identity errors to the model state.


        // This is a helper function to send emails.
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            const string fromEmail = "pbagaza.hr@gmail.com";
            const string appPassword = "emri gqvw nbyx aeug";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, "نقابة المحامين - غزة- الدعم الفني ");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
