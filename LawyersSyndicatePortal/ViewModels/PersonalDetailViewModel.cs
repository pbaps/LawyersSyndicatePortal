// LawyersSyndicatePortal\ViewModels\PersonalDetailViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لتوفير SelectListItem لقوائم الاختيار المنسدلة

namespace LawyersSyndicatePortal.ViewModels
{
    public class PersonalDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "الجنس")]
        [Required(ErrorMessage = "الرجاء تحديد الجنس")]
        public string Gender { get; set; }

        [Display(Name = "عنوان البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "الرجاء إدخال بريد إلكتروني صحيح")]
        [StringLength(255, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتجاوز 255 حرفاً")]
        public string EmailAddress { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        [Required(ErrorMessage = "الرجاء تحديد المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [Display(Name = "محافظة التواجد حالياً")]
        [Required(ErrorMessage = "الرجاء تحديد محافظة التواجد حالياً")]
        public string CurrentGovernorate { get; set; }

        [Display(Name = "طبيعة السكن حالياً")]
        [Required(ErrorMessage = "الرجاء تحديد طبيعة السكن حالياً")]
        public string AccommodationType { get; set; }

        [Display(Name = "العنوان كامل")]
        [StringLength(500, ErrorMessage = "العنوان لا يمكن أن يتجاوز 500 حرفاً")]
        public string FullAddress { get; set; }

        [Display(Name = "رقم الجوال")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم جوال صحيح")]
        [StringLength(20, ErrorMessage = "رقم الجوال لا يمكن أن يتجاوز 20 حرفاً")]
        public string MobileNumber { get; set; }

        [Display(Name = "رقم جوال احتياطي 1")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم جوال احتياطي صحيح")]
        [StringLength(20, ErrorMessage = "رقم الجوال لا يمكن أن يتجاوز 20 حرفاً")]
        public string AltMobileNumber1 { get; set; }

        [Display(Name = "رقم جوال احتياطي 2")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم جوال احتياطي صحيح")]
        [StringLength(20, ErrorMessage = "رقم الجوال لا يمكن أن يتجاوز 20 حرفاً")]
        public string AltMobileNumber2 { get; set; }

        [Display(Name = "رقم الواتساب")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم واتساب صحيح")]
        [StringLength(20, ErrorMessage = "رقم الواتساب لا يمكن أن يتجاوز 20 حرفاً")]
        public string WhatsAppNumber { get; set; }

        [Display(Name = "رقم الهاتف الأرضي")]
        [Phone(ErrorMessage = "الرجاء إدخال رقم هاتف أرضي صحيح")]
        [StringLength(20, ErrorMessage = "رقم الهاتف الأرضي لا يمكن أن يتجاوز 20 حرفاً")]
        public string LandlineNumber { get; set; }

        // --- الحقول الجديدة للبيانات المصرفية والمحفظة ---

        // 🔥 أضف الخصائص التالية
        [StringLength(100)]
        [Display(Name = "اسم البنك")]
        public string BankName { get; set; }
        public List<SelectListItem> BankList { get; set; }

        [Display(Name = "فرع البنك")]
        [StringLength(100, ErrorMessage = "فرع البنك لا يمكن أن يتجاوز 100 حرفاً")]
        public string BankBranch { get; set; }

        [Display(Name = "رقم الحساب")]
        [StringLength(50, ErrorMessage = "رقم الحساب لا يمكن أن يتجاوز 50 حرفاً")]
        public string BankAccountNumber { get; set; }

        [Display(Name = "رقم الايبان")]
        [StringLength(50, ErrorMessage = "رقم الايبان لا يمكن أن يتجاوز 50 حرفاً")]
        public string IBAN { get; set; }

        [Display(Name = "نوع المحفظة")]
        [StringLength(50, ErrorMessage = "نوع المحفظة لا يمكن أن يتجاوز 50 حرفاً")]
        public string WalletType { get; set; }

        [Display(Name = "رقم حساب المحفظة")]
        [StringLength(100, ErrorMessage = "رقم حساب المحفظة لا يمكن أن يتجاوز 100 حرفاً")]
        public string WalletAccountNumber { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        // قوائم الاختيار المنسدلة (Dropdown lists)
        public List<SelectListItem> Genders { get; set; }
        public List<SelectListItem> Governorates { get; set; }
        public List<SelectListItem> HousingTypes { get; set; }

        // --- تهيئة قوائم الاختيار المنسدلة
        public PersonalDetailViewModel()
        {
            Genders = new List<SelectListItem>
            {
                new SelectListItem { Value = "Male", Text = "ذكر" },
                new SelectListItem { Value = "Female", Text = "أنثى" }
            };

            Governorates = new List<SelectListItem>
            {
                new SelectListItem { Value = "NorthGaza", Text = "شمال غزة" },
                new SelectListItem { Value = "GazaCity", Text = "مدينة غزة" },
                new SelectListItem { Value = "MiddleArea", Text = "الوسطى" },
                new SelectListItem { Value = "KhanYounis", Text = "خانيونس" },
                new SelectListItem { Value = "Rafah", Text = "رفح" }
            };

            HousingTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Apartment", Text = "شقة" },
                new SelectListItem { Value = "SharedApartment", Text = "شقة مع باقي العائلة" },
                new SelectListItem { Value = "Hosted", Text = "استضافة عند أحد" },
                new SelectListItem { Value = "Tent", Text = "خيمة" },
                new SelectListItem { Value = "ShelterCenter", Text = "مركز إيواء" },
                new SelectListItem { Value = "OutsideGaza", Text = "خارج قطاع غزة" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };

            // تهيئة قائمة البنوك في الـ ViewModel
            BankList = new List<SelectListItem>
            {
                new SelectListItem { Value = "بنك فلسطين", Text = "بنك فلسطين" },
                new SelectListItem { Value = "البنك الإسلامي العربي", Text = "البنك الإسلامي العربي" },
                new SelectListItem { Value = "البنك الإسلامي الفلسطيني", Text = "البنك الإسلامي الفلسطيني" },
                new SelectListItem { Value = "البنك الوطني", Text = "البنك الوطني" },
                new SelectListItem { Value = "بنك القدس", Text = "بنك القدس" },
                new SelectListItem { Value = "البنك العربي", Text = "البنك العربي" }
            };
        }
    }
}