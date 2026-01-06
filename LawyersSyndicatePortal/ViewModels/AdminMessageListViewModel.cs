// Path: LawyersSyndicatePortal\ViewModels\AdminMessageListViewModel.cs
using LawyersSyndicatePortal.Models; // تأكد من وجود هذا الـ using لـ ContactMessage
using System.Collections.Generic;
using System.Web.Mvc; // Required for SelectListItem

namespace LawyersSyndicatePortal.ViewModels
{
    public class AdminMessageListViewModel
    {
        public List<ContactMessage> Messages { get; set; } // قائمة رسائل الاتصال
        public string SearchString { get; set; } // لسلسلة البحث
        public bool? IsReadFilter { get; set; } // فلتر حالة القراءة (مقروءة/غير مقروءة/الكل)
        public bool? IsRepliedFilter { get; set; } // 💡 Add this new property
        public int CurrentPage { get; set; } // الصفحة الحالية
        public int PageSize { get; set; } // حجم الصفحة (عدد العناصر في الصفحة)
        public int TotalMessages { get; set; } // إجمالي عدد السجلات (الرسائل)
        public int TotalPages { get; set; } // إجمالي عدد الصفحات
        public List<SelectListItem> PageSizes { get; set; } = new List<SelectListItem> // خيارات أحجام الصفحات
        {
            new SelectListItem { Value = "10", Text = "10" },
            new SelectListItem { Value = "20", Text = "20" },
            new SelectListItem { Value = "50", Text = "50" },
            new SelectListItem { Value = "100", Text = "100" }
        };
    }
}
