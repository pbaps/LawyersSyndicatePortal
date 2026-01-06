// LawyersSyndicatePortal/ViewModels/ColleagueReportViewModel.cs
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي لصفحة تقارير الزملاء.
    /// يحتوي على خيارات البحث واختيارات الأعمدة.
    /// </summary>
    public class ColleagueReportViewModel
    {
        // -----------------------------------------------------------
        // الخصائص لمرشحات البحث (Search Filters)
        // -----------------------------------------------------------

        [Display(Name = "اسم المحامي")]
        public string FullName { get; set; }

        [Display(Name = "نوع التقرير")]
        public string ReportType { get; set; }

        public List<SelectListItem> AvailableReportTypes { get; set; }

        [Display(Name = "تاريخ الحالة (من)")]
        [DataType(DataType.Date)]
        public DateTime? ReportDateStart { get; set; }

        [Display(Name = "تاريخ الحالة (إلى)")]
        [DataType(DataType.Date)]
        public DateTime? ReportDateEnd { get; set; }

        // خاصية تخزين عدد النتائج تم نقلها إلى ColleagueReportResultViewModel
        // public int SearchResultsCount { get; set; } 

        // -----------------------------------------------------------
        // الخصائص لاختيار الأعمدة
        // -----------------------------------------------------------

        public List<ReportColumn> AvailableColumns { get; set; }

        public List<string> SelectedColumns { get; set; } = new List<string>();


        // -----------------------------------------------------------
        // فئات داخلية لتحديد الأعمدة
        // -----------------------------------------------------------
        public class ReportColumn
        {
            public string ColumnName { get; set; }
            public string DisplayName { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        // -----------------------------------------------------------
        // تهيئة قائمة الأعمدة المتاحة
        // -----------------------------------------------------------
        public ColleagueReportViewModel()
        {
            AvailableReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "كل التقارير" },
                new SelectListItem { Value = "Martyr", Text = "شهداء" },
                new SelectListItem { Value = "Detained", Text = "معتقلين" },
                new SelectListItem { Value = "Injured", Text = "مصابين" }
            };

            AvailableColumns = new List<ReportColumn>
            {
                new ReportColumn { ColumnName = "LawyerFullName", DisplayName = "الاسم الكامل للمحامي" },
                new ReportColumn { ColumnName = "LawyerIdNumber", DisplayName = "رقم هوية المحامي" },
                new ReportColumn { ColumnName = "ColleagueFullName", DisplayName = "الاسم الكامل للزميل" },
             // new ReportColumn { ColumnName = "ColleagueIdNumber", DisplayName = "رقم هوية الزميل" },
                new ReportColumn { ColumnName = "ReportType", DisplayName = "نوع الحالة" },
             // new ReportColumn { ColumnName = "ReportDate", DisplayName = "تاريخ الحالة" },
             // new ReportColumn { ColumnName = "Description", DisplayName = "الوصف" },
                new ReportColumn { ColumnName = "MobileNumber", DisplayName = "رقم التواصل" }
            };
        }
    }

    /// <summary>
    /// نموذج عرض مخصص لصفحة النتائج.
    /// </summary>
    public class ColleagueReportResultViewModel
    {
        public List<ColleagueReportResultItem> Reports { get; set; }
        public List<ColleagueReportViewModel.ReportColumn> SelectedColumns { get; set; }

        // تم إضافة الخاصية SearchResultsCount إلى هذا الكلاس لحل الخطأ
        public int SearchResultsCount { get; set; }
    }

    /// <summary>
    /// عنصر واحد في قائمة نتائج التقرير، يجمع البيانات من عدة نماذج.
    /// </summary>
    public class ColleagueReportResultItem
    {
        public string LawyerFullName { get; set; }
        public string LawyerIdNumber { get; set; }
        public string ColleagueFullName { get; set; }
        ///  public string ColleagueIdNumber { get; set; }
        public string ReportType { get; set; }
        ///  public DateTime? ReportDate { get; set; }
        //   public string Description { get; set; }
        public string MobileNumber { get; set; }
    }
}
