// المسار: LawyersSyndicatePortal\ViewModels\LawyerGovernorateReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerGovernorateReportViewModel
    {
        [Display(Name = "إحصائيات حسب المحافظة")]
        public List<GovernorateCount> GovernorateCounts { get; set; }

        [Display(Name = "إجمالي عدد المحامين")]
        public int TotalLawyers { get; set; }

        // لتحديد ما إذا كان التقرير للمحافظة الأصلية أو الحالية
        public string ReportType { get; set; }

        public LawyerGovernorateReportViewModel()
        {
            GovernorateCounts = new List<GovernorateCount>();
        }
    }
}
