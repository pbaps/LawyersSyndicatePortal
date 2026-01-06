// LawyersSyndicatePortal\ViewModels\ColleagueInfoViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // For SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class ColleagueInfoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "هل تعلم بوجود أي شهداء من أعضاء الهيئة العامة؟")]
        public bool KnowsOfMartyrColleagues { get; set; }

        [Display(Name = "هل هناك شهداء في سجلاتك؟")]
        public bool HasMartyrs { get; set; }

        public List<MartyrColleagueViewModel> MartyrColleagues { get; set; }

        [Display(Name = "هل تعلم بوجود أي معتقلين من أعضاء الهيئة العامة؟")]
        public bool KnowsOfDetainedColleagues { get; set; }

        [Display(Name = "هل هناك معتقلين في سجلاتك؟")]
        public bool HasDetained { get; set; }

        public List<DetainedColleagueViewModel> DetainedColleagues { get; set; }

        [Display(Name = "هل تعلم بوجود أي مصابين من أعضاء الهيئة العامة بسبب الإبادة؟")]
        public bool KnowsOfInjuredColleagues { get; set; }

        [Display(Name = "هل هناك مصابين في سجلاتك؟")]
        public bool HasInjured { get; set; }

        public List<InjuredColleagueViewModel> InjuredColleagues { get; set; }

        public ColleagueInfoViewModel()
        {
            MartyrColleagues = new List<MartyrColleagueViewModel>();
            DetainedColleagues = new List<DetainedColleagueViewModel>();
            InjuredColleagues = new List<InjuredColleagueViewModel>();
        }
    }
}
