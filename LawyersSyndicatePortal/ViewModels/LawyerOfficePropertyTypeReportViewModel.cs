// المسار: LawyersSyndicatePortal\ViewModels\LawyerOfficePropertyTypeReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerOfficePropertyTypeReportViewModel
    {
        [Display(Name = "إحصائيات حسب نوع ملكية المكتب")]
        public List<OfficePropertyTypeCount> PropertyTypeCounts { get; set; }

        [Display(Name = "إجمالي عدد المكاتب")]
        public int TotalOffices { get; set; }

        public LawyerOfficePropertyTypeReportViewModel()
        {
            PropertyTypeCounts = new List<OfficePropertyTypeCount>();
        }
    }
}
