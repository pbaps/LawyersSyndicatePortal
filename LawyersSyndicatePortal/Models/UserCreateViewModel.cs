using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectList

namespace LawyersSyndicatePortal.Models
{
    public class UserCreateViewModel
    {
        // هذا المعرف مطلوب في حالة التعديل، وليس مطلوبًا للإنشاء (يمكن أن يكون null)
        public string Id { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

       /// [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور على الأقل {2} حرفًا.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        [Display(Name = "الرقم الوطني")]
        public string IdNumber { get; set; }

        [Display(Name = "المحامي المرتبط (اختياري)")]
        public string LinkedLawyerIdNumber { get; set; }

        [Display(Name = "الدور")]
        public string RoleId { get; set; } // معرف الدور المختار

        // قوائم للعناصر المنسدلة في الـ View
        public IEnumerable<SelectListItem> Roles { get; set; }
        public IEnumerable<SelectListItem> Lawyers { get; set; }
    }
}