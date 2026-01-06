using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamAttendeeExportModel
    {
        public string ExamTitle { get; set; }
        public string LawyerIdNumber { get; set; }
        public string LawyerName { get; set; }
        public string LawyerMobile { get; set; }
        public bool CanAttend { get; set; }
        public bool IsExamVisible { get; internal set; }
    }
}