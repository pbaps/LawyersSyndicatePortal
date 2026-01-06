// LawyersSyndicatePortal\ViewModels\LawyerDetailsViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; // لإضافة Any()

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerDetailsViewModel
    {
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم المحامي رباعي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; }

        [Display(Name = "رقم العضوية")]
        public string MembershipNumber { get; set; }

        [Display(Name = "تاريخ بدء المزاولة")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PracticeStartDate { get; set; }

        [Display(Name = "تاريخ بدء التدريب")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? TrainingStartDate { get; set; }

        [Display(Name = "اسم المحامي المدرب")]
        public string TrainerLawyerName { get; set; }

        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "متدرب")]
        public bool IsTrainee { get; set; }

        // --- الحقول الجديدة للبيانات المصرفية والمحفظة ---

        [Display(Name = "اسم البنك")]
        [StringLength(100, ErrorMessage = "اسم البنك لا يمكن أن يتجاوز 100 حرفاً")]
        public string BankName { get; set; }

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

        // خصائص البيانات الشخصية
        [Display(Name = "عنوان البريد الالكتروني")]
        public string EmailAddress { get; set; }

        [Display(Name = "رقم الجوال")]
        public string MobileNumber { get; set; }

        [Display(Name = "رقم جوال احتياطي 1")]
        public string AltMobileNumber1 { get; set; }

        [Display(Name = "رقم جوال احتياطي 2")]
        public string AltMobileNumber2 { get; set; }

        [Display(Name = "رقم الواتساب")]
        public string WhatsAppNumber { get; set; }

        [Display(Name = "رقم الهاتف الأرضي")]
        public string LandlineNumber { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorate { get; set; }

        [Display(Name = "طبيعة السكن حاليًا")]
        public string AccommodationType { get; set; }

        [Display(Name = "العنوان كامل")]
        public string FullAddress { get; set; }

        // خصائص تفاصيل العائلة
        [Display(Name = "الحالة الاجتماعية")]
        public string MaritalStatus { get; set; }

        [Display(Name = "عدد الزوجات")]
        public int? NumberOfSpouses { get; set; }

        [Display(Name = "هل لديك أبناء؟")]
        public bool HasChildren { get; set; }

        [Display(Name = "عدد الأبناء")]
        public int? NumberOfChildren { get; set; }

        public List<SpouseViewModel> Spouses { get; set; } = new List<SpouseViewModel>();
        public List<ChildViewModel> Children { get; set; } = new List<ChildViewModel>();

        // خصائص الحالة الصحية
        [Display(Name = "حالة المحامي الصحية")]
        public string LawyerCondition { get; set; }

        [Display(Name = "تفاصيل الإصابة")]
        public string InjuryDetails { get; set; }

        [Display(Name = "العلاج المطلوب")]
        public string TreatmentNeeded { get; set; }

        [Display(Name = "التشخيص الطبي")]
        public string LawyerDiagnosis { get; set; }

        [Display(Name = "هل يوجد أفراد عائلة مصابون؟")]
        public bool HasFamilyMembersInjured { get; set; }

        [Display(Name = "عدد أفراد العائلة المصابين")]
        public int? NumberOfFamilyMembersInjured { get; set; }

        public List<FamilyMemberInjuryViewModel> FamilyMemberInjuries { get; set; } = new List<FamilyMemberInjuryViewModel>();

        // خصائص تفاصيل المكتب
        [Display(Name = "اسم المكتب")]
        public string OfficeName { get; set; }

        [Display(Name = "عنوان المكتب")]
        public string OfficeAddress { get; set; }

        [Display(Name = "نوع العقار")]
        public string PropertyType { get; set; }

        [Display(Name = "حالة العقار")]
        public string PropertyStatus { get; set; }

        [Display(Name = "هل يوجد شركاء؟")]
        public bool HasPartners { get; set; }

        [Display(Name = "عدد الشركاء")]
        [Range(0, 50, ErrorMessage = "يجب أن يكون عدد الشركاء بين 0 و 50.")]
        public int? NumberOfPartners { get; set; }

        public List<PartnerViewModel> Partners { get; set; } = new List<PartnerViewModel>();

        // خصائص أضرار المنزل
        [Display(Name = "هل تعرض المنزل لأضرار؟")]
        public bool HasHomeDamage { get; set; }

        [Display(Name = "نوع ضرر المنزل")]
        public string HomeDamageType { get; set; }

        [Display(Name = "تفاصيل أضرار المنزل")]
        public string HomeDamageDetails { get; set; }

        // خصائص أضرار المكتب
        [Display(Name = "هل تعرض المكتب لأضرار؟")]
        public bool HasOfficeDamage { get; set; } // تم إضافة هذه الخاصية

        [Display(Name = "نوع ضرر المكتب")]
        public string OfficeDamageType { get; set; }

        [Display(Name = "تفاصيل أضرار المكتب")]
        public string OfficeDamageDetails { get; set; }

        // خصائص تفاصيل الاعتقال
        [Display(Name = "هل تعرض المحامي للاعتقال؟")]
        public bool WasDetained { get; set; }

        [Display(Name = "مدة الاعتقال")]
        public string DetentionDuration { get; set; }

        [Display(Name = "تاريخ بدء الاعتقال")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DetentionStartDate { get; set; }

        [Display(Name = "هل ما زال معتقلاً؟")]
        public bool IsStillDetained { get; set; }

        [Display(Name = "تاريخ الإفراج")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "نوع الاعتقال")]
        public string DetentionType { get; set; }

        [Display(Name = "مكان الاعتقال")]
        public string DetentionLocation { get; set; }

        // خصائص معلومات الزملاء
        [Display(Name = "يعلم بوجود شهداء من الزملاء")]
        public bool KnowsOfMartyrColleagues { get; set; }

        [Display(Name = "لديه شهداء في سجلاته")]
        public bool HasMartyrs { get; set; }

        public List<MartyrColleagueViewModel> MartyrColleagues { get; set; } = new List<MartyrColleagueViewModel>();

        [Display(Name = "يعلم بوجود معتقلين من الزملاء")]
        public bool KnowsOfDetainedColleagues { get; set; }

        [Display(Name = "لديه معتقلين في سجلاته")]
        public bool HasDetained { get; set; }

        public List<DetainedColleagueViewModel> DetainedColleagues { get; set; } = new List<DetainedColleagueViewModel>();

        [Display(Name = "يعلم بوجود مصابين من الزملاء")]
        public bool KnowsOfInjuredColleagues { get; set; }

        [Display(Name = "لديه مصابين في سجلاته")]
        public bool HasInjured { get; set; }

        public List<InjuredColleagueViewModel> InjuredColleagues { get; set; } = new List<InjuredColleagueViewModel>();

        // خصائص المعلومات العامة
        [Display(Name = "يمارس المهنة الشرعية")]
        public bool PracticesShariaLaw { get; set; }

        [Display(Name = "تاريخ الحصول على مزاولة المهنة الشرعية")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ShariaLawPracticeStartDate { get; set; }

        [Display(Name = "استلم مساعدات من النقابة")]
        public bool ReceivedAidFromSyndicate { get; set; }

        public List<ReceivedAidViewModel> ReceivedAids { get; set; } = new List<ReceivedAidViewModel>();

        // NEW: خصائص المرفقات
        [Display(Name = "مرفقات المحامي")]
        public List<LawyerAttachmentViewModel> LawyerAttachments { get; set; } = new List<LawyerAttachmentViewModel>();
    }
}
