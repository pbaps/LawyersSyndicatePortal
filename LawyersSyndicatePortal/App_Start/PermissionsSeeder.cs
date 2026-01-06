using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Filters; // يجب إضافة هذه المكتبة للوصول إلى Filter
using LawyersSyndicatePortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LawyersSyndicatePortal
{
    public static class PermissionsSeeder
    {
        // قاموس لترجمة أسماء المتحكمات والإجراءات إلى العربية
        // يمكن إضافة المزيد من الكلمات المفتاحية هنا لزيادة دقة الترجمة.
        private static readonly Dictionary<string, string> arabicNamesMap = new Dictionary<string, string>
        {
            // أسماء المتحكمات
            { "Home", "الرئيسية" },
            { "Permissions", "الصلاحيات" },
            { "Roles", "الأدوار" },
            { "Account", "الحسابات" },
            { "Manage", "الإدارة" },
            { "Audit", "التدقيق" },
             { "Search", "بحث" },
            
            // أسماء الإجراءات الشائعة
            { "Index", "عرض" },
            { "Create", "إنشاء" },
            { "Edit", "تعديل" },
            { "Delete", "حذف" },
            { "Details", "التفاصيل" },
            { "Update", "تحديث" },
            { "Add", "إضافة" },
            { "Remove", "إزالة" },
            { "Change", "تغيير" },
            
            // أسماء الإجراءات المحددة
            { "UpdateRolePermissions", "تحديث صلاحيات الدور" },
            { "CreateRole", "إنشاء دور" },
            { "EditRole", "تعديل دور" },
            { "DeleteRole", "حذف دور" },
            { "Login", "تسجيل الدخول" },
            { "Register", "التسجيل" },
            { "ForgotPassword", "نسيت كلمة المرور" },
            { "ResetPassword", "إعادة تعيين كلمة المرور" },
            { "GeneratePermissions", "إنشاء الصلاحيات تلقائيًا" },
            { "AdminRecoveryRequest", "تعديل كلمة مرور للمستخدمين" }
        };

        private static string GetArabicName(string englishName)
        {
            // هذا المنطق يعمل بشكل صحيح ويضمن الدقة.
            return arabicNamesMap.ContainsKey(englishName) ? arabicNamesMap[englishName] : englishName;
        }

        public static async Task SeedPermissionsAndRolesAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            await SeedAllPermissions(context);
            await SeedDefaultRoles(roleManager);
            await SeedAdminPermissions(context, roleManager);
            await SeedDefaultAdminUser(context, userManager, roleManager);
        }

        private static async Task SeedAllPermissions(ApplicationDbContext context)
        {
            var existingPermissions = await context.Permissions.ToListAsync();
            // استخدام Dictionary للبحث السريع عن الصلاحيات الموجودة بناءً على اسم المتحكم والإجراء
            var existingPermissionLookupByAction = existingPermissions
                .ToDictionary(p => $"{p.ControllerName}_{p.ActionName}", p => p);

            var newPermissions = new List<Permission>();

            var controllers = typeof(MvcApplication).Assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
                .ToList();

            foreach (var controller in controllers)
            {
                var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsDefined(typeof(PermissionAuthorizationFilter), false) && !m.IsSpecialName);

                foreach (var action in actions)
                {
                    var controllerName = controller.Name.Replace("Controller", "");
                    var actionName = action.Name;

                    // التعديل الرئيسي هنا: استخدام اسم فريد وغير قابل للتغيير كاسم للصلاحية
                    // هذا يضمن عدم حدوث تعارض في قاعدة البيانات
                    var permissionName = $"{controllerName}.{actionName}";

                    // إنشاء الوصف تلقائيًا بناءً على اسم المتحكم والإجراء
                    var permissionDescription = $"{GetArabicName(actionName)} في متحكم {GetArabicName(controllerName)}";

                    var lookupKey = $"{controllerName}_{actionName}";

                    // التحقق من عدم وجود صلاحية بنفس المتحكم والإجراء
                    if (!existingPermissionLookupByAction.ContainsKey(lookupKey))
                    {
                        var newPermission = new Permission
                        {
                            Name = permissionName,
                            ControllerName = controllerName,
                            ActionName = actionName,
                            Description = permissionDescription
                        };

                        newPermissions.Add(newPermission);
                        existingPermissionLookupByAction.Add(lookupKey, newPermission);
                    }
                }
            }

            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDefaultRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Employee", "Lawyer", "Trainee", "Corrector" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminPermissions(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            const string adminRoleName = "Admin";

            var adminRole = await roleManager.FindByNameAsync(adminRoleName);

            if (adminRole == null)
            {
                return;
            }

            var allPermissions = await context.Permissions.ToListAsync();

            var existingAdminPermissions = new HashSet<int>(await context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync());

            var permissionsToAdd = allPermissions
                .Where(p => !existingAdminPermissions.Contains(p.Id))
                .Select(p => new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id })
                .ToList();

            context.RolePermissions.AddRange(permissionsToAdd);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDefaultAdminUser(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminIdNumber = "900641960";
            string adminPassword = "123456*mM";
            string adminEmail = "admin@syndicate.com";

            var adminUser = await userManager.Users.SingleOrDefaultAsync(u => u.IdNumber == adminIdNumber);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "محمد عطية محمد العمري",
                    IdNumber = adminIdNumber
                };

                // تأكد من أن هذه القيم غير فارغة قبل المتابعة.
                if (string.IsNullOrEmpty(adminUser.FullName) || string.IsNullOrEmpty(adminUser.IdNumber))
                {
                    throw new InvalidOperationException("FullName and IdNumber cannot be null or empty.");
                }

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser.Id, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser.Id, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser.Id, "Admin");
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
