// Path: LawyersSyndicatePortal\ViewModels\BroadcastListViewModel.cs
using LawyersSyndicatePortal.Models; // تأكد من وجود هذا الـ using لـ Broadcast
using System.Collections.Generic;
using System.Web.Mvc; // Required for SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class BroadcastListViewModel
    {
        // تم تغيير نوع الخاصية Broadcasts ليتناسب مع الاستخدام في MessagesController
        public List<UserBroadcastViewModel> Broadcasts { get; set; } // قائمة التعميمات للمستخدمين (مع حالة القراءة)
        public string SearchString { get; set; } // لسلسلة البحث
        public int CurrentPage { get; set; } // الصفحة الحالية
        public int PageSize { get; set; } // حجم الصفحة (عدد العناصر في الصفحة)
        public int TotalRecords { get; set; } // إجمالي عدد السجلات (التعميمات)
        public int TotalPages { get; set; } // إجمالي عدد الصفحات
        public List<SelectListItem> PageSizes { get; set; } = new List<SelectListItem> // خيارات أحجام الصفحات
        {
            new SelectListItem { Value = "10", Text = "10" },
            new SelectListItem { Value = "20", Text = "20" },
            new SelectListItem { Value = "50", Text = "50" }
        };
    }
}
