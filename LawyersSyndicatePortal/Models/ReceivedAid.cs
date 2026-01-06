// LawyersSyndicatePortal\Models\ReceivedAid.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class ReceivedAid
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("GeneralInfo")]
        public int GeneralInfoId { get; set; }
        public virtual GeneralInfo GeneralInfo { get; set; }

        [StringLength(255)]
        public string AidType { get; set; }

        [Display(Name = "تاريخ الاستلام")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceivedDate { get; set; } // إضافة خاصية تاريخ الاستلام
    }
}
