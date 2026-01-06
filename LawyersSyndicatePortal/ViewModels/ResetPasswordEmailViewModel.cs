using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ResetPasswordEmailViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [StringLength(100, ErrorMessage = "يجب أن تكون {0} 6 أحرف على الأقل.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقان.")]
        public string ConfirmPassword { get; set; }

        // الخصائص اللازمة لعملية التحقق وإعادة التعيين
        [Required]
        public string Code { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}