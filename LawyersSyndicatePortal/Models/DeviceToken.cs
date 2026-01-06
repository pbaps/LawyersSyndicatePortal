using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class DeviceToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        [Display(Name = "معرف المستخدم")]
        public string UserId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "رمز الجهاز")]
        public string Token { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedDate { get; set; }
    }
}
