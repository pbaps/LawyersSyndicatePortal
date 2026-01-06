using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamQuestionsExportViewModel
    {
        [Display(Name = "اسم الامتحان")]
        public string ExamTitle { get; set; }

        [Display(Name = "نص السؤال")]
        public string QuestionText { get; set; }

        [Display(Name = "هل هو سؤال متعدد الخيارات؟")]
        public bool IsMultipleChoice { get; set; }

        [Display(Name = "نص الخيار")]
        public string OptionText { get; set; }

        [Display(Name = "هل الخيار صحيح؟")]
        public bool IsCorrectOption { get; set; }
    }
}