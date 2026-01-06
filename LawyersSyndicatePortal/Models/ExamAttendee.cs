using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class ExamAttendee
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Exam")]
        public int ExamId { get; set; }
        public virtual Exam Exam { get; set; }

        [ForeignKey("Lawyer")]
        public string LawyerIdNumber { get; set; }
        public virtual Lawyer Lawyer { get; set; }

        [Display(Name = "رقم الجوال")]
        [StringLength(20)]
        public string MobileNumber { get; set; }

        [Display(Name = "هل يحق له الالتحاق؟")]
        public bool CanAttend { get; set; }

        [Display(Name = "هل الاختبار مرئي له؟")]
        public bool IsExamVisible { get; set; }

        [Display(Name = "هل تم الانتهاء من الاختبار؟")]
        public bool IsCompleted { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Navigation properties
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ExamResult ExamResult { get; set; } // علاقة واحد لواحد
    }
}
