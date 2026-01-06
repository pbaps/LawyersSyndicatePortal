// LawyersSyndicatePortal\Models\FamilyMemberInjury.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class FamilyMemberInjury
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "معرف الحالة الصحية")]
        [ForeignKey("HealthStatus")]
        public int HealthStatusId { get; set; }
        public virtual HealthStatus HealthStatus { get; set; }

        [StringLength(500)]
        [Display(Name = "طبيعة إصابة فرد العائلة")]
        public string InjuryDetails { get; set; }

        [StringLength(255)]
        [Display(Name = "اسم المصاب")]
        public string InjuredFamilyMemberName { get; set; }

        [StringLength(50)]
        [Display(Name = "العلاقة بالمحامي")]
        public string RelationshipToLawyer { get; set; }
    }
}