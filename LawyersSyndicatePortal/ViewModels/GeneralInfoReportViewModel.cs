// LawyersSyndicatePortal/ViewModels/GeneralInfoReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لتضمين SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير المعلومات العامة.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class GeneralInfoReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم الهوية / رقم العضوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "هل يمارس المهنة الشرعية؟")]
        public bool? PracticesShariaLaw { get; set; }

        [Display(Name = "هل استلم مساعدات من النقابة؟")]
        public bool? ReceivedAidFromSyndicate { get; set; }

        [Display(Name = "نوع المساعدة المستلمة")]
        public string AidType { get; set; }

        // -----------------------------------------------------------
        // الأعمدة المتاحة للاختيار في التقرير (Available Columns)
        // -----------------------------------------------------------
        public List<ReportColumn> AvailableColumns { get; set; }

        /// <summary>
        /// فئة داخلية لتمثيل عمود في التقرير، مع اسمه وعنوان عرضه وحالة اختياره.
        /// </summary>
        public class ReportColumn
        {
            /// <summary>
            /// اسم الخاصية في نموذج النتيجة (مثل "LawyerFullName").
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
    /// نموذج عرض مخصص لصفحة النتائج، يجمع البيانات من عدة نماذج (Lawyer, GeneralInfo, ReceivedAid)
    /// لتسهيل عرضها في الجدول.
    /// </summary>
    public class GeneralInfoReportResultViewModel
    {
        public List<GeneralInfoReportResultItem> Reports { get; set; }
        public List<GeneralInfoReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير.
    /// </summary>
    public class GeneralInfoReportResultItem
    {
        public string LawyerIdNumber { get; set; }
        public string LawyerFullName { get; set; }
        public bool? PracticesShariaLaw { get; set; }
        public DateTime? ShariaLawPracticeStartDate { get; set; }
        public bool? ReceivedAidFromSyndicate { get; set; }
        public string ReceivedAidsDetails { get; set; } // لتجميع تفاصيل المساعدات
    }
}
