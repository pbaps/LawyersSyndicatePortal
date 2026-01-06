// LawyersSyndicatePortal\Models\OfficeDetail.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class OfficeDetail
    {
        [Key]
        public int Id { get; set; } // **إعادة المفتاح الأساسي إلى int Id**

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; } // المفتاح الخارجي
        public virtual Lawyer Lawyer { get; set; } // خاصية التنقل

        // ... (بقية الخصائص) ...
        [StringLength(255)]
        [Display(Name = "اسم المكتب")]
        public string OfficeName { get; set; }

        [StringLength(500)]
        [Display(Name = "عنوان المكتب")]
        public string OfficeAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "نوع العقار")]
        public string PropertyType { get; set; }

        [StringLength(100)]
        [Display(Name = "حالة العقار")]
        public string PropertyStatus { get; set; }

        [Display(Name = "هل يوجد شركاء؟")]
        public bool HasPartners { get; set; }
        [Display(Name = "عدد الشركاء")]
        public int? NumberOfPartners { get; set; }

        public virtual ICollection<Partner> Partners { get; set; }

        public OfficeDetail()
        {
            Partners = new HashSet<Partner>();
        }
    }
}