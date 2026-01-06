// Path: LawyersSyndicatePortal\Models\Exam.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الاختبار مطلوب.")]
        [Display(Name = "عنوان الاختبار")]
        public string Title { get; set; }

        [Display(Name = "وصف الاختبار")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تاريخ ووقت الاختبار مطلوب.")]
        [Display(Name = "تاريخ ووقت الاختبار")]
        public DateTime ExamDateTime { get; set; }

        [Required(ErrorMessage = "مدة الاختبار مطلوبة.")]
        [Display(Name = "مدة الاختبار (بالدقائق)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "نوع الاختبار")]
        public string ExamType { get; set; }

        [Required(ErrorMessage = "نسبة النجاح مطلوبة.")]
        [Range(0, 100, ErrorMessage = "يجب أن تكون نسبة النجاح بين 0 و 100.")]
        [Display(Name = "نسبة النجاح")]
        public decimal PassingScorePercentage { get; set; }

        [Display(Name = "الدرجة القصوى")]
        public decimal MaxScore { get; set; }

        [Display(Name = "هل الاختبار منشور؟")]
        public bool IsPublished { get; set; }

        [Display(Name = "هل الأسئلة عشوائية؟")]
        public bool IsRandomized { get; set; }

        [Display(Name = "هل يمكن إعادة الاختبار؟")]
        public bool CanRetake { get; set; }

        [Display(Name = "عدد أيام الإعادة")]
        public int RetakeDelayDays { get; set; }
        [Required]
        [Display(Name = "النتائج مرئية؟")]
        public bool IsResultVisible { get; set; }

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<ExamAttendee> ExamAttendees { get; set; }
        public virtual ICollection<ExamResult> ExamResults { get; set; }
    }
}