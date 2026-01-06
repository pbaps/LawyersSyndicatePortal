// Path: LawyersSyndicatePortal/Models/AuditLog.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    /// <summary>
    /// Model for logging administrative actions.
    /// هذا النموذج يستخدم لتسجيل الإجراءات الإدارية التي يقوم بها المسؤولون.
    /// </summary>
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم المسؤول")]
        public string AdminName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "اسم المتحكم")]
        public string ControllerName { get; set; }

        [Required]
        [Display(Name = "تاريخ ووقت التعديل")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Display(Name = "الإجراء")]
        [StringLength(255)]
        public string Action { get; set; }

        [Display(Name = "التفاصيل")]
        public string Details { get; set; }

        [Display(Name = "اسم الجدول")]
        [StringLength(100)]
        public string TableName { get; set; }

        [Display(Name = "المفتاح الأساسي للكيان")]
        public int? EntityId { get; set; }
    }
}