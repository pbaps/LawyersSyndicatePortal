// Path: LawyersSyndicatePortal\ViewModels\ComposeBroadcastViewModel.cs
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // للسماح باستخدام AllowHtml

namespace LawyersSyndicatePortal.ViewModels
{
    public class ComposeBroadcastViewModel
    {
        [Display(Name = "الموضوع")]
        [Required(ErrorMessage = "الموضوع مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول الموضوع 255 حرفاً.")]
        public string Subject { get; set; }

        [Display(Name = "نص التعميم")]
        [Required(ErrorMessage = "نص التعميم مطلوب.")]
        [AllowHtml] // للسماح بإدخال محتوى HTML إذا كنت تستخدم محرر نصوص غني
        public string Body { get; set; }
    }
}
