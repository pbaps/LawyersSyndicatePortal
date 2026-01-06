using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    // Define the enum outside the class. This is good practice.
    public enum QuestionType
    {
        MultipleChoice,
        TextAnswer
    }

    // The main model for a single question.
    public class Question
    {
        // Primary Key for the Question entity.
        [Key]
        public int Id { get; set; }

        // The text of the question, e.g., "What is the capital of France?".
        [Required(ErrorMessage = "نص السؤال مطلوب.")]
        [Display(Name = "نص السؤال")]
        public string Text { get; set; }

        // The type of question, using the QuestionType enum.
        // This is correctly defined and will be stored as an integer by Entity Framework.
        [Required(ErrorMessage = "نوع السؤال مطلوب.")]
        [Display(Name = "نوع السؤال")]
        public QuestionType QuestionType { get; set; }

        // The score value for the question. Using decimal for precision.
        [Required(ErrorMessage = "درجة السؤال مطلوبة.")]
        [Display(Name = "الدرجة")]
        public decimal Score { get; set; }

        // Optional path to an image associated with the question.
        [Display(Name = "مسار الصورة")]
        public string ImagePath { get; set; }

        // Correct answer text for questions that are not multiple choice.
        [Display(Name = "الإجابة الصحيحة")]
        public string CorrectAnswer { get; set; }

        // Optional duration for the question in seconds.
        // The '?' makes it a nullable integer, which is what you wanted for Excel import.
        [Display(Name = "مدة السؤال (بالثواني)")]
        public int? DurationSeconds { get; set; }

        // Flag to indicate if the question needs to be manually graded by a person.
        [Display(Name = "يتطلب تصحيحاً يدوياً؟")]
        public bool RequiresManualGrading { get; set; }

        // Foreign key to link this question to a specific exam.
        [ForeignKey("Exam")]
        public int ExamId { get; set; }
        public virtual Exam Exam { get; set; }

        // Navigation properties for related data.
        // ICollection represents a collection of related entities.
        public virtual ICollection<QuestionOption> Options { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }

    // This is the missing model for the options.
    public class QuestionOption
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نص الخيار مطلوب.")]
        [Display(Name = "نص الخيار")]
        public string Text { get; set; }

        [Display(Name = "إجابة صحيحة")]
        public bool IsCorrect { get; set; }

        // Foreign key to link this option back to its question.
        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
    }
}
