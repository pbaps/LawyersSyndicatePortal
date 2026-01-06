// Path: LawyersSyndicatePortal\Models\ExamResult.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class ExamResult
    {
        [Key, ForeignKey("ExamAttendee")]
        public int ExamAttendeeId { get; set; }
        public virtual ExamAttendee ExamAttendee { get; set; }

        [ForeignKey("Exam")]
        public int ExamId { get; set; }
        public virtual Exam Exam { get; set; }

        [ForeignKey("Lawyer")]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [Display(Name = "الدرجة الكلية للاختبار")]
        public decimal TotalExamScore { get; set; }

        [Display(Name = "الدرجة التي حصل عليها")]
        public decimal TotalScoreAchieved { get; set; }

        [Display(Name = "نسبة النجاح")]
        public decimal? PassPercentage { get; set; }

        [Display(Name = "هل نجح؟")]
        public bool? IsPassed { get; set; }

        [Display(Name = "اكتمل التصحيح؟")]
        public bool IsGradingComplete { get; set; }
    }
}