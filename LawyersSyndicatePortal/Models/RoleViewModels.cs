using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectListItem

namespace LawyersSyndicatePortal.Models
{
    // ViewModel لعرض قائمة الأدوار
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Display(Name = "اسم الدور")]
        public string Name { get; set; }
    }

    // ViewModel لإنشاء دور جديد
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "اسم الدور مطلوب.")]
        [StringLength(256, ErrorMessage = "يجب أن يكون اسم الدور بين {2} و {1} حرفًا.", MinimumLength = 2)]
        [Display(Name = "اسم الدور")]
        public string Name { get; set; }
    }

    // ViewModel لتعديل دور
    public class EditRoleViewModel
    {
        public string Id { get; set; } // معرف الدور
        [Required(ErrorMessage = "اسم الدور مطلوب.")]
        [StringLength(256, ErrorMessage = "يجب أن يكون اسم الدور بين {2} و {1} حرفًا.", MinimumLength = 2)]
        [Display(Name = "اسم الدور")]
        public string Name { get; set; }
    }

    // ViewModel لتعيين الأدوار للمستخدم
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "الأدوار المخصصة")]
        public List<string> UserAssignedRoles { get; set; } = new List<string>(); // الأدوار التي يمتلكها المستخدم حاليًا

        // قائمة بجميع الأدوار المتاحة للاختيار منها
        public List<SelectListItem> AllRoles { get; set; }
    }
}