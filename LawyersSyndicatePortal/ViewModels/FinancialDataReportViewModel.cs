// LawyersSyndicatePortal\ViewModels\FinancialDataReportViewModel.cs
using System.Collections.Generic;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.ViewModels
{
    public class FinancialDataReportViewModel
    {
        // حقول البحث
        public string FullName { get; set; }
        public string IdNumber { get; set; }
        public string MembershipNumber { get; set; }
        public string BankName { get; set; }

        // قوائم الخيارات
        public List<SelectListItem> BankList { get; set; }

        // خيارات الأعمدة
        public List<ReportColumn> AvailableColumns { get; set; }

        // نتيجة البحث
        public List<Lawyer> Lawyers { get; set; }

        public class ReportColumn
        {
            public string ColumnName { get; set; }
            public string DisplayName { get; set; }
            public bool IsSelected { get; set; }
        }
    }

    public class FinancialDataReportResultViewModel
    {
        public List<Lawyer> Lawyers { get; set; }
        public List<FinancialDataReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }
}