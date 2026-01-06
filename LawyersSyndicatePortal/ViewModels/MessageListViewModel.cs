// Path: LawyersSyndicatePortal\\ViewModels\\MessageListViewModel.cs
using System.Collections.Generic;
using LawyersSyndicatePortal.Models; // To ensure access to your Message model
using System.ComponentModel.DataAnnotations; // For Display
using System.Web.Mvc; // Required for SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class MessageListViewModel
    {
        [Display(Name = "الرسائل")]
        public List<Message> Messages { get; set; }

        // Pagination properties
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public string SearchString { get; set; } // Current search text

        // List of available page sizes for DropdownList (New Property)
        public List<SelectListItem> PageSizes { get; set; }

        public MessageListViewModel()
        {
            Messages = new List<Message>();
            // Initialize PageSizes with default options
            PageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" },
                new SelectListItem { Value = "100", Text = "100" }
            };
        }
    }
}
