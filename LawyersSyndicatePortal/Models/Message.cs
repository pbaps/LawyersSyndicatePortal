// Path: LawyersSyndicatePortal\Models\Message.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        // رقم هوية المرسل (المفتاح الخارجي لـ ApplicationUser)
        [Required(ErrorMessage = "معرف المرسل مطلوب.")]
        [StringLength(50)]
        [Display(Name = "المرسل (معرف المستخدم)")]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser SenderUser { get; set; }

        // رقم هوية المحامي المرسل (إذا كان المرسل محامياً) - جديد
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي المرسل")]
        public string SenderLawyerIdNumber { get; set; }

        // اسم المحامي المرسل الكامل (إذا كان المرسل محامياً) - جديد
        [StringLength(255)]
        [Display(Name = "اسم المحامي المرسل")]
        public string SenderLawyerFullName { get; set; }

        // رقم هوية المستلم (المفتاح الخارجي لـ ApplicationUser)
        // يمكن أن يكون فارغاً للتعميمات الإدارية
        [StringLength(50)]
        [Display(Name = "المستلم")]
        public string ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser ReceiverUser { get; set; }

        [Required(ErrorMessage = "عنوان الرسالة مطلوب.")]
        [StringLength(255)]
        [Display(Name = "الموضوع")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "محتوى الرسالة مطلوب.")]
        [Display(Name = "نص الرسالة")]
        public string Body { get; set; }

        [Display(Name = "تاريخ الإرسال")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime SentDate { get; set; }

        [Display(Name = "مقروءة")]
        public bool IsRead { get; set; }

        // حقل لتمييز الرسائل كتعميمات إدارية - تم إضافة هذا الحقل
        [Display(Name = "تعميم إداري")]
        public bool IsAdminBroadcast { get; set; }

        // يمكن استخدام هذا الحقل لتمييز نوع الرسالة (خاصة، رد)
        [StringLength(50)]
        [Display(Name = "نوع الرسالة")]
        public string MessageType { get; set; } // مثال: "Private", "Reply"
    }
}
