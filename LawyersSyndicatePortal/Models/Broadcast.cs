// LawyersSyndicatePortal\Models\Broadcast.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Broadcast
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "المرسل")]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser SenderUser { get; set; }

        [Required(ErrorMessage = "عنوان التعميم مطلوب.")]
        [StringLength(255)]
        [Display(Name = "الموضوع")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "محتوى التعميم مطلوب.")]
        [Display(Name = "نص التعميم")]
        public string Body { get; set; }

        [Display(Name = "تاريخ الإرسال")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime SentDate { get; set; }

        [Display(Name = "مقروءة")]
        public bool IsRead { get; set; } // Can be used to track if a specific user has read it, or just for general tracking
    }
}