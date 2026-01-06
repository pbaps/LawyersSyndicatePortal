// LawyersSyndicatePortal\Models\Partner.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Partner
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "معرف تفاصيل المكتب")]
        [ForeignKey("OfficeDetail")]
        public int OfficeDetailId { get; set; }
        public virtual OfficeDetail OfficeDetail { get; set; }

        [StringLength(255)]
        [Display(Name = "اسم الشريك")]
        public string PartnerName { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم عضويته")]
        public string PartnerMembershipNumber { get; set; }
    }
}