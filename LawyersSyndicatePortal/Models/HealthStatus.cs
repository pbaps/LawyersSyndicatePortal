// LawyersSyndicatePortal\Models\HealthStatus.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class HealthStatus
    {
        [Key]
        public int Id { get; set; } // **إعادة المفتاح الأساسي إلى int Id**

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; } // المفتاح الخارجي
        public virtual Lawyer Lawyer { get; set; } // خاصية التنقل

        // ... (بقية الخصائص) ...
        [StringLength(50)]
        [Display(Name = "حالة المحامي الصحية")]
        public string LawyerCondition { get; set; }

        [StringLength(500)]
        [Display(Name = "طبيعة الإصابة (إذا كان مصابًا)")]
        public string InjuryDetails { get; set; }

        [StringLength(500)]
        [Display(Name = "طبيعة العلاج (إذا كان بحاجة لعلاج)")]
        public string TreatmentNeeded { get; set; }

        [StringLength(1000)]
        [Display(Name = "تشخيص حالة المحامي")]
        public string LawyerDiagnosis { get; set; }

        [Display(Name = "هل تعرض أحد أفراد العائلة للإصابة؟")]
        public bool HasFamilyMembersInjured { get; set; }

        [Display(Name = "عدد أفراد العائلة المصابين")]
        public int? FamilyMembersInjured { get; set; }

        public virtual ICollection<FamilyMemberInjury> FamilyMemberInjuries { get; set; }

        public HealthStatus()
        {
            FamilyMemberInjuries = new HashSet<FamilyMemberInjury>();
        }
    }
}