// Path: LawyersSyndicatePortal\ViewModels\ReplyMessageViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ReplyMessageViewModel
    {
        [Required]
        public int OriginalMessageId { get; set; }

        [Display(Name = "اسم المرسل الأصلي")]
        public string SenderName { get; set; }

        [Display(Name = "بريد المرسل الأصلي")]
        public string SenderEmail { get; set; }

        [Display(Name = "موضوع الرسالة الأصلية")]
        public string OriginalSubject { get; set; }

        [Display(Name = "نص الرسالة الأصلية")]
        public string OriginalMessageBody { get; set; }

        [Required(ErrorMessage = "موضوع الرد مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول الموضوع 255 حرفًا.")]
        [Display(Name = "موضوع الرد")]
        public string ReplySubject { get; set; }

        [Required(ErrorMessage = "نص الرد مطلوب.")]
        [Display(Name = "نص الرد")]
        [DataType(DataType.MultilineText)]
        public string ReplyBody { get; set; }
    }
}
