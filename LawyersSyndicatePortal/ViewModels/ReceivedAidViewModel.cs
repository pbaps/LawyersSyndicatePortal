// LawyersSyndicatePortal\ViewModels\ReceivedAidViewModel.cs
using System; // إضافة هذا الـ namespace لـ DateTime
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ReceivedAidViewModel
    {
        public int Id { get; set; }
        public int GeneralInfoId { get; set; }

        [Display(Name = "نوع المساعدة")]
        [Required(ErrorMessage = "نوع المساعدة مطلوب.")]
        [StringLength(255)]
        public string AidType { get; set; }

        [Display(Name = "تاريخ الاستلام")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceivedDate { get; set; } // حقل جديد لتاريخ الاستلام
    }
}
