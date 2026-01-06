// LawyersSyndicatePortal\ViewModels\LawyerAttachmentViewModel.cs
using System.ComponentModel.DataAnnotations;
using System.Web; // For HttpPostedFileBase
using System.Collections.Generic; // For SelectListItem
using System; // For DateTime
using System.Web.Mvc;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerAttachmentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "نوع المرفق")]
        [Required(ErrorMessage = "نوع المرفق مطلوب.")]
        public string AttachmentType { get; set; }

        [Display(Name = "الملف")]
        public HttpPostedFileBase File { get; set; }

        [Display(Name = "اسم الملف الحالي")]
        public string ExistingFileName { get; set; } // لعرض اسم الملف الموجود

        [Display(Name = "مسار الملف")]
        public string FilePath { get; set; } // **تمت إضافة هذه الخاصية**

        [Display(Name = "ملاحظات")]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Display(Name = "تاريخ الرفع")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UploadDate { get; set; } // **تمت إضافة هذه الخاصية**

        public IEnumerable<SelectListItem> AvailableAttachmentTypes { get; set; }

        public LawyerAttachmentViewModel()
        {
            AvailableAttachmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "صورة شخصية", Text = "صورة شخصية" },
                new SelectListItem { Value = "صورة هوية", Text = "صورة هوية" },
                new SelectListItem { Value = "شهادة تخرج", Text = "شهادة تخرج" },
                new SelectListItem { Value = "شهادة مزاولة مهنة", Text = "شهادة مزاولة مهنة" },
                new SelectListItem { Value = "عقد إيجار", Text = "عقد إيجار" },
                new SelectListItem { Value = "فاتورة كهرباء", Text = "فاتورة كهرباء" },
                new SelectListItem { Value = "تقرير طبي", Text = "تقرير طبي" },
                new SelectListItem { Value = "وثيقة اعتقال", Text = "وثيقة اعتقال" },
                new SelectListItem { Value = "أخرى", Text = "أخرى" }
            };
        }
    }
}
