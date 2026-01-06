// LawyersSyndicatePortal\Models\DetainedColleague.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class DetainedColleague
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ColleagueInfo")]
        public int ColleagueInfoId { get; set; }
        public virtual ColleagueInfo ColleagueInfo { get; set; }

        [StringLength(255)]
        [Display(Name = "اسم المعتقل")]
        public string DetainedName { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "رقم التواصل مع ذوي المعتقل (اختياري)")]
        public string ContactNumber { get; set; }
    }
}
