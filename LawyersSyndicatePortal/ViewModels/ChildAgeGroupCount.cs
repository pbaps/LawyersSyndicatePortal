// المسار: LawyersSyndicatePortal\ViewModels\ChildAgeGroupCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerChildrenAgeReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ChildAgeGroupCount
    {
        [Display(Name = "الفئة العمرية")]
        public string AgeGroup { get; set; }

        [Display(Name = "العدد")]
        public int Count { get; set; }
    }
}
