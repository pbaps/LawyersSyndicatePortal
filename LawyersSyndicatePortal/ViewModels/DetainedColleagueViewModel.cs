// LawyersSyndicatePortal\ViewModels\DetainedColleagueViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class DetainedColleagueViewModel
    {
        public int Id { get; set; }
        public int ColleagueInfoId { get; set; }

        [Display(Name = "اسم المعتقل")]
        [StringLength(255)]
        public string DetainedName { get; set; } // اختياري حسب الوصف

        [Display(Name = "رقم التواصل مع ذوي المعتقل (اختياري)")]
        [StringLength(20)]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة.")]
        public string ContactNumber { get; set; } // اختياري حسب الوصف
    }
}
