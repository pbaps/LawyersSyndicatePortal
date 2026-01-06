// LawyersSyndicatePortal\Models\FamilyDetail.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class FamilyDetail
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
        [Display(Name = "الحالة الاجتماعية")]
        public string MaritalStatus { get; set; }

        [Display(Name = "عدد الزوجات")]
        public int? NumberOfSpouses { get; set; }

        [Display(Name = "هل يوجد أبناء؟")]
        public bool HasChildren { get; set; }
        [Display(Name = "عدد الأبناء")]
        public int? NumberOfChildren { get; set; }

        public virtual ICollection<Spouse> Spouses { get; set; }
        public virtual ICollection<Child> Children { get; set; }

        public FamilyDetail()
        {
            Spouses = new HashSet<Spouse>();
            Children = new HashSet<Child>();
        }
    }
}