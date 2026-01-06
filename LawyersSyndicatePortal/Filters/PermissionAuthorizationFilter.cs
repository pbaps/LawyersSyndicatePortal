using System;
using System.Security.Claims;
using System.Web.Mvc;
using LawyersSyndicatePortal.Utility;

namespace LawyersSyndicatePortal.Filters
{
    public class PermissionAuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
    {
        // قم بتحويلها إلى خصائص عامة (public properties)
        public string PermissionName { get; }
        public string Description { get; }

        public PermissionAuthorizationFilter(string permissionName, string description)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentNullException(nameof(permissionName));
            }
            PermissionName = permissionName;
            Description = description;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // الحصول على كائن المستخدم الحالي
            var user = filterContext.HttpContext.User;

            // إذا لم يكن المستخدم مصادقًا، أعد توجيهه إلى صفحة تسجيل الدخول.
            if (!user.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            // التحقق من صلاحية المستخدم باستخدام ClaimsPrincipal
            var claimsPrincipal = user as ClaimsPrincipal;

            // إذا كان المستخدم ClaimsPrincipal و لديه الصلاحية المطلوبة
            if (claimsPrincipal != null && PermissionsHelper.HasPermission(claimsPrincipal, PermissionName))
            {
                // لا تفعل شيئًا، اترك العملية تستمر
                return;
            }

            // إذا لم يتم العثور على الصلاحية، أعد التوجيه إلى صفحة غير مصرح بها.
            filterContext.Result = new RedirectResult("~/Home/Unauthorized");
        }
    }
}
