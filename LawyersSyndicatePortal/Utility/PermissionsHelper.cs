using System.Security.Claims;
using System.Linq;

namespace LawyersSyndicatePortal.Utility
{
    /// <summary>
    /// فئة مساعدة (Helper) للتعامل مع منطق الصلاحيات.
    /// </summary>
    public static class PermissionsHelper
    {
        /// <summary>
        /// دالة للتحقق مما إذا كان المستخدم يمتلك صلاحية معينة.
        /// </summary>
        /// <param name="user">كائن المستخدم الحالي.</param>
        /// <param name="permissionName">اسم الصلاحية المطلوب التحقق منها.</param>
        /// <returns>صحيح (True) إذا كان المستخدم يمتلك الصلاحية، وإلا (False).</returns>
        public static bool HasPermission(ClaimsPrincipal user, string permissionName)
        {
            // التحقق من وجود المستخدم و Authentication
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // إذا كان المستخدم لديه Role "Admin"، اعطه كل الصلاحيات.
            if (user.IsInRole("Admin"))
            {
                return true;
            }

            // التحقق مما إذا كان المستخدم لديه Claim يحمل اسم الصلاحية المطلوبة
            return user.HasClaim("Permission", permissionName);
        }
    }
}
