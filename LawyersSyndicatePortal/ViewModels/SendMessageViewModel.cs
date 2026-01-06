// Path: LawyersSyndicatePortal\ViewModels\SendMessageViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // For SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class SendMessageViewModel
    {
        [Display(Name = "المستلم")]
        // This field will be used for a specific recipient only
        public string ReceiverIdNumber { get; set; }

        [Display(Name = "الموضوع")]
        [Required(ErrorMessage = "الموضوع مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز طول الموضوع 255 حرفاً.")]
        public string Subject { get; set; }

        [Display(Name = "نص الرسالة")]
        [Required(ErrorMessage = "نص الرسالة مطلوب.")]
        [AllowHtml] // To allow HTML input if you are using a rich text editor
        public string Body { get; set; }

        // List of available recipients for selection (used in Admin View when selecting a specific recipient)
        public IEnumerable<SelectListItem> AvailableReceivers { get; set; }

        // Property to specify the recipient type (new)
        [Display(Name = "نوع المستلم")]
        public string RecipientType { get; set; } // Possible values: "Specific", "AllLawyers", "AllTrainees", "AllLawyersAndTrainees"

        // Helper properties for searching a specific recipient (new)
        [Display(Name = "بحث عن مستلم")]
        public string SearchTerm { get; set; }

        // List of lawyers and trainees for dynamic search (new)
        public IEnumerable<SelectListItem> SearchResults { get; set; }

        // NEW: Property to store the ID of the original message when replying
        public int? OriginalMessageId { get; set; }
    }
}
