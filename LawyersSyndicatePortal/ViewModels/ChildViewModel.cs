// LawyersSyndicatePortal\ViewModels\ChildViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // For SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class ChildViewModel
    {
        public int Id { get; set; } // ID of the Child record if it exists
        public int FamilyDetailId { get; set; } // Foreign Key to FamilyDetail

        [Required(ErrorMessage = "اسم الابن/الابنة مطلوب")]
        [StringLength(255)]
        [Display(Name = "اسم الابن/الابنة")]
        public string ChildName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        [StringLength(10)]
        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        // Dropdown list for Gender
        public List<SelectListItem> Genders { get; set; }

        public ChildViewModel()
        {
            Genders = new List<SelectListItem>
            {
                new SelectListItem { Value = "Male", Text = "ذكر" },
                new SelectListItem { Value = "Female", Text = "أنثى" }
            };
        }
    }
}