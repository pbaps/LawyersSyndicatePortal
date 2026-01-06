using LawyersSyndicatePortal.Models;
using System.Collections.Generic;

namespace LawyersSyndicatePortal.ViewModels
{
    /// <summary>
    /// هذا النموذج (ViewModel) مصمم خصيصًا لصفحة Index في متحكم ExamTaker.
    /// وظيفته هي تجميع قائمة الاختبارات النشطة (التي لم يتم أخذها بعد)
    /// وقائمة الاختبارات المكتملة (التي تم أخذها بالفعل)،
    /// وذلك لتقديمها إلى الـ View في كائن واحد.
    /// </summary>
    public class ExamListViewModel
    {
        // قائمة بالاختبارات المتاحة للمحامي والتي لم يتم أخذها بعد.
        public List<Exam> ActiveExams { get; set; }

        // قائمة بالاختبارات التي أكملها المحامي.
        public List<Exam> CompletedExams { get; set; }
    }
}
