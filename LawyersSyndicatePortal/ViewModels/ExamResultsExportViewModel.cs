using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamResultsExportViewModel
    {
        [Display(Name = "عنوان الامتحان")]
        public string ExamTitle { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم المحامي")]
        public string LawyerName { get; set; }

        [Display(Name = "عدد الأسئلة الصحيحة")]
        public int CorrectAnswersCount { get; set; }

        [Display(Name = "إجمالي الأسئلة")]
        public int TotalQuestionsCount { get; set; }

        [Display(Name = "النسبة المئوية للنتيجة")]
        public double Score { get; set; }
    }
}