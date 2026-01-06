// LawyersSyndicatePortal\Models\Spouse.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Spouse
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "معرف تفاصيل العائلة")]
        [ForeignKey("FamilyDetail")]
        public int FamilyDetailId { get; set; }
        public virtual FamilyDetail FamilyDetail { get; set; }

        [StringLength(255)]
        [Display(Name = "اسم الزوجة")]
        public string SpouseName { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم هوية الزوجة")]
        public string SpouseIdNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم جوال الزوجة")]
        [Phone]
        public string SpouseMobileNumber { get; set; }
    }
}