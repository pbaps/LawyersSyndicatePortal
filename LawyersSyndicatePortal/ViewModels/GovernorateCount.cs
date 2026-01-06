// المسار: LawyersSyndicatePortal\ViewModels\GovernorateCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerGovernorateReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class GovernorateCount
    {
        [Display(Name = "المحافظة")]
        public string Governorate { get; set; }

        [Display(Name = "العدد")]
        public int Count { get; set; }
    }
}
