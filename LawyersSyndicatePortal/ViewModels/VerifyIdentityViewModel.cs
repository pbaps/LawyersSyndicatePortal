using System;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    // نموذج التحقق من الهوية وتاريخ المزاولة/التدريب
    public class VerifyIdentityViewModel
    {
        [Required(ErrorMessage = "يجب إدخال رقم الهوية")]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "يجب إدخال تاريخ المزاولة أو التدريب")]
        [Display(Name = "تاريخ المزاولة أو التدريب")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? VerificationDate { get; set; }
    }
}
