// LawyersSyndicatePortal/ViewModels/NewDetentionReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لتوفير SelectListItem
using LawyersSyndicatePortal.Models; // للتأكد من استخدام النماذج الصحيحة

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير تفاصيل الاعتقال للمحامين.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class NewDetentionReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم الهوية / رقم العضوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "هل ما زال معتقلاً؟")]
        public bool? IsStillDetained { get; set; } // Nullable bool للسماح بـ "الكل"

        [Display(Name = "نوع الاعتقال")]
        public string DetentionType { get; set; } // تتوافق مع DetentionType في DetentionDetail

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
    /// نموذج عرض مخصص لصفحة النتائج، يجمع البيانات من عدة نماذج (Lawyer, DetentionDetail)
    /// لتسهيل عرضها في الجدول.
    /// </summary>
    public class NewDetentionReportResultViewModel
    {
        public List<NewDetentionReportResultItem> Reports { get; set; }
        public List<NewDetentionReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير.
    /// </summary>
    public class NewDetentionReportResultItem
    {
        public string LawyerIdNumber { get; set; }
        public string LawyerFullName { get; set; }
        public bool WasDetained { get; set; } // هل تعرض المحامي للاعتقال؟
        public string DetentionDuration { get; set; } // مدة الاعتقال
        public DateTime? DetentionStartDate { get; set; } // تاريخ بدء الاعتقال
        public bool IsStillDetained { get; set; } // هل ما زال معتقلاً؟
        public DateTime? ReleaseDate { get; set; } // تاريخ الإفراج
        public string DetentionType { get; set; } // نوع الاعتقال
        public string DetentionLocation { get; set; } // مكان الاعتقال
    }
}
