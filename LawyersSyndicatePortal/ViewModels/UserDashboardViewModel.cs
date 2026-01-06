// Path: LawyersSyndicatePortal\ViewModels\UserDashboardViewModel.cs
using LawyersSyndicatePortal.Models; // تأكد من وجود هذا الـ using لـ ApplicationUser و Broadcast
using System.Collections.Generic; // لـ List<T>
using System.ComponentModel.DataAnnotations; // لـ Display

namespace LawyersSyndicatePortal.ViewModels
{
    public class UserDashboardViewModel
    {
        // الخصائص الأساسية للمستخدم (مطلوبة لـ @Model.User.FullName وغيرها في View)
        public ApplicationUser User { get; set; }

        // إضافة الخاصية المفقودة RecentBroadcasts (مطلوبة لـ @Model.RecentBroadcasts في View)
        public List<Broadcast> RecentBroadcasts { get; set; }

        // الخصائص التي قدمتها في النسخة الجديدة
        [Display(Name = "الاسم الرباعي")]
        public string FullName { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        // خصائص جديدة لعدد الرسائل والتعميمات
        [Display(Name = "رسائل غير مقروءة")]
        public int UnreadMessagesCount { get; set; }

        [Display(Name = "تعميمات غير مقروءة")]
        public int UnreadBroadcastsCount { get; set; }

        [Display(Name = "هل توجد تعميمات جديدة؟")]
        public bool HasNewBroadcasts { get; set; }
        // NEW: قائمة الاختبارات المتاحة للمستخدم الحالي
        public ICollection<ExamAttendee> AvailableExams { get; set; }
    }
}
