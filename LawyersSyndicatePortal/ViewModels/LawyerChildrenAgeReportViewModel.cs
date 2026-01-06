// المسار: LawyersSyndicatePortal\ViewModels\LawyerChildrenAgeReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerChildrenAgeReportViewModel
    {
        [Display(Name = "إحصائيات حسب الفئة العمرية للأطفال")]
        public List<ChildAgeGroupCount> AgeGroupCounts { get; set; }

        [Display(Name = "إجمالي عدد الأطفال")]
        public int TotalChildren { get; set; }

        public LawyerChildrenAgeReportViewModel()
        {
            AgeGroupCounts = new List<ChildAgeGroupCount>();
        }
    }
}
