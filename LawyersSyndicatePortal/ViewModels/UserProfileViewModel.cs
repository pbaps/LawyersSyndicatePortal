// LawyersSyndicatePortal\ViewModels\UserProfileViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // تأكد من وجود هذه المكتبة للقوائم

namespace LawyersSyndicatePortal.ViewModels
{
    public class UserProfileViewModel // تم تغيير الاسم من UserViewModel إلى UserProfileViewModel
    {
        [Display(Name = "معرف المستخدم")]
        public string Id { get; set; }

        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "تاريخ إنشاء الحساب")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "هل المستخدم مرتبط بمحامٍ؟")]
        public bool IsLinkedToLawyer { get; set; }

        // خصائص جديدة لبيانات المكتب
        [Display(Name = "تفاصيل المكتب")]
        public OfficeDetailViewModel OfficeDetails { get; set; }

        [Display(Name = "الشركاء")]
        public List<PartnerViewModel> Partners { get; set; } = new List<PartnerViewModel>(); // تهيئة القائمة لتجنب NullReferenceException
    }
}
