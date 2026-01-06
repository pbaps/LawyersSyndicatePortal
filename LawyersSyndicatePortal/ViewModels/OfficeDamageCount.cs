// المسار: LawyersSyndicatePortal\ViewModels\OfficeDamageCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerOfficeDamageReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class OfficeDamageCount
    {
        [Display(Name = "نوع الضرر")]
        public string DamageType { get; set; }

        [Display(Name = "العدد")]
        public int Count { get; set; }
    }
}
