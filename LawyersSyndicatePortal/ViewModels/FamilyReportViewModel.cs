using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج عرض يجمع كل البيانات اللازمة لصفحة "تقرير العائلة".
    /// </summary>
    public class FamilyReportViewModel
    {
        // خصائص خاصة بفلاتر البحث
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumberFilter { get; set; }

        [Display(Name = "اسم المحامي الكامل")]
        public string LawyerFullNameFilter { get; set; }

        // حقول البحث الجديدة
        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorateFilter { get; set; }

        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorateFilter { get; set; }

        [Display(Name = "رقم الجوال")]
        public string MobileNumberFilter { get; set; }

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatusFilter { get; set; }

        // الخصائص التي ستحتوي على بيانات التقرير الفعلية
        public ICollection<SpouseReportViewModel> Spouses { get; set; }
        public ICollection<ChildReportViewModel> Children { get; set; }

        public FamilyReportViewModel()
        {
            Spouses = new List<SpouseReportViewModel>();
            Children = new List<ChildReportViewModel>();
        }
    }
}
