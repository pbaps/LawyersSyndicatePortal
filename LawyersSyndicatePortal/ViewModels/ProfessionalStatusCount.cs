// المسار: LawyersSyndicatePortal\ViewModels\ProfessionalStatusCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerProfessionalStatusReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ProfessionalStatusCount
    {
        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; }

        [Display(Name = "عدد المحامين")]
        public int Count { get; set; }
    }
}
