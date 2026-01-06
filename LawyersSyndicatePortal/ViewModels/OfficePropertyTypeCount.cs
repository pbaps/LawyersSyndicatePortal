// المسار: LawyersSyndicatePortal\ViewModels\OfficePropertyTypeCount.cs
// (يمكن أن يكون هذا الكلاس داخل نفس ملف LawyerOfficePropertyTypeReportViewModel.cs أو في ملف منفصل)
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class OfficePropertyTypeCount
    {
        [Display(Name = "نوع الملكية")]
        public string PropertyType { get; set; }

        [Display(Name = "العدد")]
        public int Count { get; set; }
    }
}
