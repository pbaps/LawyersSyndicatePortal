// LawyersSyndicatePortal\ViewModels\HealthStatusViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class HealthStatusViewModel
    {
        public int Id { get; set; }

        [Display(Name = "الحالة الصحية للمحامي")]
        [StringLength(100)]
        public string LawyerCondition { get; set; }

        [Display(Name = "تفاصيل الإصابة (إن وجدت)")]
        [StringLength(1000)]
        public string InjuryDetails { get; set; }

        [Display(Name = "العلاج المطلوب")]
        [StringLength(1000)]
        public string TreatmentNeeded { get; set; }

        [Display(Name = "التشخيص الطبي")]
        [StringLength(1000)]
        public string LawyerDiagnosis { get; set; }

        [Display(Name = "هل يوجد أفراد عائلة مصابون؟")]
        public bool HasFamilyMembersInjured { get; set; }

        [Display(Name = "عدد أفراد العائلة المصابين")]
        [Range(0, 50, ErrorMessage = "يجب أن يكون عدد أفراد العائلة المصابين بين 0 و 50.")]
        public int NumberOfFamilyMembersInjured { get; set; }

        // الخاصية المفقودة التي تسببت في الخطأ
        public List<FamilyMemberInjuryViewModel> FamilyMemberInjuries { get; set; } = new List<FamilyMemberInjuryViewModel>();

        // خصائص لـ DropDownLists
        public IEnumerable<SelectListItem> LawyerConditions { get; set; }

        public HealthStatusViewModel()
        {
            // تهيئة القائمة لتجنب NullReferenceException
            FamilyMemberInjuries = new List<FamilyMemberInjuryViewModel>();
        }
    }
}
