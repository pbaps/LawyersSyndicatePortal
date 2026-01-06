using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ChildReportViewModel2
    {
        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم الابن/الابنة")]
        public string ChildName { get; set; }

        [Display(Name = "رقم هوية الابن/الابنة")]
        public string ChildIdNumber { get; set; }

        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; } // تم تصحيح النوع إلى DateTime?
    }
}