// LawyersSyndicatePortal\ViewModels\SpouseViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class SpouseViewModel
    {
        public int Id { get; set; } // ID of the Spouse record if it exists
        public int FamilyDetailId { get; set; } // Foreign Key to FamilyDetail

        [Display(Name = "اسم الزوجة")]
        [Required(ErrorMessage = "اسم الزوجة مطلوب")]
        [StringLength(255)]
        public string SpouseName { get; set; }

        [Display(Name = "رقم هوية الزوجة")]
        [StringLength(50)]
        public string SpouseIdNumber { get; set; }

        [Display(Name = "رقم جوال الزوجة")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم جوال صحيح")]
        [StringLength(20)]
        public string SpouseMobileNumber { get; set; }
    }
}