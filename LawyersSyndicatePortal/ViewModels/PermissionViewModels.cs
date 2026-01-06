using System.Collections.Generic;

namespace LawyersSyndicatePortal.Models.ViewModels
{
    /// <summary>
    /// نموذج عرض (ViewModel) لتمثيل صلاحية واحدة.
    /// يستخدم لعرض تفاصيل الصلاحية في الـ Views.
    /// </summary>
    public class PermissionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool IsAssigned { get; set; }
    }

    /// <summary>
    /// نموذج عرض (ViewModel) لصفحة "عرض جميع الصلاحيات".
    /// يحتوي على قائمة من الـ PermissionViewModel.
    /// </summary>
    public class PermissionsViewModel
    {
        public List<PermissionViewModel> Permissions { get; set; }
    }

    /// <summary>
    /// نموذج عرض (ViewModel) لصفحة "إدارة صلاحيات الدور".
    /// يحتوي على تفاصيل الدور وقائمة الصلاحيات المرتبطة به.
    /// </summary>
    public class RolePermissionsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionViewModel> Permissions { get; set; }
    }
}
