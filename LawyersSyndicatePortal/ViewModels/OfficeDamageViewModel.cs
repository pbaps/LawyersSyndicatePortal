// LawyersSyndicatePortal\ViewModels\OfficeDamageViewModel.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class OfficeDamageViewModel
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Display(Name = "نوع الضرر")]
        public string DamageType { get; set; }

        [StringLength(1000)]
        [Display(Name = "توضيح تفاصيل الأضرار")]
        public string DamageDetails { get; set; }

        // خصائص لـ DropDownLists
        public IEnumerable<SelectListItem> DamageTypes { get; set; }
    }
}
