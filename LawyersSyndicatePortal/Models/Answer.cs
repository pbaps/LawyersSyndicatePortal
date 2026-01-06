// Path: LawyersSyndicatePortal\Models\Answer.cs

using System; // إضافة مساحة الاسم هذه لاستخدام DateTime
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        public string UserAnswer { get; set; }

        [Display(Name = "الدرجة المكتسبة")]
        public decimal? AcquiredScore { get; set; }

        public bool IsCorrect { get; set; }

        // الخصائص الجديدة المطلوبة
        [Display(Name = "تم التصحيح يدوياً")]
        public bool IsGraded { get; set; }

        [Display(Name = "تاريخ التصحيح")]
        public DateTime? CorrectionDate { get; set; }

        [Display(Name = "المصحح")]
        public string CorrectorId { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        [ForeignKey("ExamAttendee")]
        public int ExamAttendeeId { get; set; }
        public virtual ExamAttendee ExamAttendee { get; set; }
    }
}
