// المسار: LawyersSyndicatePortal\Models\ContactMessage.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // لإضافة [Index]

namespace LawyersSyndicatePortal.Models
{
    /// <summary>
    /// يمثل رسالة اتصال مرسلة من قبل مستخدم.
    /// </summary>
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول الاسم الكامل 255 حرفًا.")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول البريد الإلكتروني 255 حرفًا.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "الموضوع مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول الموضوع 255 حرفًا.")]
        [Display(Name = "الموضوع")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "نص الرسالة مطلوب.")]
        [Display(Name = "نص الرسالة")]
        public string MessageBody { get; set; }

        [Display(Name = "تاريخ الإرسال")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime SentDate { get; set; } = DateTime.Now; // تعيين القيمة الافتراضية لتاريخ الإرسال

        // خاصية جديدة لتحديد ما إذا كانت الرسالة قد تمت قراءتها من قبل المسؤول
        [Display(Name = "مقروءة")]
        public bool IsRead { get; set; } = false; // افتراضيًا، الرسالة غير مقروءة عند وصولها

        // خاصية جديدة لتسجيل تاريخ الرد على الرسالة
        [Display(Name = "تاريخ الرد")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ReplyDate { get; set; } // يمكن أن يكون فارغًا إذا لم يتم الرد بعد
    }
}
