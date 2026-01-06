using LawyersSyndicatePortal.Models;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LawyersSyndicatePortal.Filters;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Data.Entity;

namespace LawyersSyndicatePortal.Migrations
{
    // „·«ÕŸ…: Â–Â «·ÿ—Ìﬁ… ÂÌ «·√ﬂÀ— „ÊÀÊﬁÌ… · ÂÌ∆… «·»Ì«‰«  «·√Ê·Ì….
    // Ì „ «” œ⁄«¡ œ«·… Seed  ·ﬁ«∆Ì« »⁄œ ﬂ· ⁄„·Ì…  —ÕÌ· (Migration).
    internal sealed class Configuration : DbMigrationsConfiguration<LawyersSyndicatePortal.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true; // ﬁ„ » „ﬂÌ‰ «· —ÕÌ·«  «· ·ﬁ«∆Ì… · »”Ìÿ «·⁄„·Ì….
            AutomaticMigrationDataLossAllowed = true; // «”„Õ »›ﬁœ«‰ «·»Ì«‰«  ›Ì Õ«·  €Ì—  ÂÌﬂ·… «·ÃœÊ·.
        }

        protected override void Seed(LawyersSyndicatePortal.Models.ApplicationDbContext context)
        {
            // Â–« ÂÊ «·„ﬂ«‰ «·’ÕÌÕ · ÂÌ∆… «·»Ì«‰«  «·√Ê·Ì… (Seeding)
            // »„« ›Ì –·ﬂ «·’·«ÕÌ«  Ê«·√œÊ«— Ê«·„” Œœ„Ì‰.

            try
            {
                // ÌÃ» √‰  ﬂÊ‰ «·⁄„·Ì«  Â‰« „ “«„‰… (Synchronous)
                // · Ã‰» „‘«ﬂ· «· “«„‰ „⁄ Entity Framework.
                SeedPermissions(context).Wait();
                SeedDefaultRoles(context).Wait();
                SeedAdminPermissions(context).Wait();
                SeedDefaultAdminUser(context).Wait();
            }
            catch (Exception ex)
            {
                // ÌÃ»  ”ÃÌ· «·√Œÿ«¡ »Ê÷ÊÕ
                Debug.WriteLine($"An error occurred during seeding: {ex.Message}");
                // Ì„ﬂ‰ﬂ «Œ Ì«— ≈Ìﬁ«› «· ÿ»Ìﬁ √Ê  ”ÃÌ· «·Œÿ√ ›ﬁÿ
                throw;
            }
        }

        // ﬁ«„Ê” · —Ã„… √”„«¡ «·„ Õﬂ„«  Ê«·≈Ã—«¡«  ≈·Ï «·⁄—»Ì…
        private static readonly Dictionary<string, string> arabicNamesMap = new Dictionary<string, string>
        {
            // √”„«¡ «·„ Õﬂ„« 
            { "Home", "«·—∆Ì”Ì…" },
            { "Permissions", "«·’·«ÕÌ« " },
            { "Roles", "«·√œÊ«—" },
            { "Account", "«·Õ”«»« " },
            { "Manage", "«·≈œ«—…" },
            { "Audit", "«· œﬁÌﬁ" },

            // √”„«¡ «·≈Ã—«¡«  «·‘«∆⁄…
            { "Index", "⁄—÷" },
            { "Create", "≈‰‘«¡" },
            { "Edit", " ⁄œÌ·" },
            { "Delete", "Õ–›" },
            { "Details", "«· ›«’Ì·" },
            { "Update", " ÕœÌÀ" },
            { "Add", "≈÷«›…" },
            { "Remove", "≈“«·…" },
            { "Change", " €ÌÌ—" },

            // √”„«¡ «·≈Ã—«¡«  «·„Õœœ…
            { "UpdateRolePermissions", " ÕœÌÀ ’·«ÕÌ«  «·œÊ—" },
            { "CreateRole", "≈‰‘«¡ œÊ—" },
            { "EditRole", " ⁄œÌ· œÊ—" },
            { "DeleteRole", "Õ–› œÊ—" },
            { "Login", " ”ÃÌ· «·œŒÊ·" },
            { "Register", "«· ”ÃÌ·" },
            { "ForgotPassword", "‰”Ì  ﬂ·„… «·„—Ê—" },
            { "ResetPassword", "≈⁄«œ…  ⁄ÌÌ‰ ﬂ·„… «·„—Ê—" },
            { "GeneratePermissions", "≈‰‘«¡ «·’·«ÕÌ«   ·ﬁ«∆Ì«" },
            { "AdminRecoveryRequest", " ⁄œÌ· ﬂ·„… „—Ê— ··„” Œœ„Ì‰" }
        };

        private static string GetArabicName(string englishName)
        {
            return arabicNamesMap.ContainsKey(englishName) ? arabicNamesMap[englishName] : englishName;
        }

        private static async Task SeedPermissions(ApplicationDbContext context)
        {
            var existingPermissions = await context.Permissions.ToListAsync();
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
                    var permissionName = $"{controllerName}.{actionName}";
                    var permissionDescription = $"{GetArabicName(actionName)} ›Ì „ Õﬂ„ {GetArabicName(controllerName)}";
                    var lookupKey = $"{controllerName}_{actionName}";

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

        private static async Task SeedDefaultRoles(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            string[] roleNames = { "Admin", "Employee", "Lawyer", "Trainee", "Corrector" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminPermissions(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
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

        private static async Task SeedDefaultAdminUser(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

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
                    FullName = "„Õ„œ ⁄ÿÌ… „Õ„œ «·⁄„—Ì",
                    IdNumber = adminIdNumber
                };

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
        }
    }
}
