// LawyersSyndicatePortal/ViewModels/LawyerAttachmentReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لتضمين SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير المرفقات.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class LawyerAttachmentReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم الهوية / رقم العضوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم الملف")]
        public string FileName { get; set; }

        [Display(Name = "نوع المرفق")]
        public string AttachmentType { get; set; }

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
    /// نموذج عرض مخصص لصفحة النتائج، يجمع البيانات من عدة نماذج (Lawyer, LawyerAttachment)
    /// لتسهيل عرضها في الجدول.
    /// </summary>
    public class LawyerAttachmentReportResultViewModel
    {
        public List<LawyerAttachmentReportResultItem> Reports { get; set; }
        public List<LawyerAttachmentReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير.
    /// تم إضافة خاصية Id هنا.
    /// </summary>
    public class LawyerAttachmentReportResultItem
    {
        public int Id { get; set; } // تم إضافة هذه الخاصية لتمرير معرف المرفق
        public string LawyerIdNumber { get; set; }
        public string LawyerFullName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } // سيبقى هذا المسار الفيزيائي في النموذج
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string AttachmentType { get; set; }
        public string Notes { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
