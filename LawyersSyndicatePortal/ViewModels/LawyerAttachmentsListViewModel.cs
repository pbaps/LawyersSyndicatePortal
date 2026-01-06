// LawyersSyndicatePortal\ViewModels\LawyerAttachmentsListViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerAttachmentsListViewModel
    {
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        public List<LawyerAttachmentViewModel> Attachments { get; set; } = new List<LawyerAttachmentViewModel>();

        // لملء القائمة المنسدلة لأنواع المرفقات في جميع العناصر
        public IEnumerable<SelectListItem> AvailableAttachmentTypes { get; set; }

        public LawyerAttachmentsListViewModel()
        {
            // تهيئة لتجنب NullReferenceException إذا لم يتم تعيينها صراحة
            AvailableAttachmentTypes = new List<SelectListItem>();
        }
    }
}
