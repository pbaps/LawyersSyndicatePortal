// LawyersSyndicatePortal\Models\Child.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Child
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "معرف تفاصيل العائلة")]
        [ForeignKey("FamilyDetail")]
        public int FamilyDetailId { get; set; }
        public virtual FamilyDetail FamilyDetail { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "اسم الابن/الابنة")]
        public string ChildName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        [StringLength(10)]
        [Display(Name = "الجنس")]
        public string Gender { get; set; }
    }
}