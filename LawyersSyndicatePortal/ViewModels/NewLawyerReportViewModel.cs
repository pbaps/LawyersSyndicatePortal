// LawyersSyndicatePortal\ViewModels\NewLawyerReportViewModel.cs
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة التقارير والاستعلامات المتخصصة.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class NewLawyerReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "الاسم الكامل للمحامي")]
        public string FullName { get; set; }

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; }

        /// <summary>
        /// قائمة من SelectListItem لملء قائمة الحالات المهنية المنسدلة.
        /// </summary>
        public List<SelectListItem> ProfessionalStatuses { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorate { get; set; }

        /// <summary>
        /// قائمة من SelectListItem لملء قائمة المحافظات المنسدلة.
        /// </summary>
        public List<SelectListItem> GovernorateList { get; set; }

        [Display(Name = "الحالة الاجتماعية")]
        public string MaritalStatus { get; set; }

        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "لديه أبناء؟")]
        public bool? HasChildren { get; set; }

        /// <summary>
        /// قائمة من SelectListItem لملء قائمة الحالات الاجتماعية المنسدلة.
        /// </summary>
        public List<SelectListItem> MaritalStatuses { get; set; }

        /// <summary>
        /// قائمة من SelectListItem لملء قائمة الجنس المنسدلة.
        /// </summary>
        public List<SelectListItem> Genders { get; set; }

        /// <summary>
        /// قائمة من SelectListItem لملء قائمة "لديه أبناء؟" المنسدلة.
        /// </summary>
        public List<SelectListItem> HasChildrenOptions { get; set; }

        [Display(Name = "رقم العضوية")]
        public string MembershipNumber { get; set; }

        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        // -----------------------------------------------------------
        // الخصائص لاختيار الأعمدة (Column Selection)
        // -----------------------------------------------------------

        /// <summary>
        /// قائمة بجميع الأعمدة المتاحة لتمكين المستخدم من الاختيار.
        /// </summary>
        public List<ReportColumn> AvailableColumns { get; set; }
        public List<AuditLog> LatestAuditLogs { get; internal set; }

        /// <summary>
        /// مُنشئ يقوم بتهيئة قائمة الأعمدة الافتراضية.
        /// </summary>
        public NewLawyerReportViewModel()
        {
            AvailableColumns = new List<ReportColumn>
            {
         // ... (هذه صحيحة بالفعل)
        new ReportColumn { ColumnName = "IdNumber", DisplayName = "رقم الهوية", IsSelected = false },
        new ReportColumn { ColumnName = "FullName", DisplayName = "الاسم الكامل", IsSelected = true },
        // ... (بقية الحقول المباشرة)

        // حقول تفاصيل العائلة (صحيحة)
        new ReportColumn { ColumnName = "FamilyDetails.MaritalStatus", DisplayName = "الحالة الاجتماعية", IsSelected = false },
        new ReportColumn { ColumnName = "FamilyDetails.NumberOfSpouses", DisplayName = "عدد الزوجات", IsSelected = false },
        new ReportColumn { ColumnName = "FamilyDetails.HasChildren", DisplayName = "هل يوجد أبناء؟", IsSelected = false },
        new ReportColumn { ColumnName = "FamilyDetails.NumberOfChildren", DisplayName = "عدد الأبناء", IsSelected = false },

        // حقول التفاصيل الشخصية (صحيحة)
        new ReportColumn { ColumnName = "PersonalDetails.Gender", DisplayName = "الجنس", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.EmailAddress", DisplayName = "عنوان البريد الالكتروني", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.OriginalGovernorate", DisplayName = "المحافظة الأصلية", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.CurrentGovernorate", DisplayName = "محافظة التواجد حاليًا", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.AccommodationType", DisplayName = "طبيعة السكن حاليًا", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.FullAddress", DisplayName = "العنوان كامل", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.MobileNumber", DisplayName = "رقم الجوال", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.AltMobileNumber1", DisplayName = "رقم جوال احتياطي 1", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.AltMobileNumber2", DisplayName = "رقم جوال احتياطي 2", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.WhatsAppNumber", DisplayName = "رقم الواتساب", IsSelected = false },
        new ReportColumn { ColumnName = "PersonalDetails.LandlineNumber", DisplayName = "رقم الهاتف الأرضي", IsSelected = false },
            };
        }

        /// <summary>
        /// فئة فرعية لتمثيل خيارات الأعمدة في التقرير.
        /// </summary>
        public class ReportColumn
        {
            /// <summary>
            /// اسم الخاصية في الموديل (مثل "FullName").
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// الاسم الذي يظهر في واجهة المستخدم (مثل "الاسم الكامل").
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// مؤشر يحدد ما إذا كان المستخدم قد اختار هذا العمود.
            /// </summary>
            public bool IsSelected { get; set; }
        }
    }
}
