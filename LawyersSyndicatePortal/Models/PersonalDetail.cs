// LawyersSyndicatePortal\Models\PersonalDetail.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class PersonalDetail
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lawyer")]
        [StringLength(50)]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [StringLength(50)]
        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [StringLength(255)]
        [Display(Name = "عنوان البريد الالكتروني")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [StringLength(100)]
        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorate { get; set; }

        [StringLength(100)]
        [Display(Name = "طبيعة السكن حاليًا")]
        public string AccommodationType { get; set; }

        [StringLength(500)]
        [Display(Name = "العنوان كامل")]
        public string FullAddress { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الجوال")]
        [Phone]
        public string MobileNumber { get; set; }

        // الخصائص المضافة لحل الأخطاء
        [StringLength(20)]
        [Display(Name = "رقم جوال احتياطي 1")]
        [Phone]
        public string AltMobileNumber1 { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم جوال احتياطي 2")]
        [Phone]
        public string AltMobileNumber2 { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الواتساب")]
        public string WhatsAppNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الهاتف الأرضي")]
        [Phone]
        public string LandlineNumber { get; set; }

        // --- البيانات المصرفية والمحفظة ---

        [StringLength(100)]
        [Display(Name = "اسم البنك")]
        public string BankName { get; set; }

        [StringLength(100)]
        [Display(Name = "فرع البنك")]
        public string BankBranch { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم الحساب")]
        public string BankAccountNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم الايبان")]
        public string IBAN { get; set; }

        [StringLength(50)]
        [Display(Name = "نوع المحفظة")]
        public string WalletType { get; set; }

        [StringLength(100)]
        [Display(Name = "رقم حساب المحفظة")]
        public string WalletAccountNumber { get; set; }

        // خصائص أخرى قد تكون موجودة في ViewModel ولكنها غير موجودة هنا
        // إذا كنت لا تريد استخدامها، فتأكد من إزالتها من ViewModel أو معالجتها بشكل مناسب.
         [Display(Name = "تاريخ الميلاد")]
         [DataType(DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
         public DateTime? DateOfBirth { get; set; }

        // [StringLength(100)]
        // [Display(Name = "مكان الميلاد")]
        // public string PlaceOfBirth { get; set; }

        // [StringLength(100)]
        // [Display(Name = "الجنسية")]
        // public string Nationality { get; set; }
    }
}
