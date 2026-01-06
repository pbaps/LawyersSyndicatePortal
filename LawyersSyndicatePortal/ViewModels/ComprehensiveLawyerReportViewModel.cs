using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ComprehensiveLawyerReportViewModel
    {
        // --- قائمة الأعمدة المختارة ---
        public List<string> SelectedColumns { get; set; }

        // --- الفلاتر الأساسية ---
        [Display(Name = "الحالة المهنية")]
        public string SelectedProfessionalStatus { get; set; }

        [Display(Name = "الجنس")]
        public string SelectedGender { get; set; }

        [Display(Name = "المحافظة الحالية")]
        public List<string> SelectedCurrentGovernorates { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public List<string> SelectedOriginalGovernorates { get; set; }

        // --- فلاتر العائلة ---
        [Display(Name = "الحالة الاجتماعية")]
        public List<string> SelectedMaritalStatuses { get; set; } // تأكد أن الاسم بصيغة الجمع

        [Display(Name = "عدد الزوجات (يساوي)")]
        public int? SpousesCount { get; set; }

        [Display(Name = "عدد الأبناء (يساوي أو أكبر من)")]
        public int? ChildrenCountMin { get; set; }

        // --- فلاتر أعمار الأبناء ---
        [Display(Name = "عمر الابن (من سنة)")]
        public int? MinChildAge { get; set; }

        [Display(Name = "عمر الابن (إلى سنة)")]
        public int? MaxChildAge { get; set; }

        // --- فلتر إجمالي أفراد الأسرة ---
        [Display(Name = "إجمالي عدد أفراد الأسرة")]
        public int? TotalFamilyMembersMin { get; set; }

        // --- القوائم المنسدلة ---
        public MultiSelectList GovernorateList { get; set; }
        public SelectList GenderList { get; set; }
        public SelectList ProfessionalStatusList { get; set; }
        public MultiSelectList MaritalStatusList { get; set; }

        // --- النتائج ---
        public List<LawyerReportRow> Results { get; set; }

        public ComprehensiveLawyerReportViewModel()
        {
            Results = new List<LawyerReportRow>();
            SelectedCurrentGovernorates = new List<string>();
            SelectedOriginalGovernorates = new List<string>();
            SelectedMaritalStatuses = new List<string>();
            // الأعمدة الافتراضية
            SelectedColumns = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        }
    }

    // (بقية الكلاس LawyerReportRow كما هو عندك)
    public class LawyerReportRow
    {
        public string LawyerIdNumber { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string ProfessionalStatus { get; set; }
        public string MembershipNumber { get; set; }
        public string CurrentGovernorate { get; set; }
        public string OriginalGovernorate { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public int NumberOfSpouses { get; set; }
        public string SpousesNames { get; set; }
        public string SpousesIds { get; set; }
        public string SpousesMobiles { get; set; }
        public bool HasChildren { get; set; }
        public int NumberOfChildren { get; set; }
        public string ChildrenNames { get; set; }
        public string ChildrenGenders { get; set; }
        public string ChildrenDOBs { get; set; }
        public string ChildrenAges { get; set; }
        public int TotalFamilyMembers { get; set; }
    }
}