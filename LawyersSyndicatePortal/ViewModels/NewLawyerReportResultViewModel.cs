// LawyersSyndicatePortal\ViewModels\NewLawyerReportResultViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LawyersSyndicatePortal.Models;

namespace LawyersSyndicatePortal.ViewModels
{
    public class NewLawyerReportResultViewModel
    {
        public List<Lawyer> Lawyers { get; set; }
        public List<NewLawyerReportViewModel.ReportColumn> SelectedColumns { get; set; }
    }
}