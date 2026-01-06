// Path: LawyersSyndicatePortal\ViewModels\PrivateMessageListViewModel.cs
using System.Collections.Generic;
using LawyersSyndicatePortal.Models; // للتأكد من الوصول إلى نموذج Message
using System.ComponentModel.DataAnnotations; // لـ Display
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class PrivateMessageListViewModel
    {
        [Display(Name = "الرسائل")]
        public List<Message> Messages { get; set; }

        // خصائص لتقسيم الصفحات (Pagination)
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public string SearchString { get; set; } // نص البحث الحالي

        // قائمة بأحجام الصفحات المتاحة (لـ DropdownList)
        public List<SelectListItem> PageSizes { get; set; }

        public PrivateMessageListViewModel()
        {
            Messages = new List<Message>();
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
