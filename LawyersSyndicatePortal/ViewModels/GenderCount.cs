// المسار: LawyersSyndicatePortal\ViewModels\GenderCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerGenderReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class GenderCount
    {
        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "العدد")]
        public int Count { get; set; }
    }
}
