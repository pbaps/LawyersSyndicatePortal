// LawyersSyndicatePortal\ViewModels\MyProfileViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // Required for SelectListItem if you use dropdowns

namespace LawyersSyndicatePortal.ViewModels
{
    public class MyProfileViewModel
    {
        // خصائص التحكم في خطوات النموذج (إذا كنت تخطط لنموذج متعدد الخطوات)
        public int CurrentStep { get; set; }
        public bool IsSubmitted { get; set; }

        // خصائص بيانات المحامي الأساسية التي تأتي من جدول Lawyer
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "الاسم الكامل للمحامي")]
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
        [StringLength(255)]
        public string TrainerLawyerName { get; set; }

        [Display(Name = "الجنس")]
        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }

        [Display(Name = "هل هو متدرب؟")]
        public bool IsTrainee { get; set; }

        // ViewModels لكل خطوة من خطوات النموذج الأخرى
        // تأكد من وجود هذه الملفات (PersonalDetailViewModel.cs, FamilyDetailViewModel.cs, HealthStatusViewModel.cs)
        // في نفس مجلد ViewModels أو في مجلدات فرعية مناسبة.
        public PersonalDetailViewModel PersonalDetail { get; set; }
        public FamilyDetailViewModel FamilyDetail { get; set; }
        public HealthStatusViewModel HealthStatus { get; set; }

        public MyProfileViewModel()
        {
            PersonalDetail = new PersonalDetailViewModel();
            FamilyDetail = new FamilyDetailViewModel();
            HealthStatus = new HealthStatusViewModel();
        }
    }
}