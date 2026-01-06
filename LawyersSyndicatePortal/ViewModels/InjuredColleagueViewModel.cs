// LawyersSyndicatePortal\ViewModels\InjuredColleagueViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class InjuredColleagueViewModel
    {
        public int Id { get; set; }
        public int ColleagueInfoId { get; set; } // المفتاح الخارجي لربط المصاب بمعلومات الزملاء

        [Display(Name = "اسم المصاب")]
        [Required(ErrorMessage = "اسم المصاب مطلوب.")]
        [StringLength(255)]
        public string InjuredName { get; set; }

        [Display(Name = "رقم التواصل مع ذوي المصاب (اختياري)")]
        [StringLength(20)]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة.")]
        public string ContactNumber { get; set; }
    }
}
