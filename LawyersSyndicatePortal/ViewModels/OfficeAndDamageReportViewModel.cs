// LawyersSyndicatePortal/ViewModels/OfficeAndDamageReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير المكاتب وأضرارها وأضرار المنازل.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class OfficeAndDamageReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم الهوية / رقم العضوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "نوع ضرر المكتب")]
        public string OfficeDamageType { get; set; }

        [Display(Name = "نوع ضرر المنزل")]
        public string HomeDamageType { get; set; }

        [Display(Name = "المحافظة الحالية")]
        public string CurrentGovernorate { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        // خاصية جديدة لإظهار عدد نتائج البحث
        public int SearchResultsCount { get; set; }

        // -----------------------------------------------------------
        // الأعمدة المتاحة للاختيار في التقرير
        // -----------------------------------------------------------

        public List<ReportColumn> AvailableColumns { get; set; }

        public OfficeAndDamageReportViewModel()
        {
            // تهيئة قائمة الأعمدة المتاحة
            AvailableColumns = new List<ReportColumn>
            {
                new ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم الهوية", IsSelected = true },
                new ReportColumn { ColumnName = "LawyerFullName", DisplayName = "الاسم الكامل للمحامي", IsSelected = true },
                new ReportColumn { ColumnName = "CurrentGovernorate", DisplayName = "المحافظة الحالية", IsSelected = false },
                new ReportColumn { ColumnName = "OriginalGovernorate", DisplayName = "المحافظة الأصلية", IsSelected = false },
                new ReportColumn { ColumnName = "OfficeAddress", DisplayName = "عنوان المكتب", IsSelected = false },
                new ReportColumn { ColumnName = "OfficeDamageType", DisplayName = "نوع ضرر المكتب", IsSelected = false },
                new ReportColumn { ColumnName = "OfficeDamageDetails", DisplayName = "تفاصيل ضرر المكتب", IsSelected = false },
                new ReportColumn { ColumnName = "HomeDamageType", DisplayName = "نوع ضرر المنزل", IsSelected = false },
                new ReportColumn { ColumnName = "HomeDamageDetails", DisplayName = "تفاصيل ضرر المنزل", IsSelected = false }
            };
        }

        /// <summary>
        /// يمثل عموداً قابلاً للعرض في التقرير، مع اسم الخاصية واسم العرض وحالة الاختيار.
        /// </summary>
        public class ReportColumn
        {
            /// <summary>
            /// اسم الخاصية في الموديل (مثل "LawyerFullName").
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
    /// نموذج عرض مخصص لصفحة النتائج، يجمع البيانات من عدة نماذج (Lawyer, OfficeDetail, etc.)
    /// لتسهيل عرضها في الجدول.
    /// </summary>
    public class OfficeAndDamageReportResultViewModel
    {
        public List<OfficeAndDamageReportResultItem> Reports { get; set; }
        public List<OfficeAndDamageReportViewModel.ReportColumn> SelectedColumns { get; set; }
        public int SearchResultsCount { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير.
    /// </summary>
    public class OfficeAndDamageReportResultItem
    {
        public string LawyerIdNumber { get; set; }
        public string LawyerFullName { get; set; }
        public string CurrentGovernorate { get; set; }
        public string OriginalGovernorate { get; set; }
        public string OfficeAddress { get; set; }
        public string OfficeDamageType { get; set; }
        public string OfficeDamageDetails { get; set; }
        public string HomeDamageType { get; set; }
        public string HomeDamageDetails { get; set; }
    }
}
