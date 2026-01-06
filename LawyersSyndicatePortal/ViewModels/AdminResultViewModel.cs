using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    // هذا الفيو موديل يُستخدم لتجميع البيانات لصفحة عرض النتائج الرئيسية
    public class AdminResultViewModel
    {
        [Display(Name = "اختر الاختبار")]
        public int ExamId { get; set; }

        // يمكن استخدام قائمة SelectList لتعبئة DropDownList في الفيو
        public IEnumerable<ExamViewModel> Exams { get; set; }

        // Added new properties to hold the categorized exam lists
        public List<ExamViewModel> ExamsToGrade { get; set; }
        public List<ExamViewModel> CompletedExams { get; set; }
    }

    // يُستخدم لعرض تفاصيل الاختبار في قائمة
 

    // --- الفيو موديل الخاص بصفحة عرض النتائج ---
    public class DisplayResultsViewModel
    {
        public int ExamId { get; set; }
        [Display(Name = "عنوان الاختبار")]
        public string ExamTitle { get; set; }

        public List<ExamAttendeeResultViewModel> AttendeesResults { get; set; }
    }

    public class ExamAttendeeResultViewModel
    {
        [Display(Name = "رقم الهوية")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "الدرجة التي حصل عليها")]
        public decimal TotalScoreAchieved { get; set; }

        [Display(Name = "الدرجة الكلية للاختبار")]
        public decimal TotalExamScore { get; set; }

        [Display(Name = "هل نجح؟")]
        public bool IsPassed { get; set; }

        [Display(Name = "هل تم الانتهاء؟")]
        public bool IsCompleted { get; set; }

        [Display(Name = "اكتمل التصحيح؟")]
        public bool IsGradingComplete { get; set; }
    }

    // --- الفيو موديل الخاص بصفحة التصحيح اليدوي ---
    public class ManualGradingViewModel
    {
        public int ExamId { get; set; }

        [Display(Name = "عنوان الاختبار")]
        public string ExamTitle { get; set; }

        public List<AnswerToGradeViewModel> AnswersToGrade { get; set; }
    }

    public class AnswerToGradeViewModel
    {
        public int AnswerId { get; set; }

        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "نص السؤال")]
        public string QuestionText { get; set; }

        [Display(Name = "إجابة المحامي")]
        public string UserAnswer { get; set; }

        [Display(Name = "الدرجة الكلية للسؤال")]
        public decimal QuestionScore { get; set; }
    }
}
