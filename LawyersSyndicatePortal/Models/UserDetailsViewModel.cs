using System;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; }

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Display(Name = "الرقم الوطني")]
        public string IdNumber { get; set; }

        [Display(Name = "رقم هوية المحامي المرتبط")]
        public string LinkedLawyerIdNumber { get; set; }

        [Display(Name = "اسم المحامي المرتبط")]
        public string LinkedLawyerFullName { get; set; }

        [Display(Name = "الدور")]
        public string RoleName { get; set; } // اسم الدور للعرض

        [Display(Name = "تاريخ الإنشاء")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }
    }
}