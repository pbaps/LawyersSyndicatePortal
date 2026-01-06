// LawyersSyndicatePortal\ViewModels\MartyrColleagueViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class MartyrColleagueViewModel
    {
        public int Id { get; set; }
        public int ColleagueInfoId { get; set; }

        [Display(Name = "اسم الشهيد")]
        [Required(ErrorMessage = "اسم الشهيد مطلوب.")]
        [StringLength(255)]
        public string MartyrName { get; set; }

        [Display(Name = "رقم التواصل مع ذوي الشهيد")]
        [StringLength(20)]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة.")]
        public string ContactNumber { get; set; }
    }
}
