// Path: LawyersSyndicatePortal\Models\PermissionModels.cs
// قم بإنشاء ملف جديد بهذا الاسم وأضف إليه الكود التالي

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Migrations; // إضافة هذه المكتبة
using System.Linq; // إضافة هذه المكتبة

namespace LawyersSyndicatePortal.Models
{
    // نموذج الصلاحية
    // هذا الجدول سيحتوي على جميع الصلاحيات المتاحة في النظام، مثل "CanViewLawyers", "CanEditLawyers"
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // إضافة خاصيتين جديدتين لربط الصلاحية بالمتحكم والإجراء
        [Required]
        [StringLength(256)]
        public string ControllerName { get; set; }

        [Required]
        [StringLength(256)]
        public string ActionName { get; set; }
    }

    // نموذج ربط الصلاحية بالدور (Role)
    // هذا الجدول يربط كل دور (Role) بالصلاحيات التي يمتلكها
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Role")]
        public string RoleId { get; set; }

        [Required]
        [ForeignKey("Permission")]
        public int PermissionId { get; set; }

        // خاصيات التنقل
        public virtual Microsoft.AspNet.Identity.EntityFramework.IdentityRole Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
