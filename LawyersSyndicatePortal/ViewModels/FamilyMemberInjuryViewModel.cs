// LawyersSyndicatePortal\ViewModels\FamilyMemberInjuryViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class FamilyMemberInjuryViewModel
    {
        public int Id { get; set; }
        public int HealthStatusId { get; set; }

        [Display(Name = "اسم فرد العائلة المصاب")]
        [Required(ErrorMessage = "اسم فرد العائلة المصاب مطلوب.")] // تأكد من وجود هذا السطر
        [StringLength(255)]
        public string InjuredFamilyMemberName { get; set; }

        [Display(Name = "العلاقة بالمحامي")]
        [StringLength(100)]
        public string RelationshipToLawyer { get; set; }

        [Display(Name = "تفاصيل الإصابة")]
        [StringLength(1000)]
        public string InjuryDetails { get; set; }


    }
}