// Path: LawyersSyndicatePortal\\ViewModels\\UserBroadcastViewModel.cs
using LawyersSyndicatePortal.Models; // للتأكد من الوصول إلى نموذج Broadcast
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class UserBroadcastViewModel
    {
        public Broadcast Broadcast { get; set; }

        [Display(Name = "مقروءة")]
        public bool IsReadForCurrentUser { get; set; }
    }
}
