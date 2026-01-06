// LawyersSyndicatePortal\Models\HomeDamage.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class HomeDamage
    {
        [Key]
        public int Id { get; set; } // **إعادة المفتاح الأساسي إلى int Id**

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; } // المفتاح الخارجي
        public virtual Lawyer Lawyer { get; set; } // خاصية التنقل

        // ... (بقية الخصائص) ...
        [Display(Name = "هل تعرض المنزل لأضرار؟")]
        public bool HasHomeDamage { get; set; }

        [StringLength(100)]
        [Display(Name = "نوع الضرر")]
        public string DamageType { get; set; }

        [StringLength(1000)]
        [Display(Name = "تفاصيل الأضرار")]
        public string DamageDetails { get; set; }
    }
}