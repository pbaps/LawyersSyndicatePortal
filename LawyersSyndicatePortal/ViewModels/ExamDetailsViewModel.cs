using LawyersSyndicatePortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ExamDetailsViewModel
    {
        public Exam Exam { get; set; }
        public int TotalQuestions { get; set; }
    }
}