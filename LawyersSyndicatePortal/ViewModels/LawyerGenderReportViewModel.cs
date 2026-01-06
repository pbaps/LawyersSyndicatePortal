// المسار: LawyersSyndicatePortal\ViewModels\LawyerGenderReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerGenderReportViewModel
    {
        [Display(Name = "إحصائيات حسب الجنس")]
        public List<GenderCount> GenderCounts { get; set; }

        [Display(Name = "إجمالي عدد المحامين")]
        public int TotalLawyers { get; set; }

        public LawyerGenderReportViewModel()
        {
            GenderCounts = new List<GenderCount>();
        }
    }
}
