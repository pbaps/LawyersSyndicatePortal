// File: Models/LawyerResetPasswordViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    // نموذج البيانات للتعامل مع طلبات إعادة تعيين كلمة المرور.
    public class LawyerResetPasswordViewModel
    {
        // خاصية لمعرف المستخدم، يتم إرسالها كحقل مخفي.
        // حقول مخفية لتمريرها من الرابط
        public string UserId { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال كلمة المرور الجديدة.")]
        [StringLength(100, ErrorMessage = "{0} يجب أن لا يقل طوله عن {2} حرفاً.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }
    }
}
