using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "رقم الهوية مطلوب.")]
        [Display(Name = "رقم الهوية")]
        // يمكنك إضافة أي تنسيقات أو قيود إضافية لرقم الهوية هنا
        public string IdNumber { get; set; } // تم تغيير Email إلى IdNumber

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "تذكرني؟")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
 

        [Required]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدية")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تاكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وكلمة المرور التأكيدية غير متطابقتين.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
        // أضف هذا السطر لحل المشكلة
        public string UserId { get; set; }
    }

    public class ForgotPasswordViewModel
    {/*
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
*/
        [Required(ErrorMessage = "يجب إدخال رقم الهوية")]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }
    }
}
