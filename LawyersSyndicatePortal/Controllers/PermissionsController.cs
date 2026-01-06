using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;
using LawyersSyndicatePortal.Models.ViewModels;
using LawyersSyndicatePortal.Filters;
using System.Reflection;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using LawyersSyndicatePortal.Utility;
using System.Web;

namespace LawyersSyndicatePortal.Controllers
{
    // تقييد الوصول لهذا الـ Controller للمسؤولين فقط
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationRoleManager _roleManager;

        public PermissionsController()
        {
            _context = new ApplicationDbContext();
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


        // GET: Permissions/Index
        // عرض قائمة بجميع الأدوار المتاحة في النظام
        [PermissionAuthorizationFilter("عرض الأدوار", "عرض قائمة بجميع الأدوار المتاحة")]
        [AuditLog("عرض الأدوار", "عرض قائمة بجميع الأدوار المتاحة")]
        public async Task<ActionResult> Index()
        {
            try
            {
                var allRoles = await RoleManager.Roles.ToListAsync();
                return View(allRoles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Permissions/ManageRolePermissions
        // عرض وإدارة الصلاحيات لدور معين
        [PermissionAuthorizationFilter("إدارة صلاحيات الدور", "عرض وإدارة الصلاحيات لدور معين")]
        [AuditLog("إدارة صلاحيات الدور", "عرض وإدارة الصلاحيات لدور معين")]
        public async Task<ActionResult> ManageRolePermissions(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "معرف الدور غير صالح أو مفقود.";
                    return RedirectToAction("Index");
                }
                var role = await RoleManager.FindByIdAsync(id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "خطأ: لم يتم العثور على الدور.";
                    return RedirectToAction("Index");
                }

                // جلب جميع الصلاحيات المتاحة
                var allPermissions = await _context.Permissions.ToListAsync();
                // جلب الصلاحيات المرتبطة بهذا الدور
                var rolePermissions = await _context.RolePermissions
                                                   .Where(rp => rp.RoleId == id)
                                                   .Select(rp => rp.PermissionId)
                                                   .ToListAsync();

                var viewModel = new RolePermissionsViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Permissions = allPermissions.Select(p => new PermissionViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        ControllerName = p.ControllerName,
                        ActionName = p.ActionName,
                        IsAssigned = rolePermissions.Contains(p.Id)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ وإعادة توجيه المستخدم برسالة خطأ واضحة
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // تحديث وحفظ الصلاحيات لدور معين
        // POST: Permissions/UpdateRolePermissions
        // تحديث وحفظ الصلاحيات لدور معين
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تحديث صلاحيات الدور", "تحديث وحفظ الصلاحيات لدور معين")]
        [AuditLog("تحديث صلاحيات الدور", "تحديث وحفظ الصلاحيات لدور معين")]
        public async Task<ActionResult> UpdateRolePermissions(string roleId, List<int> selectedPermissions)
        {
            try
            {
                // ** تعديل: إضافة فحص للتأكد من أن roleId ليس فارغاً **
                if (string.IsNullOrEmpty(roleId))
                {
                    TempData["ErrorMessage"] = "معرف الدور غير صالح أو مفقود عند الحفظ.";
                    return RedirectToAction("Index");
                }

                // التحقق من وجود الدور
                var role = await RoleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "خطأ: لم يتم العثور على الدور.";
                    return RedirectToAction("Index");
                }

                // حذف جميع الصلاحيات الحالية المرتبطة بالدور
                var existingPermissions = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
                _context.RolePermissions.RemoveRange(existingPermissions);
                await _context.SaveChangesAsync();

                // إضافة الصلاحيات الجديدة المحددة
                if (selectedPermissions != null && selectedPermissions.Count > 0)
                {
                    foreach (var permissionId in selectedPermissions)
                    {
                        var newRolePermission = new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = permissionId
                        };
                        _context.RolePermissions.Add(newRolePermission);
                    }
                    await _context.SaveChangesAsync();
                }

                // ** تعديل: إرجاع رسالة نجاح وإعادة التوجيه إلى نفس الصفحة **
                TempData["SuccessMessage"] = "تم تحديث صلاحيات الدور بنجاح.";
                return RedirectToAction("ManageRolePermissions", new { id = roleId });
            }
            catch (Exception ex)
            {
                // ** تعديل: إرجاع رسالة فشل وإعادة التوجيه إلى نفس الصفحة **
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء التحديث: {ex.Message}";
                return RedirectToAction("ManageRolePermissions", new { id = roleId });
            }
        }
        // GET: Permissions/GeneratePermissions
        // إضافة: هذا الإجراء الجديد يعيد صفحة الـ View
        [HttpGet]
     ///   [PermissionAuthorizationFilter("إنشاء الصلاحيات", "عرض صفحة لإنشاء الصلاحيات تلقائيًا")]
        [AuditLog("إنشاء الصلاحيات", "عرض صفحة لإنشاء الصلاحيات تلقائيًا")]
        public ActionResult GeneratePermissions()
        {
            return View();
        }

        // POST: Permissions/GeneratePermissions
        // إنشاء الصلاحيات تلقائياً بناءً على المتحكمات والإجراءات في التطبيق
        [HttpPost]
     ///   [PermissionAuthorizationFilter("إنشاء الصلاحيات", "إنشاء الصلاحيات تلقائياً بناءً على المتحكمات والإجراءات")]
        [AuditLog("إنشاء الصلاحيات", "إنشاء الصلاحيات تلقائيًا بناءً على المتحكمات والإجراءات")]
        public async Task<ActionResult> GeneratePermissionsPost()
        {
            try
            {
                // جلب جميع المتحكمات في التجميع الحالي
                var controllers = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass && typeof(Controller).IsAssignableFrom(type));

                var newPermissionsToAdd = new List<Permission>();

                // جلب الصلاحيات الموجودة بالفعل في قاموس لعمليات البحث السريعة بناءً على المتحكم والإجراء
                var existingPermissionsByAction = await _context.Permissions
                    .ToDictionaryAsync(p => $"{p.ControllerName}-{p.ActionName}");

                // جلب أسماء الصلاحيات الموجودة بالفعل في مجموعة (HashSet) لعمليات البحث السريعة بناءً على الاسم
                var existingPermissionNames = new HashSet<string>(await _context.Permissions.Select(p => p.Name).ToListAsync());

                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Replace("Controller", "");
                    var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(method => method.IsDefined(typeof(PermissionAuthorizationFilter), false) && !method.IsSpecialName);

                    foreach (var action in actions)
                    {
                        var permissionAttribute = action.GetCustomAttribute<PermissionAuthorizationFilter>();
                        var permissionName = permissionAttribute.PermissionName;
                        var permissionDescription = permissionAttribute.Description;
                        var actionName = action.Name;

                        // بناء مفتاح فريد للصلاحية للتحقق من وجودها بناءً على المتحكم والإجراء
                        var permissionKey = $"{controllerName}-{actionName}";

                        // التحقق من وجود الصلاحية بالفعل بناءً على المتحكم والإجراء
                        if (!existingPermissionsByAction.ContainsKey(permissionKey))
                        {
                            // التحقق الإضافي الجديد: التأكد من أن اسم الصلاحية (Name) غير موجود بالفعل
                            // هذا يحل مشكلة التعارض في الفهرس الفريد على عمود Name
                            if (!existingPermissionNames.Contains(permissionName))
                            {
                                var newPermission = new Permission
                                {
                                    Name = permissionName,
                                    ControllerName = controllerName,
                                    ActionName = actionName,
                                    Description = permissionDescription
                                };
                                newPermissionsToAdd.Add(newPermission);

                                // إضافة الصلاحية الجديدة إلى المجموعة والقواميس لتجنب تكرارها
                                // في نفس عملية الإنشاء
                                existingPermissionsByAction.Add(permissionKey, newPermission);
                                existingPermissionNames.Add(permissionName);
                            }
                        }
                    }
                }

                if (newPermissionsToAdd.Any())
                {
                    _context.Permissions.AddRange(newPermissionsToAdd);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"تم تحديث الصلاحيات بنجاح. تم إضافة {newPermissionsToAdd.Count} صلاحية جديدة.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"حدث خطأ في قاعدة البيانات أثناء إنشاء الصلاحيات: {dbEx.InnerException?.InnerException?.Message ?? "لا توجد تفاصيل إضافية."}";
                Debug.WriteLine($"DbUpdateException Inner Exception: {dbEx.InnerException?.InnerException?.Message ?? "لا توجد تفاصيل إضافية."}");
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException validationEx)
            {
                var errorMessages = validationEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                TempData["ErrorMessage"] = $"حدث خطأ في التحقق من صحة البيانات: {fullErrorMessage}";
                Debug.WriteLine($"Validation Exception: {fullErrorMessage}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء إنشاء الصلاحيات: {ex.Message}";
                Debug.WriteLine($"Unexpected Exception: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: Permissions/Create
        // عرض صفحة إضافة صلاحية جديدة
        [HttpGet]
        [PermissionAuthorizationFilter("إنشاء صلاحية يدوياً", "عرض صفحة إضافة صلاحية يدوية جديدة")]
        [AuditLog("إنشاء صلاحية يدوياً", "عرض صفحة إضافة صلاحية يدوية جديدة")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Permissions/Create
        // معالجة بيانات الصلاحية الجديدة وحفظها في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إنشاء صلاحية يدوياً", "معالجة بيانات الصلاحية الجديدة وحفظها في قاعدة البيانات")]
        [AuditLog("إنشاء صلاحية يدوياً", "معالجة بيانات الصلاحية الجديدة وحفظها في قاعدة البيانات")]
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,ControllerName,ActionName")] Permission permission)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Permissions.AnyAsync(p => p.Name == permission.Name))
                {
                    ModelState.AddModelError("Name", "صلاحية بهذا الاسم موجودة بالفعل.");
                    TempData["ErrorMessage"] = "صلاحية بهذا الاسم موجودة بالفعل.";
                    return View(permission);
                }

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة الصلاحية بنجاح.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "يرجى تصحيح الأخطاء في النموذج.";
            return View(permission);
        }

        // GET: Permissions/Edit/5
        // عرض صفحة تعديل صلاحية موجودة
        [PermissionAuthorizationFilter("تعديل صلاحية", "عرض صفحة تعديل صلاحية موجودة")]
        [AuditLog("تعديل صلاحية", "عرض صفحة تعديل صلاحية موجودة")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return HttpNotFound();
            }
            return View(permission);
        }

        // POST: Permissions/Edit/5
        // معالجة بيانات التعديل وحفظها في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تعديل صلاحية", "معالجة بيانات التعديل وحفظها في قاعدة البيانات")]
        [AuditLog("تعديل صلاحية", "معالجة بيانات التعديل وحفظها في قاعدة البيانات")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,ControllerName,ActionName")] Permission permission)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Permissions.AnyAsync(p => p.Name == permission.Name && p.Id != permission.Id))
                {
                    ModelState.AddModelError("Name", "صلاحية بهذا الاسم موجودة بالفعل.");
                    TempData["ErrorMessage"] = "صلاحية بهذا الاسم موجودة بالفعل.";
                    return View(permission);
                }

                _context.Entry(permission).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل الصلاحية بنجاح.";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "يرجى تصحيح الأخطاء في النموذج.";
            return View(permission);
        }

        // GET: Permissions/Delete/5
        // عرض صفحة تأكيد الحذف
        [PermissionAuthorizationFilter("حذف صلاحية", "عرض صفحة تأكيد حذف صلاحية")]
        [AuditLog("حذف صلاحية", "عرض صفحة تأكيد حذف صلاحية")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return HttpNotFound();
            }
            return View(permission);
        }

        // POST: Permissions/Delete/5
        // معالجة طلب الحذف
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حذف صلاحية", "معالجة طلب حذف صلاحية")]
        [AuditLog("حذف صلاحية", "معالجة طلب حذف صلاحية")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    TempData["ErrorMessage"] = "لم يتم العثور على الصلاحية المراد حذفها.";
                    return RedirectToAction("Index");
                }

                // حذف أي علاقات مرتبطة بهذه الصلاحية أولاً
                var rolePermissions = await _context.RolePermissions.Where(rp => rp.PermissionId == id).ToListAsync();
                _context.RolePermissions.RemoveRange(rolePermissions);
                await _context.SaveChangesAsync();

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الصلاحية بنجاح.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء الحذف: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
