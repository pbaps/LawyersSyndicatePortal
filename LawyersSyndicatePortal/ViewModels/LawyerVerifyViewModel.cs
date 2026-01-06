// File: Models/LawyerVerifyViewModel.cs
// قم بتحديث هذا الملف ليطابق الكود أدناه.

using System;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    public class LawyerVerifyViewModel
    {
        [Required(ErrorMessage = "حقل رقم الهوية مطلوب")]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        // تأكد من أن هذا الحقل من نوع string لتجنب الخطأ
        [Required(ErrorMessage = "حقل التاريخ مطلوب")]
        [Display(Name = "تاريخ المزاولة/التدريب")]
        public string Date { get; set; }

        [Display(Name = "البريد الإلكتروني الحالي")]
        public string CurrentEmail { get; set; }

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني الجديد (اختياري)")]
        public string NewEmail { get; set; }
    }
}
