// المسار: LawyersSyndicatePortal\ViewModels\LawyerOfficeDamageReportViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerOfficeDamageReportViewModel
    {
        [Display(Name = "إحصائيات حسب نوع ضرر المكتب")]
        public List<OfficeDamageCount> OfficeDamageCounts { get; set; }

        [Display(Name = "إجمالي عدد المكاتب المتضررة")]
        public int TotalDamagedOffices { get; set; }

        public LawyerOfficeDamageReportViewModel()
        {
            OfficeDamageCounts = new List<OfficeDamageCount>();
        }
    }
}
