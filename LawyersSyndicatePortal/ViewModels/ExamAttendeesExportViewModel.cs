using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamAttendeesExportViewModel
    {
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم المحامي")]
        public string LawyerName { get; set; }

        [Display(Name = "عنوان الامتحان")]
        public string ExamTitle { get; set; }

        [Display(Name = "وقت البدء")]
        public DateTime StartTime { get; set; }

        [Display(Name = "وقت الانتهاء")]
        public DateTime EndTime { get; set; }

        [Display(Name = "هل اكتمل الامتحان؟")]
        public bool IsCompleted { get; set; }
    }
}