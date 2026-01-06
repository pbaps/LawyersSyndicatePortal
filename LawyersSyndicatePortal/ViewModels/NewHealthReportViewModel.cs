// LawyersSyndicatePortal/ViewModels/NewHealthReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لتوفير SelectListItem
using LawyersSyndicatePortal.Models; // للتأكد من استخدام النماذج الصحيحة

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير الحالة الصحية للمحامين.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class NewHealthReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم الهوية / رقم العضوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "الحالة الصحية للمحامي")]
        public string LawyerHealthCondition { get; set; } // تتوافق مع LawyerCondition في HealthStatus

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; } // تتوافق مع ProfessionalStatus في Lawyer

        [Display(Name = "هل يوجد أفراد عائلة مصابون؟")]
        public bool? HasFamilyMembersInjured { get; set; } // تتوافق مع HasFamilyMembersInjured في HealthStatus (Nullable bool للسماح بـ "الكل")

        // -----------------------------------------------------------
        // الأعمدة المتاحة للاختيار في التقرير
        // -----------------------------------------------------------

        public List<ReportColumn> AvailableColumns { get; set; }

        /// <summary>
        /// نموذج فرعي يمثل عموداً في التقرير.
        /// </summary>
        public class ReportColumn
        {
            /// <summary>
            /// اسم الخاصية في نموذج النتائج (مثل "LawyerFullName").
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// الاسم الذي يظهر في واجهة المستخدم (مثل "الاسم الكامل للمحامي").
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// مؤشر يحدد ما إذا كان المستخدم قد اختار هذا العمود.
            /// </summary>
            public bool IsSelected { get; set; }
        }
    }

    /// <summary>
    /// نموذج عرض مخصص لصفحة النتائج، يجمع البيانات من عدة نماذج (Lawyer, HealthStatus, FamilyMemberInjury)
    /// لتسهيل عرضها في الجدول.
    /// </summary>
    public class NewHealthReportResultViewModel
    {
        public List<NewHealthReportResultItem> Reports { get; set; }
        public List<NewHealthReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير.
    /// </summary>
    public class NewHealthReportResultItem
    {
        public string LawyerIdNumber { get; set; }
        public string LawyerFullName { get; set; }
        public string LawyerHealthCondition { get; set; } // حالة المحامي الصحية
        public string ProfessionalStatus { get; set; } // الحالة المهنية للمحامي
        public bool? HasFamilyMembersInjured { get; set; } // هل يوجد أفراد عائلة مصابون
        public string FamilyMembersInjuredDetails { get; set; } // تفاصيل إصابات أفراد العائلة (اسم المصاب، طبيعة الإصابة، العلاقة)
        public int? NumberOfInjuredFamilyMembers { get; set; } // عدد أفراد العائلة المصابين
        public string LawyerInjuryDetails { get; set; } // طبيعة إصابة المحامي
        public string LawyerTreatmentNeeded { get; set; } // طبيعة العلاج للمحامي
        public string LawyerDiagnosis { get; set; } // تشخيص حالة المحامي
    }
}
