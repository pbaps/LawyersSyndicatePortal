using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    // نموذج تحديث البريد الإلكتروني
    public class UpdateEmailViewModel
    {
        [Required(ErrorMessage = "يجب إدخال البريد الإلكتروني الجديد")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني الجديد")]
        [StringLength(255)]
        public string NewEmail { get; set; }

        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }
    }
}
