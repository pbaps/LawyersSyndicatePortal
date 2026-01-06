// LawyersSyndicatePortal\ViewModels\SpouseReportViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class SpouseReportViewModel
    {
        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم الزوجة")]
        public string SpouseName { get; set; }

        [Display(Name = "رقم هوية الزوجة")]
        public string SpouseIdNumber { get; set; }

        [Display(Name = "رقم جوال الزوجة")]
        public string SpouseMobileNumber { get; set; }
        // الخصائص الجديدة
        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorate { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; }
    }
}
