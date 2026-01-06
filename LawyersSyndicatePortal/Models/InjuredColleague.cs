// LawyersSyndicatePortal\Models\InjuredColleague.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class InjuredColleague
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ColleagueInfo")]
        public int ColleagueInfoId { get; set; }
        public virtual ColleagueInfo ColleagueInfo { get; set; }

        [StringLength(255)]
        [Display(Name = "اسم المصاب")]
        public string InjuredName { get; set; }

        [StringLength(20)]
        [Phone] // إضافة خاصية التحقق من رقم الهاتف
        [Display(Name = "رقم التواصل مع ذوي المصاب")] // هذا هو الحقل الجديد
        public string ContactNumber { get; set; }
    }
}
