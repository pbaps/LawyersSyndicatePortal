// LawyersSyndicatePortal\ViewModels\GeneralInfoViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // For SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class GeneralInfoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "هل تمارس المهنة الشرعية؟")]
        public bool PracticesShariaLaw { get; set; }

        [Display(Name = "تاريخ الحصول على مزاولة المحاماة الشرعية")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ShariaLawPracticeStartDate { get; set; }

        [Display(Name = "هل استلمت من النقابة مساعدات؟")]
        public bool ReceivedAidFromSyndicate { get; set; }

        public List<ReceivedAidViewModel> ReceivedAids { get; set; }

        // قائمة بأنواع المساعدات المتاحة للاختيار
        public List<SelectListItem> AvailableAidTypes { get; set; }

        public GeneralInfoViewModel()
        {
            ReceivedAids = new List<ReceivedAidViewModel>();
            AvailableAidTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "الدفعة المالية الاولى", Text = "الدفعة المالية الاولى" },
                new SelectListItem { Value = "الدفعة المالية الثانية", Text = "الدفعة المالية الثانية" },
                new SelectListItem { Value = "الدفعة المالية الثالثة", Text = "الدفعة المالية الثالثة" },
                new SelectListItem { Value = "الدفعة المالية الرابعة", Text = "الدفعة المالية الرابعة" },
                new SelectListItem { Value = "طرد غذائي", Text = "طرد غذائي" },
                new SelectListItem { Value = "طرد صحي", Text = "طرد صحي" },
                new SelectListItem { Value = "طرد خضار", Text = "طرد خضار" }
            };
        }
    }
}
