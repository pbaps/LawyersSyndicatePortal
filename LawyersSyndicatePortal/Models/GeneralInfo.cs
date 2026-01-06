// LawyersSyndicatePortal\Models\GeneralInfo.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class GeneralInfo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [Display(Name = "هل تمارس المهنة الشرعية؟")]
        public bool PracticesShariaLaw { get; set; }

        [Display(Name = "تاريخ الحصول على مزاولة المحاماة الشرعية")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ShariaLawPracticeStartDate { get; set; }

        [Display(Name = "هل استلمت من النقابة مساعدات؟")]
        public bool ReceivedAidFromSyndicate { get; set; }

        public virtual ICollection<ReceivedAid> ReceivedAids { get; set; }

        public GeneralInfo()
        {
            ReceivedAids = new HashSet<ReceivedAid>();
        }
    }
}
