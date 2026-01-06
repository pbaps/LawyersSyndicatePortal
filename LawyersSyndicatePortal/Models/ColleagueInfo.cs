// LawyersSyndicatePortal\Models\ColleagueInfo.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class ColleagueInfo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [Display(Name = "هل تعلم بوجود أي شهداء من أعضاء الهيئة العامة؟")]
        public bool KnowsOfMartyrColleagues { get; set; }

        [Display(Name = "هل هناك شهداء في سجلاتك؟")]
        public bool HasMartyrs { get; set; } // هذا الحقل قد يكون زائداً إذا كانت KnowsOfMartyrColleagues كافية، ولكن سنبقيه كما هو بناءً على الملف المرفق.

        [Display(Name = "هل تعلم بوجود أي معتقلين من أعضاء الهيئة العامة؟")]
        public bool KnowsOfDetainedColleagues { get; set; }

        [Display(Name = "هل هناك معتقلين في سجلاتك؟")]
        public bool HasDetained { get; set; } // هذا الحقل قد يكون زائداً إذا كانت KnowsOfDetainedColleagues كافية، ولكن سنبقيه كما هو بناءً على الملف المرفق.

        [Display(Name = "هل تعلم بوجود أي مصابين من أعضاء الهيئة العامة بسبب الإبادة؟")]
        public bool KnowsOfInjuredColleagues { get; set; }

        [Display(Name = "هل هناك مصابين في سجلاتك؟")]
        public bool HasInjured { get; set; } // هذا الحقل قد يكون زائداً إذا كانت KnowsOfInjuredColleagues كافية، ولكن سنبقيه كما هو بناءً على الملف المرفق.

        public virtual ICollection<MartyrColleague> MartyrColleagues { get; set; }
        public virtual ICollection<DetainedColleague> DetainedColleagues { get; set; }
        public virtual ICollection<InjuredColleague> InjuredColleagues { get; set; }

        public ColleagueInfo()
        {
            MartyrColleagues = new HashSet<MartyrColleague>();
            DetainedColleagues = new HashSet<DetainedColleague>();
            InjuredColleagues = new HashSet<InjuredColleague>();
        }
    }
}
