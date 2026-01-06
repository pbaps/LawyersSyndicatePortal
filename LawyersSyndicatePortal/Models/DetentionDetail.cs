// LawyersSyndicatePortal\Models\DetentionDetail.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class DetentionDetail
    {
        [Key]
        public int Id { get; set; } // **إعادة المفتاح الأساسي إلى int Id**

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; } // المفتاح الخارجي
        public virtual Lawyer Lawyer { get; set; } // خاصية التنقل

        // ... (بقية الخصائص) ...
        [Display(Name = "هل تعرض المحامي للاعتقال؟")]
        public bool WasDetained { get; set; }

        [StringLength(100)]
        [Display(Name = "مدة الاعتقال")]
        public string DetentionDuration { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ بدء الاعتقال")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DetentionStartDate { get; set; }

        [Display(Name = "هل ما زال معتقلاً؟")]
        public bool IsStillDetained { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الإفراج (إن تم الإفراج)")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReleaseDate { get; set; }

        [StringLength(255)]
        [Display(Name = "نوع الاعتقال")]
        public string DetentionType { get; set; }

        [StringLength(500)]
        [Display(Name = "مكان الاعتقال")]
        public string DetentionLocation { get; set; }
    }
}