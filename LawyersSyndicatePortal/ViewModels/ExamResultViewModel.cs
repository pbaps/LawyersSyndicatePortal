using LawyersSyndicatePortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamResultViewModel
    {
        public bool IsGradingComplete { get; set; }
        public Exam Exam { get; set; }

        // تم إضافة هذه الخاصية
        public ExamAttendee ExamAttendee { get; set; }

        // 🎯 خصائص الدرجات
        public double TotalScoreAchieved { get; set; }
        public double TotalExamScore { get; set; }

        // 🎯 إجمالي الأسئلة
        public int TotalQuestions { get; set; }

        // 🎯 عدد الإجابات الصحيحة
        public int CorrectAnswersCount { get; set; }

        // 🎯 عدد الأسئلة المجابة
        public int TotalAnsweredQuestions { get; set; }

        // 🎯 عدد الأسئلة من نوع الاختيار من متعدد
        public int TotalMultipleChoiceQuestions { get; set; }

        // 🎯 عدد الأسئلة النصية
        public int TotalTextQuestions { get; set; }

        // خاصية لحساب النسبة المئوية المكتسبة (Read-only)
        public double PassPercentage
        {
            get
            {
                if (TotalExamScore > 0)
                {
                    return (TotalScoreAchieved / TotalExamScore) * 100;
                }
                return 0;
            }
        }

        // خاصية لحساب ما إذا كان الطالب قد نجح
        public bool IsPassed { get; set; }
    }
}
