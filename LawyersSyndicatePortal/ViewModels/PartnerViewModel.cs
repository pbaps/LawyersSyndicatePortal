// LawyersSyndicatePortal\ViewModels\PartnerViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class PartnerViewModel
    {
        public int Id { get; set; }
        public int OfficeDetailId { get; set; } // المفتاح الخارجي لربط الشريك بتفاصيل المكتب

        [Display(Name = "اسم الشريك")]
        [Required(ErrorMessage = "اسم الشريك مطلوب.")]
        [StringLength(255)]
        public string PartnerName { get; set; }

        [Display(Name = "رقم عضويته")]
        [StringLength(50)]
        public string MembershipNumber { get; set; }
    }
}
