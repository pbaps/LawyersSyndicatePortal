// LawyersSyndicatePortal\Models\LawyerAttachment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class LawyerAttachment // تأكد من أن الكلاس معرف داخل الـ namespace
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "اسم الملف")]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "مسار الملف")]
        public string FilePath { get; set; } // المسار النسبي للملف على الخادم

        [Required]
        [Display(Name = "حجم الملف (بالبايت)")]
        public long FileSize { get; set; } // تم إضافة هذه الخاصية

        [Required]
        [StringLength(50)]
        [Display(Name = "نوع الملف (MIME Type)")]
        public string ContentType { get; set; } // تم إضافة هذه الخاصية

        [Required]
        [StringLength(100)]
        [Display(Name = "نوع المرفق")]
        public string AttachmentType { get; set; } // مثال: "بطاقة محاماة", "تقرير طبي", "أخرى"

        [Display(Name = "ملاحظات")]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Required] // تم إزالة هذا الـ Required إذا كان من الممكن أن تكون القيمة فارغة
        [Display(Name = "تاريخ الرفع")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UploadDate { get; set; } // تم التعديل ليصبح قابلاً للقيم الفارغة (DateTime?)
    }
}
