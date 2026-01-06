using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using LawyersSyndicatePortal.Models;
using System.Collections.Generic;
using LawyersSyndicatePortal.ViewModels;
using System;
using System.IO;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using LawyersSyndicatePortal.Filters;
using System.Web.UI;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public UsersController()
        {
            db = new ApplicationDbContext();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
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

        [PermissionAuthorizationFilter("إدارة المستخدمين", "عرض المستخدمين")]
        [AuditLog("عرض", "عرض قائمة المستخدمين")]
        public async Task<ActionResult> Index(string searchString, int? page, int? pageSize)
        {
            int currentPage = page ?? 1;
            int current_pageSize = pageSize ?? 10;

            ViewBag.CurrentPage = currentPage;
            ViewBag.PageSize = current_pageSize;
            ViewBag.CurrentFilter = searchString;

            // الخطوة 1: الحصول على استعلام أساسي للمستخدمين باستخدام الخاصية UserManager.
            // هذا يضمن أن الكائن ليس فارغًا.
            var usersQuery = UserManager.Users.AsQueryable();

            // الخطوة 2: تطبيق فلتر البحث إذا تم توفير سلسلة بحث.
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email.Contains(searchString) ||
                    u.FullName.Contains(searchString) ||
                    u.IdNumber.Contains(searchString));
            }

            // الخطوة 3: حساب العدد الإجمالي للسجلات التي تطابق الفلتر.
            int totalRecords = await usersQuery.CountAsync();
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize);

            // الخطوة 4: تنفيذ استعلام فعال للحصول على المستخدمين المصفحين.
            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((currentPage - 1) * current_pageSize)
                .Take(current_pageSize)
                .ToListAsync();

            // الخطوة 5: تهيئة قائمة جديدة لعرض البيانات المطلوبة.
            var userList = new List<UserViewModel>();

            // الخطوة 6: التكرار على كل مستخدم لجلب الأدوار والمحامي المرتبط بشكل منفصل.
            foreach (var user in users)
            {
                // تم التعديل: استخدم UserManager لجلب الأدوار بشكل صحيح.
                var roles = await UserManager.GetRolesAsync(user.Id);

                // جلب المحامي المرتبط بشكل منفصل.
                var linkedLawyer = await db.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == user.LinkedLawyerIdNumber);

                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    IdNumber = user.IdNumber,
                    UserName = user.UserName,
                    CreationDate = user.CreationDate,
                    Roles = string.Join(", ", roles),
                    LinkedLawyerFullName = linkedLawyer?.FullName,
                    LinkedLawyerIdNumber = user.LinkedLawyerIdNumber
                });
            }

            ViewBag.PageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10", Selected = current_pageSize == 10 },
                new SelectListItem { Value = "25", Text = "25", Selected = current_pageSize == 25 },
                new SelectListItem { Value = "50", Text = "50", Selected = current_pageSize == 50 },
                new SelectListItem { Value = "100", Text = "100", Selected = current_pageSize == 100 }
            };

            return View(userList);
        }
        // GET: Users/Create (Display form to create a new user)
        [PermissionAuthorizationFilter("إدارة المستخدمين", "إنشاء مستخدم جديد")]
        [AuditLog("عرض", "عرض صفحة إنشاء مستخدم جديد")]
        public ActionResult Create()
        {
            var roles = RoleManager.Roles.ToList().Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();

            var lawyers = db.Lawyers.ToList().Select(l => new SelectListItem
            {
                Value = l.IdNumber,
                Text = l.FullName + " (" + l.IdNumber + ")"
            }).ToList();

            var model = new UserCreateViewModel
            {
                Roles = roles,
                Lawyers = lawyers
            };
            return View(model);
        }

        // POST: Users/Create (Process new user creation data)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة المستخدمين", "إنشاء مستخدم جديد")]
        [AuditLog("إضافة", "إنشاء مستخدم جديد")]
        public async Task<ActionResult> Create(UserCreateViewModel model)
        {
            // Repopulate dropdown lists in case of model errors
            var roles = RoleManager.Roles.ToList().Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();
            model.Roles = roles;

            var lawyers = db.Lawyers.ToList().Select(l => new SelectListItem
            {
                Value = l.IdNumber,
                Text = l.FullName + " (" + l.IdNumber + ")"
            }).ToList();
            model.Lawyers = lawyers;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    IdNumber = model.IdNumber,
                    LinkedLawyerIdNumber = model.LinkedLawyerIdNumber,
                    CreationDate = System.DateTime.Now
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.RoleId))
                    {
                        var roleName = RoleManager.FindById(model.RoleId)?.Name;
                        if (roleName != null)
                        {
                            await UserManager.AddToRoleAsync(user.Id, roleName);
                        }
                    }

                    TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح.";
                    return RedirectToAction("Index");
                }

                AddErrors(result);
            }

            return View(model);
        }

        // GET: Users/Edit/5 (Display form to edit an existing user)
        [PermissionAuthorizationFilter("إدارة المستخدمين", "تعديل مستخدم")]
        [AuditLog("عرض", "عرض صفحة تعديل مستخدم")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = RoleManager.Roles.ToList().Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            string currentRoleId = null;
            if (userRoles.Any())
            {
                var currentRole = await RoleManager.FindByNameAsync(userRoles.First());
                if (currentRole != null)
                {
                    currentRoleId = currentRole.Id;
                }
            }

            var lawyers = db.Lawyers.ToList().Select(l => new SelectListItem
            {
                Value = l.IdNumber,
                Text = l.FullName + " (" + l.IdNumber + ")",
                // هنا نقوم بتحديد المحامي المرتبط بالقيمة
                Selected = l.IdNumber == user.LinkedLawyerIdNumber
            }).ToList();

            // إضافة خيار فارغ في بداية القائمة
            lawyers.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "--- اختر محامياً (اختياري) ---"
            });

            var model = new UserCreateViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IdNumber = user.IdNumber,
                LinkedLawyerIdNumber = user.LinkedLawyerIdNumber,
                RoleId = currentRoleId,
                Roles = roles,
                Lawyers = lawyers
            };

            // ملاحظة: لا حاجة لإضافة Password أو ConfirmPassword إلى الـ ViewModel
            // لأننا لا نريد عرضها. الـ Validation سيكون في الـ POST action.

            return View(model);
        }

        // POST: Users/Edit/5 (Process user edit data)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة المستخدمين", "تعديل مستخدم")]
        [AuditLog("تعديل", "تعديل بيانات مستخدم")]
        public async Task<ActionResult> Edit(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user to be updated
                var user = await UserManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update basic user information
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;
                user.IdNumber = model.IdNumber;
                user.LinkedLawyerIdNumber = model.LinkedLawyerIdNumber;

                // **IMPORTANT**: Check if a new password was provided.
                // The Password field on the view has a placeholder "اترك فارغاً لعدم تغيير كلمة المرور"
                // which tells us to only update the password if a value is entered.
                if (!string.IsNullOrEmpty(model.Password))
                {
                    // First, remove the old password
                    var passwordResetToken = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var result = await UserManager.ResetPasswordAsync(user.Id, passwordResetToken, model.Password);

                    if (!result.Succeeded)
                    {
                        // If password update failed, add the errors to the model state
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        // Repopulate dropdowns and return the view
                        model.Roles = RoleManager.Roles.ToList().Select(r => new SelectListItem { Value = r.Id, Text = r.Name });
                        model.Lawyers = db.Lawyers.ToList().Select(l => new SelectListItem { Value = l.IdNumber, Text = l.FullName + " (" + l.IdNumber + ")" });
                        return View(model);
                    }
                }

                // Update user roles
                var userRoles = await UserManager.GetRolesAsync(user.Id);
                var selectedRole = await RoleManager.FindByIdAsync(model.RoleId);
                if (!userRoles.Contains(selectedRole.Name))
                {
                    await UserManager.RemoveFromRolesAsync(user.Id, userRoles.ToArray());
                    await UserManager.AddToRoleAsync(user.Id, selectedRole.Name);
                }

                // Update the user details
                var updateResult = await UserManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "تم تعديل المستخدم بنجاح.";
                    return RedirectToAction("Index");
                }
                else
                {
                    // If the user update failed for any other reason, add errors and return view
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            // If ModelState is not valid, re-populate dropdowns and return the view
            model.Roles = RoleManager.Roles.ToList().Select(r => new SelectListItem { Value = r.Id, Text = r.Name });
            model.Lawyers = db.Lawyers.ToList().Select(l => new SelectListItem { Value = l.IdNumber, Text = l.FullName + " (" + l.IdNumber + ")" });
            return View(model);
        }

        // GET: Users/Details/5 (Display details of an existing user)
        [PermissionAuthorizationFilter("إدارة المستخدمين", "عرض تفاصيل المستخدم")]
        [AuditLog("عرض", "عرض تفاصيل مستخدم")]
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            string roleName = "لا يوجد دور";
            if (userRoles.Any())
            {
                roleName = string.Join(", ", userRoles);
            }

            string linkedLawyerFullName = "لا يوجد محامٍ مرتبط";
            if (!string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                var linkedLawyer = await db.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == user.LinkedLawyerIdNumber);
                if (linkedLawyer != null)
                {
                    linkedLawyerFullName = linkedLawyer.FullName;
                }
            }

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IdNumber = user.IdNumber,
                UserName = user.UserName,
                LinkedLawyerIdNumber = user.LinkedLawyerIdNumber,
                LinkedLawyerFullName = linkedLawyerFullName,
                RoleName = roleName,
                CreationDate = user.CreationDate
            };

            return View(model);
        }

        // GET: Users/Delete/5 (Display confirmation page for user deletion)
        [PermissionAuthorizationFilter("إدارة المستخدمين", "حذف مستخدم")]
        [AuditLog("عرض", "عرض صفحة حذف مستخدم")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            string roleName = "لا يوجد دور";
            if (userRoles.Any())
            {
                roleName = string.Join(", ", userRoles);
            }

            string linkedLawyerFullName = "لا يوجد محامٍ مرتبط";
            if (!string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                var linkedLawyer = await db.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == user.LinkedLawyerIdNumber);
                if (linkedLawyer != null)
                {
                    linkedLawyerFullName = linkedLawyer.FullName;
                }
            }

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IdNumber = user.IdNumber,
                UserName = user.UserName,
                LinkedLawyerIdNumber = user.LinkedLawyerIdNumber,
                LinkedLawyerFullName = linkedLawyerFullName,
                RoleName = roleName,
                CreationDate = user.CreationDate
            };

            return View(model);
        }

        // POST: Users/Delete/5 (Process user deletion after confirmation)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة المستخدمين", "حذف مستخدم")]
        [AuditLog("حذف", "حذف مستخدم")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // الخطوة 1: حذف الرسائل المرتبطة بالمستخدم
            // يجب عليك العثور على الرسائل التي يكون المستخدم هو المُرسل أو المُستلم
            // (اعتمادًا على كيفية تصميم جدول الرسائل الخاص بك).

            // مثال (إذا كان لديك جدول رسائل):
            var messages = db.Messages.Where(m => m.SenderId == id || m.ReceiverId == id).ToList();
            if (messages.Any())
            {
                db.Messages.RemoveRange(messages);
                await db.SaveChangesAsync();
            }

            // الخطوة 2: الآن يمكنك حذف المستخدم بأمان
            var result = await UserManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح.";
                return RedirectToAction("Index");
            }
            else
            {
                // ... (إدارة الأخطاء إذا فشل الحذف)
                AddErrors(result);
                return View("Delete", new UserDetailsViewModel { Id = user.Id, FullName = user.FullName, Email = user.Email });
            }
        }

        // GET: Users/LawyerDetails/{lawyerIdNumber} - Display full lawyer data
        [HttpGet]
        [PermissionAuthorizationFilter("إدارة المستخدمين", "عرض تفاصيل محامٍ")]
        [AuditLog("عرض", "عرض تفاصيل محامٍ")]
        public async Task<ActionResult> LawyerDetails(string lawyerIdNumber)
        {
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Lawyer ID Number is required.");
            }

            // Fetch lawyer data with all related entities
            var lawyer = await db.Lawyers
                                .Include(l => l.PersonalDetails)
                                .Include(l => l.FamilyDetails.Select(fd => fd.Spouses))
                                .Include(l => l.FamilyDetails.Select(fd => fd.Children))
                                .Include(l => l.HealthStatuses.Select(hs => hs.FamilyMemberInjuries))
                                .Include(l => l.OfficeDetails.Select(od => od.Partners))
                                .Include(l => l.HomeDamages)
                                .Include(l => l.OfficeDamages)
                                .Include(l => l.DetentionDetails)
                                .Include(l => l.ColleagueInfos.Select(ci => ci.MartyrColleagues))
                                .Include(l => l.ColleagueInfos.Select(ci => ci.DetainedColleagues))
                                .Include(l => l.ColleagueInfos.Select(ci => ci.InjuredColleagues))
                                .Include(l => l.GeneralInfos.Select(gi => gi.ReceivedAids))
                                .FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);

            if (lawyer == null)
            {
                return HttpNotFound("Lawyer not found.");
            }

            // Get the first detention detail, or null if none exist
            var firstDetentionDetail = lawyer.DetentionDetails.FirstOrDefault();

            // Map data to LawyerDetailsViewModel
            var model = new LawyerDetailsViewModel
            {
                // Lawyer Basic Info
                LawyerIdNumber = lawyer.IdNumber,
                LawyerFullName = lawyer.FullName,
                ProfessionalStatus = lawyer.ProfessionalStatus,
                MembershipNumber = lawyer.MembershipNumber,
                PracticeStartDate = lawyer.PracticeStartDate,
                TrainingStartDate = lawyer.TrainingStartDate,
                TrainerLawyerName = lawyer.TrainerLawyerName,
                Gender = lawyer.Gender,
                IsActive = lawyer.IsActive,
                IsTrainee = lawyer.IsTrainee,

                // Personal Details
                EmailAddress = lawyer.PersonalDetails.FirstOrDefault()?.EmailAddress,
                OriginalGovernorate = lawyer.PersonalDetails.FirstOrDefault()?.OriginalGovernorate,
                CurrentGovernorate = lawyer.PersonalDetails.FirstOrDefault()?.CurrentGovernorate,
                AccommodationType = lawyer.PersonalDetails.FirstOrDefault()?.AccommodationType,
                FullAddress = lawyer.PersonalDetails.FirstOrDefault()?.FullAddress,
                MobileNumber = lawyer.PersonalDetails.FirstOrDefault()?.MobileNumber,
                AltMobileNumber1 = lawyer.PersonalDetails.FirstOrDefault()?.AltMobileNumber1,
                AltMobileNumber2 = lawyer.PersonalDetails.FirstOrDefault()?.AltMobileNumber2,
                WhatsAppNumber = lawyer.PersonalDetails.FirstOrDefault()?.WhatsAppNumber,
                LandlineNumber = lawyer.PersonalDetails.FirstOrDefault()?.LandlineNumber,

                // Family Details
                MaritalStatus = lawyer.FamilyDetails.FirstOrDefault()?.MaritalStatus,
                NumberOfSpouses = lawyer.FamilyDetails.FirstOrDefault()?.NumberOfSpouses ?? 0,
                HasChildren = lawyer.FamilyDetails.FirstOrDefault()?.HasChildren ?? false,
                NumberOfChildren = lawyer.FamilyDetails.FirstOrDefault()?.NumberOfChildren ?? 0,
                Spouses = lawyer.FamilyDetails.FirstOrDefault()?.Spouses?.Select(s => new SpouseViewModel
                {
                    Id = s.Id,
                    FamilyDetailId = s.FamilyDetailId,
                    SpouseName = s.SpouseName,
                    SpouseIdNumber = s.SpouseIdNumber,
                    SpouseMobileNumber = s.SpouseMobileNumber
                }).ToList() ?? new List<SpouseViewModel>(),
                Children = lawyer.FamilyDetails.FirstOrDefault()?.Children?.Select(c => new ChildViewModel
                {
                    Id = c.Id,
                    FamilyDetailId = c.FamilyDetailId,
                    ChildName = c.ChildName,
                    DateOfBirth = c.DateOfBirth,
                    IdNumber = c.IdNumber,
                    Gender = c.Gender
                }).ToList() ?? new List<ChildViewModel>(),

                // Health Status
                LawyerCondition = lawyer.HealthStatuses.FirstOrDefault()?.LawyerCondition,
                InjuryDetails = lawyer.HealthStatuses.FirstOrDefault()?.InjuryDetails,
                TreatmentNeeded = lawyer.HealthStatuses.FirstOrDefault()?.TreatmentNeeded,
                LawyerDiagnosis = lawyer.HealthStatuses.FirstOrDefault()?.LawyerDiagnosis,
                HasFamilyMembersInjured = lawyer.HealthStatuses.FirstOrDefault()?.HasFamilyMembersInjured ?? false,
                NumberOfFamilyMembersInjured = lawyer.HealthStatuses.FirstOrDefault()?.FamilyMembersInjured ?? 0,
                FamilyMemberInjuries = lawyer.HealthStatuses.FirstOrDefault()?.FamilyMemberInjuries?.Select(fmi => new FamilyMemberInjuryViewModel
                {
                    Id = fmi.Id,
                    HealthStatusId = fmi.HealthStatusId,
                    InjuredFamilyMemberName = fmi.InjuredFamilyMemberName,
                    RelationshipToLawyer = fmi.RelationshipToLawyer,
                    InjuryDetails = fmi.InjuryDetails
                }).ToList() ?? new List<FamilyMemberInjuryViewModel>(),

                // Office Details
                OfficeName = lawyer.OfficeDetails.FirstOrDefault()?.OfficeName,
                OfficeAddress = lawyer.OfficeDetails.FirstOrDefault()?.OfficeAddress,
                PropertyType = lawyer.OfficeDetails.FirstOrDefault()?.PropertyType,
                PropertyStatus = lawyer.OfficeDetails.FirstOrDefault()?.PropertyStatus,
                HasPartners = lawyer.OfficeDetails.FirstOrDefault()?.HasPartners ?? false,
                NumberOfPartners = lawyer.OfficeDetails.FirstOrDefault()?.NumberOfPartners ?? 0,
                Partners = lawyer.OfficeDetails.FirstOrDefault()?.Partners?.Select(p => new PartnerViewModel
                {
                    Id = p.Id,
                    OfficeDetailId = p.OfficeDetailId,
                    PartnerName = p.PartnerName,
                    MembershipNumber = p.PartnerMembershipNumber
                }).ToList() ?? new List<PartnerViewModel>(),

                // Home Damage
                HasHomeDamage = lawyer.HomeDamages.FirstOrDefault()?.HasHomeDamage ?? false,
                HomeDamageType = lawyer.HomeDamages.FirstOrDefault()?.DamageType,
                HomeDamageDetails = lawyer.HomeDamages.FirstOrDefault()?.DamageDetails,

                // Office Damage
                HasOfficeDamage = lawyer.OfficeDamages.Any(), // Check if any office damage record exists
                OfficeDamageType = lawyer.OfficeDamages.FirstOrDefault()?.DamageType,
                OfficeDamageDetails = lawyer.OfficeDamages.FirstOrDefault()?.DamageDetails,

                // Detention Details - Modified to handle non-nullable DateTime properties
                WasDetained = firstDetentionDetail?.WasDetained ?? false,
                DetentionDuration = firstDetentionDetail?.DetentionDuration,
                // Accessing DateTime properties safely:
                DetentionStartDate = firstDetentionDetail != null ? firstDetentionDetail.DetentionStartDate : (DateTime?)null,
                IsStillDetained = firstDetentionDetail?.IsStillDetained ?? false,
                ReleaseDate = firstDetentionDetail != null ? firstDetentionDetail.ReleaseDate : (DateTime?)null,
                DetentionType = firstDetentionDetail?.DetentionType,
                DetentionLocation = firstDetentionDetail?.DetentionLocation,

                // Colleague Info
                KnowsOfMartyrColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.KnowsOfMartyrColleagues ?? false,
                HasMartyrs = lawyer.ColleagueInfos.FirstOrDefault()?.HasMartyrs ?? false,
                MartyrColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.MartyrColleagues?.Select(mc => new MartyrColleagueViewModel
                {
                    Id = mc.Id,
                    ColleagueInfoId = mc.ColleagueInfoId,
                    MartyrName = mc.MartyrName,
                    ContactNumber = mc.ContactNumber
                }).ToList() ?? new List<MartyrColleagueViewModel>(),
                KnowsOfDetainedColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.KnowsOfDetainedColleagues ?? false,
                HasDetained = lawyer.ColleagueInfos.FirstOrDefault()?.HasDetained ?? false,
                DetainedColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.DetainedColleagues?.Select(dc => new DetainedColleagueViewModel
                {
                    Id = dc.Id,
                    ColleagueInfoId = dc.ColleagueInfoId,
                    DetainedName = dc.DetainedName,
                    ContactNumber = dc.ContactNumber
                }).ToList() ?? new List<DetainedColleagueViewModel>(),
                KnowsOfInjuredColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.KnowsOfInjuredColleagues ?? false,
                HasInjured = lawyer.ColleagueInfos.FirstOrDefault()?.HasInjured ?? false,
                InjuredColleagues = lawyer.ColleagueInfos.FirstOrDefault()?.InjuredColleagues?.Select(ic => new InjuredColleagueViewModel
                {
                    Id = ic.Id,
                    ColleagueInfoId = ic.ColleagueInfoId,
                    InjuredName = ic.InjuredName,
                    ContactNumber = ic.ContactNumber
                }).ToList() ?? new List<InjuredColleagueViewModel>(),

                // General Info
                PracticesShariaLaw = lawyer.GeneralInfos.FirstOrDefault()?.PracticesShariaLaw ?? false,
                ShariaLawPracticeStartDate = lawyer.GeneralInfos.FirstOrDefault()?.ShariaLawPracticeStartDate,
                ReceivedAidFromSyndicate = lawyer.GeneralInfos.FirstOrDefault()?.ReceivedAidFromSyndicate ?? false,
                ReceivedAids = lawyer.GeneralInfos.FirstOrDefault()?.ReceivedAids?.Select(ra => new ReceivedAidViewModel
                {
                    Id = ra.Id,
                    GeneralInfoId = ra.GeneralInfoId,
                    AidType = ra.AidType,
                    ReceivedDate = ra.ReceivedDate
                }).ToList() ?? new List<ReceivedAidViewModel>()
            };

            return View(model);
        }

        // Action to export users data to Excel
        [PermissionAuthorizationFilter("إدارة المستخدمين", "تصدير بيانات المستخدمين")]
        [AuditLog("تصدير", "تصدير بيانات المستخدمين إلى Excel")]
        public async Task<ActionResult> ExportUsersToExcel()
        {
            var users = await UserManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await UserManager.GetRolesAsync(user.Id);
                string linkedLawyerFullName = null;
                if (!string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
                {
                    var lawyer = await db.Lawyers.FirstOrDefaultAsync(l => l.IdNumber == user.LinkedLawyerIdNumber);
                    linkedLawyerFullName = lawyer?.FullName;
                }

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    IdNumber = user.IdNumber,
                    LinkedLawyerIdNumber = user.LinkedLawyerIdNumber, // Keep the ID number for linking
                    LinkedLawyerFullName = linkedLawyerFullName,
                    Roles = string.Join(", ", roles),
                    CreationDate = user.CreationDate
                });
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Set headers in the first row (Arabic for display, but map to English properties internally)
                worksheet.Cells[1, 1].Value = "البريد الإلكتروني";
                worksheet.Cells[1, 2].Value = "الاسم الكامل";
                worksheet.Cells[1, 3].Value = "رقم الهوية";
                worksheet.Cells[1, 4].Value = "رقم هوية المحامي المرتبط";
                worksheet.Cells[1, 5].Value = "الأدوار";
                worksheet.Cells[1, 6].Value = "تاريخ إنشاء الحساب";

                // Apply bold style to headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Align headers right
                }

                // Populate data starting from the second row
                for (int i = 0; i < userViewModels.Count; i++)
                {
                    var user = userViewModels[i];
                    worksheet.Cells[i + 2, 1].Value = user.Email;
                    worksheet.Cells[i + 2, 2].Value = user.FullName;
                    worksheet.Cells[i + 2, 3].Value = user.IdNumber;
                    worksheet.Cells[i + 2, 4].Value = user.LinkedLawyerIdNumber; // Use IdNumber for linking
                    worksheet.Cells[i + 2, 5].Value = user.Roles;
                    worksheet.Cells[i + 2, 6].Value = user.CreationDate?.ToShortDateString(); // Format date

                    worksheet.Cells[i + 2, 6].Style.Numberformat.Format = "yyyy-MM-dd"; // Ensure date format in Excel
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set content type and file name for download
                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UsersData.xlsx");
            }
        }

        // Action to download an empty Excel template for users import
        // Action to download an empty Excel template for users import
        [PermissionAuthorizationFilter("إدارة المستخدمين", "تنزيل نموذج استيراد المستخدمين")]
        [AuditLog("تنزيل", "تنزيل نموذج استيراد المستخدمين")]
        public ActionResult DownloadUsersExcelTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UsersTemplate");

                // Define headers for the template (matching import logic)
                worksheet.Cells[1, 1].Value = "البريد الإلكتروني";
                worksheet.Cells[1, 2].Value = "الاسم الكامل";
                worksheet.Cells[1, 3].Value = "رقم الهوية";
                worksheet.Cells[1, 4].Value = "رقم هوية المحامي المرتبط";
                worksheet.Cells[1, 5].Value = "تاريخ إنشاء الحساب";
                worksheet.Cells[1, 6].Value = "كلمة المرور"; // Password for new users
                worksheet.Cells[1, 7].Value = "الأدوار"; // Comma-separated roles

                // Apply bold style to headers
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Align headers right
                }

                // Add a sample row (optional, but helpful for user)
                worksheet.Cells[2, 1].Value = "sample@example.com";
                worksheet.Cells[2, 2].Value = "اسم المستخدم التجريبي";
                worksheet.Cells[2, 3].Value = "123456789";
                worksheet.Cells[2, 4].Value = "LAWYER123";
                worksheet.Cells[2, 5].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[2, 6].Value = "Password123!";
                worksheet.Cells[2, 7].Value = "User,Admin"; // Example roles

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UsersImportTemplate.xlsx");
            }
        }

        // POST: Users/ImportUsersExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة المستخدمين", "    استيراد المستخدمين")]
        [AuditLog("استيراد", "  استيراد المستخدمين")]
        public async Task<ActionResult> ImportUsersExcel(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "الرجاء تحديد ملف Excel للاستيراد.";
                return RedirectToAction("Index");
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["ErrorMessage"] = "الرجاء رفع ملف Excel صالح (بصيغة .xlsx أو .xls).";
                return RedirectToAction("Index");
            }

            try
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "ملف Excel فارغ أو لا يحتوي على أوراق عمل.";
                        return RedirectToAction("Index");
                    }

                    // Define expected headers for robust import
                    // These headers must exactly match the headers in the Excel template
                    var expectedHeaders = new Dictionary<string, int>
                    {
                        { "البريد الإلكتروني", 1 },
                        { "الاسم الكامل", 2 },
                        { "رقم الهوية", 3 },
                        { "رقم هوية المحامي المرتبط", 4 },
                        { "تاريخ إنشاء الحساب", 5 },
                        { "كلمة المرور", 6 },
                        { "الأدوار", 7 }
                    };

                    // Validate headers by checking if all expected headers exist in the first row
                    var headerRow = worksheet.Cells[worksheet.Dimension.Start.Row, 1, 1, worksheet.Dimension.End.Column]
                                            .ToDictionary(cell => cell.Text.Trim(), cell => cell.Start.Column);

                    foreach (var headerKey in expectedHeaders.Keys)
                    {
                        if (!headerRow.ContainsKey(headerKey))
                        {
                            TempData["ErrorMessage"] = $"الملف لا يحتوي على العمود المطلوب: '{headerKey}'. يرجى استخدام القالب الصحيح.";
                            return RedirectToAction("Index");
                        }
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int successCount = 0;
                    int errorCount = 0;
                    var errorMessages = new List<string>();

                    for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip header
                    {
                        try
                        {
                            string email = worksheet.Cells[row, headerRow["البريد الإلكتروني"]].Text?.Trim();
                            string fullName = worksheet.Cells[row, headerRow["الاسم الكامل"]].Text?.Trim();
                            string idNumber = worksheet.Cells[row, headerRow["رقم الهوية"]].Text?.Trim();
                            string linkedLawyerIdNumber = worksheet.Cells[row, headerRow["رقم هوية المحامي المرتبط"]].Text?.Trim();
                            string creationDateString = worksheet.Cells[row, headerRow["تاريخ إنشاء الحساب"]].Text?.Trim();
                            string password = worksheet.Cells[row, headerRow["كلمة المرور"]].Text?.Trim();
                            string rolesString = worksheet.Cells[row, headerRow["الأدوار"]].Text?.Trim();

                            if (string.IsNullOrEmpty(email))
                            {
                                errorMessages.Add($"الصف {row}: البريد الإلكتروني لا يمكن أن يكون فارغًا.");
                                errorCount++;
                                continue;
                            }

                            ApplicationUser user = await UserManager.FindByEmailAsync(email);
                            bool isNewUser = (user == null);

                            if (isNewUser)
                            {
                                user = new ApplicationUser
                                {
                                    UserName = email,
                                    Email = email,
                                    FullName = fullName,
                                    IdNumber = idNumber,
                                    LinkedLawyerIdNumber = linkedLawyerIdNumber,
                                    CreationDate = DateTime.TryParse(creationDateString, out DateTime cDate) ? cDate : DateTime.UtcNow,
                                    IsLinkedToLawyer = !string.IsNullOrEmpty(linkedLawyerIdNumber)
                                };

                                if (string.IsNullOrEmpty(password))
                                {
                                    errorMessages.Add($"الصف {row}: كلمة المرور مطلوبة لمستخدم جديد '{email}'.");
                                    errorCount++;
                                    continue;
                                }

                                var createResult = await UserManager.CreateAsync(user, password);
                                if (!createResult.Succeeded)
                                {
                                    errorMessages.Add($"الصف {row}: فشل إنشاء المستخدم '{email}': {string.Join(", ", createResult.Errors)}");
                                    errorCount++;
                                    continue;
                                }
                            }
                            else // Existing user, update
                            {
                                user.FullName = fullName;
                                user.IdNumber = idNumber;
                                user.LinkedLawyerIdNumber = linkedLawyerIdNumber;
                                user.IsLinkedToLawyer = !string.IsNullOrEmpty(linkedLawyerIdNumber);

                                // Update creation date only if provided and valid
                                if (DateTime.TryParse(creationDateString, out DateTime cDate))
                                {
                                    user.CreationDate = cDate;
                                }

                                var updateResult = await UserManager.UpdateAsync(user);
                                if (!updateResult.Succeeded)
                                {
                                    errorMessages.Add($"الصف {row}: فشل تحديث المستخدم '{email}': {string.Join(", ", updateResult.Errors)}");
                                    errorCount++;
                                    continue;
                                }
                            }

                            // Handle roles
                            if (!string.IsNullOrEmpty(rolesString))
                            {
                                var roles = rolesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(r => r.Trim())
                                                       .ToList();
                                var currentRoles = await UserManager.GetRolesAsync(user.Id);

                                // Remove roles no longer assigned
                                var rolesToRemove = currentRoles.Except(roles).ToList();
                                if (rolesToRemove.Any())
                                {
                                    var removeResult = await UserManager.RemoveFromRolesAsync(user.Id, rolesToRemove.ToArray());
                                    if (!removeResult.Succeeded)
                                    {
                                        errorMessages.Add($"الصف {row}: فشل إزالة الأدوار من المستخدم '{email}': {string.Join(", ", removeResult.Errors)}");
                                        // Don't continue, try to add new roles
                                    }
                                }

                                // Add new roles
                                var rolesToAdd = roles.Except(currentRoles).ToList();
                                if (rolesToAdd.Any())
                                {
                                    var addResult = await UserManager.AddToRolesAsync(user.Id, rolesToAdd.ToArray());
                                    if (!addResult.Succeeded)
                                    {
                                        errorMessages.Add($"الصف {row}: فشل إضافة الأدوار للمستخدم '{email}': {string.Join(", ", addResult.Errors)}");
                                        // Don't continue
                                    }
                                }
                            }
                            else // If rolesString is empty, remove all roles
                            {
                                var currentRoles = await UserManager.GetRolesAsync(user.Id);
                                if (currentRoles.Any())
                                {
                                    var removeAllResult = await UserManager.RemoveFromRolesAsync(user.Id, currentRoles.ToArray());
                                    if (!removeAllResult.Succeeded)
                                    {
                                        errorMessages.Add($"الصف {row}: فشل إزالة جميع الأدوار من المستخدم '{email}': {string.Join(", ", removeAllResult.Errors)}");
                                    }
                                }
                            }

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"الصف {row}: خطأ أثناء معالجة البيانات: {ex.Message}");
                            errorCount++;
                        }
                    }

                    string summaryMessage = $"تم استيراد {successCount} مستخدم بنجاح.";
                    if (errorCount > 0)
                    {
                        summaryMessage += $" حدثت أخطاء في {errorCount} مستخدم:";
                        TempData["ErrorMessage"] = summaryMessage + "<br/>" + string.Join("<br/>", errorMessages);
                    }
                    else
                    {
                        TempData["SuccessMessage"] = summaryMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء معالجة ملف Excel: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


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
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
