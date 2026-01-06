// LawyersSyndicatePortal\ViewModels\OfficeDetailViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class OfficeDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "اسم المكتب")]
        [Required(ErrorMessage = "اسم المكتب مطلوب.")]
        [StringLength(255)]
        public string OfficeName { get; set; }

        [Display(Name = "عنوان المكتب")]
        [Required(ErrorMessage = "عنوان المكتب مطلوب.")]
        [StringLength(500)]
        public string OfficeAddress { get; set; }

        [Display(Name = "نوع العقار")]
        [Required(ErrorMessage = "نوع العقار مطلوب.")]
        [StringLength(100)]
        public string PropertyType { get; set; }

        [Display(Name = "حالة العقار")]
        [Required(ErrorMessage = "حالة العقار مطلوبة.")]
        [StringLength(100)]
        public string PropertyStatus { get; set; }

        [Display(Name = "هل يوجد شركاء؟")]
        public bool HasPartners { get; set; }

        [Display(Name = "عدد الشركاء")]
        [Range(0, 50, ErrorMessage = "يجب أن يكون عدد الشركاء بين 0 و 50.")]
        public int? NumberOfPartners { get; set; }

        public List<PartnerViewModel> Partners { get; set; } = new List<PartnerViewModel>();

        // خصائص لـ DropDownLists
        public IEnumerable<SelectListItem> PropertyTypes { get; set; }
        public IEnumerable<SelectListItem> PropertyStatuses { get; set; }
    }
}
