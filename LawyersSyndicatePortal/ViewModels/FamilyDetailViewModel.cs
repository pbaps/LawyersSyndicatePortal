// LawyersSyndicatePortal\ViewModels\FamilyDetailViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // For SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class FamilyDetailViewModel
    {
        public int Id { get; set; } // ID of the FamilyDetail record if it exists

        [Display(Name = "الحالة الاجتماعية")]
        [Required(ErrorMessage = "الرجاء تحديد الحالة الاجتماعية")]
        public string MaritalStatus { get; set; }

        [Display(Name = "عدد الزوجات")]
        [Range(0, 10, ErrorMessage = "العدد يجب أن يكون بين 0 و 10")] // Assuming a reasonable max
        public int? NumberOfSpouses { get; set; }

        [Display(Name = "هل يوجد أبناء؟")]
        public bool HasChildren { get; set; }

        [Display(Name = "عدد الأبناء")]
        [Range(0, 20, ErrorMessage = "العدد يجب أن يكون بين 0 و 20")] // Assuming a reasonable max
        public int? NumberOfChildren { get; set; }

        // Collections for dynamic lists of spouses and children
        public List<SpouseViewModel> Spouses { get; set; }
        public List<ChildViewModel> Children { get; set; }

        // Dropdown list for Marital Status
        public List<SelectListItem> MaritalStatuses { get; set; }

        public FamilyDetailViewModel()
        {
            Spouses = new List<SpouseViewModel>();
            Children = new List<ChildViewModel>();

            MaritalStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Single", Text = "أعزب" },
                new SelectListItem { Value = "Married", Text = "متزوج" },
                new SelectListItem { Value = "Divorced", Text = "مطلق" },
                new SelectListItem { Value = "Widowed", Text = "أرمل" }
            };
        }
    }
}