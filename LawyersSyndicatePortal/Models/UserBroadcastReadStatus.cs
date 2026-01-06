// Path: LawyersSyndicatePortal\\Models\\UserBroadcastReadStatus.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class UserBroadcastReadStatus
    {
        [Key]
        public int Id { get; set; }

        // معرف المستخدم الذي قرأ التعميم
        [Required]
        [StringLength(50)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // معرف التعميم الذي تم قراءته
        [Required]
        public int BroadcastId { get; set; }

        [ForeignKey("BroadcastId")]
        public virtual Broadcast Broadcast { get; set; }

        // حالة القراءة: true إذا تم قراءته، false إذا لم يتم
        [Display(Name = "مقروء")]
        public bool IsRead { get; set; } = false; // القيمة الافتراضية: غير مقروء

        /// يمكنك إضافة تاريخ القراءة لأغراض التتبع إذا لزم الأمر
         [Display(Name = "تاريخ القراءة")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
         public DateTime? ReadDate { get; set; }
    }
}
