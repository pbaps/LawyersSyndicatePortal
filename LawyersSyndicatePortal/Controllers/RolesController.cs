using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using LawyersSyndicatePortal.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Net; // لـ HttpStatusCodeResult
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    // يجب أن يكون هذا الكنترولر متاحًا فقط للمسؤولين
    // تأكد من أن دور "Admin" موجود لديك
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController()
        {
            _context = new ApplicationDbContext();
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
        }

        // GET: Roles/Index
        // عرض قائمة بجميع الأدوار
        [PermissionAuthorizationFilter("عرض قائمة الأدوار", "صلاحية عرض قائمة الأدوار")]
        [AuditLog("عرض", "عرض قائمة الأدوار")]
        public ActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            var model = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();
            return View(model);
        }

        // GET: Roles/Create
        // عرض نموذج لإنشاء دور جديد
        [PermissionAuthorizationFilter("إنشاء دور جديد", "صلاحية عرض نموذج إنشاء دور جديد")]
        [AuditLog("عرض", "عرض نموذج إنشاء دور جديد")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // معالجة إرسال نموذج إنشاء دور جديد
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إنشاء دور جديد", "صلاحية إنشاء دور جديد وحفظه")]
        [AuditLog("إنشاء", "إنشاء دور جديد")]
        public async Task<ActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(model.Name);
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"تم إنشاء الدور '{model.Name}' بنجاح.";
                    return RedirectToAction("Index");
                }
                AddErrors(result); // إضافة أخطاء الهوية إلى ModelState
            }
            return View(model);
        }

        // GET: Roles/Edit/5
        // عرض نموذج لتعديل دور موجود
        [PermissionAuthorizationFilter("تعديل دور", "صلاحية عرض نموذج تعديل دور")]
        [AuditLog("عرض", "عرض نموذج تعديل دور")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            var model = new EditRoleViewModel { Id = role.Id, Name = role.Name };
            return View(model);
        }

        // POST: Roles/Edit/5
        // معالجة إرسال نموذج تعديل دور
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تعديل دور", "صلاحية تحديث دور وحفظه")]
        [AuditLog("تعديل", "تعديل دور")]
        public async Task<ActionResult> Edit(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"تم تحديث الدور '{model.Name}' بنجاح.";
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: Roles/Delete/5
        // عرض صفحة تأكيد حذف دور
        [PermissionAuthorizationFilter("حذف دور", "صلاحية عرض نموذج حذف دور")]
        [AuditLog("عرض", "عرض صفحة تأكيد حذف دور")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            var model = new RoleViewModel { Id = role.Id, Name = role.Name };
            return View(model);
        }

        // POST: Roles/Delete/5
        // معالجة إرسال طلب حذف دور
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حذف دور", "صلاحية حذف دور")]
        [AuditLog("حذف", "حذف دور")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }

            // تحقق مما إذا كان هناك مستخدمون مرتبطون بهذا الدور
            var usersInRole = role.Users.Any();
            if (usersInRole)
            {
                TempData["ErrorMessage"] = $"لا يمكن حذف الدور '{role.Name}' لأنه مرتبط بمستخدمين حاليًا.";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"تم حذف الدور '{role.Name}' بنجاح.";
                return RedirectToAction("Index");
            }
            AddErrors(result);
            return RedirectToAction("Index"); // في حالة وجود أخطاء أخرى
        }

        // GET: Roles/ManageUserRoles/userId
        // عرض نموذج لتعيين الأدوار لمستخدم معين
        [PermissionAuthorizationFilter("إدارة أدوار المستخدم", "صلاحية عرض نموذج إدارة أدوار المستخدم")]
        [AuditLog("عرض", "عرض نموذج إدارة أدوار المستخدم")]
        public async Task<ActionResult> ManageUserRoles(string userId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName // أو FullName إذا أردت عرض الاسم الكامل
            };

            // جلب جميع الأدوار
            model.AllRoles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();

            // جلب الأدوار الحالية للمستخدم
            var userRoles = await _userManager.GetRolesAsync(user.Id);
            model.UserAssignedRoles = userRoles.ToList();

            return View(model);
        }

        // POST: Roles/ManageUserRoles
        // معالجة إرسال نموذج تعيين الأدوار للمستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة أدوار المستخدم", "صلاحية تحديث أدوار المستخدم")]
        [AuditLog("تحديث", "تحديث أدوار المستخدم")]
        public async Task<ActionResult> ManageUserRoles(UserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // إذا لم يكن النموذج صالحًا، أعد ملء AllRoles واعرض الـ View مرة أخرى
                model.AllRoles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // جلب الأدوار الحالية للمستخدم
            var currentRoles = await _userManager.GetRolesAsync(user.Id);

            // إزالة الأدوار التي لم تعد محددة
            var rolesToRemove = currentRoles.Except(model.UserAssignedRoles ?? new List<string>()).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user.Id, rolesToRemove.ToArray());
                if (!removeResult.Succeeded)
                {
                    AddErrors(removeResult);
                    // أعد ملء AllRoles واعرض الـ View مرة أخرى في حالة الخطأ
                    model.AllRoles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                        .ToList();
                    return View(model);
                }
            }

            // إضافة الأدوار الجديدة المحددة
            var rolesToAdd = (model.UserAssignedRoles ?? new List<string>()).Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user.Id, rolesToAdd.ToArray());
                if (!addResult.Succeeded)
                {
                    AddErrors(addResult);
                    // أعد ملء AllRoles واعرض الـ View مرة أخرى في حالة الخطأ
                    model.AllRoles = _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                        .ToList();
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = $"تم تحديث أدوار المستخدم '{user.UserName}' بنجاح.";
            return RedirectToAction("Index", "Users"); // إعادة توجيه إلى صفحة إدارة المستخدمين
        }


        // دالة مساعدة لإضافة أخطاء الهوية إلى ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _roleManager.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}