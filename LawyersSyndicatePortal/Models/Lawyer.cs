// LawyersSyndicatePortal\Models\Lawyer.cs
using System;
using System.Collections.Generic; // تأكد من وجود هذه المكتبة لـ ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models // تأكد من مطابقة مساحة الاسم لمشروعك
{
    public class Lawyer
    {
        // رقم هوية المحامي (المفتاح الأساسي لجدول Lawyers)
        [Key]
        [StringLength(50)] // يجب أن يتطابق هذا الطول مع LinkedLawyerIdNumber في ApplicationUser
        [Display(Name = "رقم هوية المحامي")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "اسم المحامي رباعي مطلوب.")] // إضافة رسالة خطأ للتحقق من الصحة
        [StringLength(255)]
        [Display(Name = "اسم المحامي رباعي")]
        public string FullName { get; set; }

        [StringLength(100)]
        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; } // مثال: "فعال", "متدرب", "متقاعد"

        [Display(Name = "متدرب")]
        public bool IsTrainee { get; set; } // هل المحامي متدرب؟

        [StringLength(255)]
        [Display(Name = "اسم المحامي المدرب")]
        public string TrainerLawyerName { get; set; } // اسم المحامي المدرب (إذا كان متدرباً)

        [StringLength(50)]
        [Display(Name = "رقم العضوية")]
        public string MembershipNumber { get; set; } // اختياري

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ التدريب")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? TrainingStartDate { get; set; } // اختياري، يمكن أن يكون null

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ المزاولة")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PracticeStartDate { get; set; } // اختياري، يمكن أن يكون null

        [StringLength(10)]
        [Display(Name = "الجنس")]
        public string Gender { get; set; } // مثال: "ذكر", "أنثى"

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true; // قيمة افتراضية: المحامي يكون نشطًا عند إضافته

        // خاصية التنقل العكسية: لربط المحامي بالمستخدمين المرتبطين به
        // هذه الخاصية تمثل مجموعة المستخدمين الذين لديهم LinkedLawyerIdNumber يطابق IdNumber لهذا المحامي
        [InverseProperty("LinkedLawyer")] // تربط هذه الخاصية بـ 'LinkedLawyer' في فئة 'ApplicationUser'
        public virtual ICollection<ApplicationUser> LinkedUsers { get; set; }

        // *** الخصائص المضافة (خاصيات التنقل) ***
        public virtual ICollection<PersonalDetail> PersonalDetails { get; set; }
        public virtual ICollection<FamilyDetail> FamilyDetails { get; set; }
        public virtual ICollection<HealthStatus> HealthStatuses { get; set; }
        public virtual ICollection<OfficeDetail> OfficeDetails { get; set; }
        public virtual ICollection<HomeDamage> HomeDamages { get; set; }
        public virtual ICollection<OfficeDamage> OfficeDamages { get; set; }
        public virtual ICollection<DetentionDetail> DetentionDetails { get; set; }
        public virtual ICollection<ColleagueInfo> ColleagueInfos { get; set; }
        public virtual ICollection<GeneralInfo> GeneralInfos { get; set; } // إضافة خاصية التنقل لـ GeneralInfo
        public virtual ICollection<LawyerAttachment> LawyerAttachments { get; set; } // NEW: المرفقات


        // تهيئة قوائم الـ ICollection لتجنب NullReferenceException
        public Lawyer()
        {
            LinkedUsers = new HashSet<ApplicationUser>();
            // تهيئة مجموعة التفاصيل الشخصية للمحامي.
            PersonalDetails = new HashSet<PersonalDetail>();
            // تهيئة مجموعة تفاصيل عائلة المحامي (الزوجات والأبناء).
            FamilyDetails = new HashSet<FamilyDetail>();
            // تهيئة مجموعة الحالات الصحية للمحامي وأفراد عائلته المصابين.
            HealthStatuses = new HashSet<HealthStatus>();
            // تهيئة مجموعة تفاصيل مكتب المحامي.
            OfficeDetails = new HashSet<OfficeDetail>();
            // تهيئة مجموعة الأضرار التي لحقت بمنزل المحامي.
            HomeDamages = new HashSet<HomeDamage>();
            // تهيئة مجموعة الأضرار التي لحقت بمكتب المحامي.
            OfficeDamages = new HashSet<OfficeDamage>();
            // تهيئة مجموعة تفاصيل اعتقال المحامي.
            DetentionDetails = new HashSet<DetentionDetail>();
            // تهيئة مجموعة معلومات الزملاء (الشهداء، المعتقلين، المصابين).
            ColleagueInfos = new HashSet<ColleagueInfo>();
            // تهيئة مجموعة المعلومات العامة الإضافية عن المحامي.
            GeneralInfos = new HashSet<GeneralInfo>();
            // تهيئة مجموعة المرفقات الخاصة بالمحامي (مثل ملفات PDF).
            LawyerAttachments = new HashSet<LawyerAttachment>();
            // تأكد من تهيئة جميع مجموعات ICollection هنا
        }
    }
}
