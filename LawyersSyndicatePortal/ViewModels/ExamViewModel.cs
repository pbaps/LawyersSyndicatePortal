using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using LawyersSyndicatePortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamViewModel
    {
        public Exam Exam { get; set; }
        // تحديد مساحة الاسم بشكل صريح
        public Models.Question Question { get; set; }
        public int CurrentQuestionNumber { get; set; }
        public int TotalQuestions { get; set; }
        public Lawyer Lawyer { get; set; }
        public int TimeRemainingSeconds { get; set; }


 

    }
}