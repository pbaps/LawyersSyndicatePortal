// المسار: LawyersSyndicatePortal\ViewModels\LawyerProfessionalStatusReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerProfessionalStatusReportViewModel
    {
        // قائمة تحتوي على تفاصيل كل حالة مهنية وعدد المحامين فيها
        public List<ProfessionalStatusCount> StatusCounts { get; set; }

        [Display(Name = "إجمالي عدد المحامين")]
        public int TotalLawyers { get; set; }

        public LawyerProfessionalStatusReportViewModel()
        {
            StatusCounts = new List<ProfessionalStatusCount>();
        }
    }
}
