using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Models; // تأكد من وجود مساحة الاسم هذه لموديلاتك
using System.Data.Entity; // لـ ToListAsync و FindAsync
using System.Linq; // لـ AnyAsync
using System.Collections.Generic; // لـ List<SelectListItem>
using Microsoft.AspNet.Identity; // لجلب معرف المستخدم الحالي
using System.IO; // لعمليات الملفات (للمرفقات)
using System; // لـ Guid
using Microsoft.AspNet.Identity.Owin; // لـ HttpContext.GetOwinContext().GetUserManager
using System.Web;
using System.Data.Entity.Validation; // إضافة هذا الـ namespace
using System.Diagnostics; // إضافة هذا الـ namespace للتصحيح
using System.Data.Entity.Infrastructure; // لـ DbUpdateConcurrencyException
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize]
    public class MyProfileController : Controller
    {
        private readonly ApplicationDbContext _context; // استبدل بـ DbContext الخاص بك
        private ApplicationUserManager _userManager; // إضافة UserManager

        public MyProfileController()
        {
            _context = new ApplicationDbContext(); // تهيئة DbContext
        }

        // خاصية لجلب UserManager من OWIN Context
        public ApplicationUserManager UserManager
        {
            get
            {
                // تم التأكد من وجود GetOwinContext()
                // تأكد من تثبيت حزمة NuGet Microsoft.AspNet.Identity.Owin
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // دالة مساعدة لجلب رقم هوية المحامي المرتبط بالمستخدم الحالي
        private async Task<string> GetCurrentLawyerIdNumber()
        {
            string userId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            // استخدام UserManager لجلب المستخدم
            var user = await UserManager.FindByIdAsync(userId);
            return user?.LinkedLawyerIdNumber;
        }

        // GET: MyProfile/Index
        [PermissionAuthorizationFilter("عرض ملف المحامي", "صلاحية عرض ملف المحامي")]
        [AuditLog("عرض", "عرض ملف المحامي")]
        public async Task<ActionResult> Index()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            LawyerDetailsViewModel lawyerDetails = new LawyerDetailsViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var lawyer = await _context.Lawyers
                                            .Include(l => l.PersonalDetails)
                                            .Include(l => l.FamilyDetails)
                                            .Include(l => l.HealthStatuses)
                                            .Include(l => l.OfficeDetails)
                                            .Include(l => l.HomeDamages)
                                            .Include(l => l.OfficeDamages)
                                            .Include(l => l.DetentionDetails)
                                            .Include(l => l.ColleagueInfos)
                                            .Include(l => l.GeneralInfos)
                                            .Include(l => l.LawyerAttachments) // تضمين المرفقات
                                            .FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);

                if (lawyer != null)
                {
                    // تعبئة LawyerDetailsViewModel من بيانات المحامي
                    lawyerDetails.LawyerIdNumber = lawyer.IdNumber;
                    lawyerDetails.LawyerFullName = lawyer.FullName;
                    lawyerDetails.ProfessionalStatus = lawyer.ProfessionalStatus;
                    lawyerDetails.MembershipNumber = lawyer.MembershipNumber;
                    lawyerDetails.PracticeStartDate = lawyer.PracticeStartDate;
                    lawyerDetails.TrainingStartDate = lawyer.TrainingStartDate;
                    lawyerDetails.TrainerLawyerName = lawyer.TrainerLawyerName;
                    lawyerDetails.Gender = lawyer.Gender;
                    lawyerDetails.IsActive = lawyer.IsActive;
                    lawyerDetails.IsTrainee = lawyer.IsTrainee;

                    // تعبئة البيانات الشخصية
                    var personalDetail = lawyer.PersonalDetails.FirstOrDefault();
                    if (personalDetail != null)
                    {
                        lawyerDetails.EmailAddress = personalDetail.EmailAddress;
                        lawyerDetails.MobileNumber = personalDetail.MobileNumber;
                        lawyerDetails.AltMobileNumber1 = personalDetail.AltMobileNumber1;
                        lawyerDetails.AltMobileNumber2 = personalDetail.AltMobileNumber2;
                        lawyerDetails.WhatsAppNumber = personalDetail.WhatsAppNumber;
                        lawyerDetails.LandlineNumber = personalDetail.LandlineNumber;
                        lawyerDetails.OriginalGovernorate = personalDetail.OriginalGovernorate;
                        lawyerDetails.CurrentGovernorate = personalDetail.CurrentGovernorate;
                        lawyerDetails.AccommodationType = personalDetail.AccommodationType;
                        lawyerDetails.FullAddress = personalDetail.FullAddress;
                    }

                    // تعبئة تفاصيل العائلة
                    var familyDetail = lawyer.FamilyDetails.FirstOrDefault();
                    if (familyDetail != null)
                    {
                        lawyerDetails.MaritalStatus = familyDetail.MaritalStatus;
                        lawyerDetails.NumberOfSpouses = familyDetail.NumberOfSpouses;
                        lawyerDetails.HasChildren = familyDetail.HasChildren;
                        lawyerDetails.NumberOfChildren = familyDetail.NumberOfChildren;
                        lawyerDetails.Spouses = familyDetail.Spouses.Select(s => new SpouseViewModel
                        {
                            SpouseName = s.SpouseName,
                            SpouseIdNumber = s.SpouseIdNumber,
                            SpouseMobileNumber = s.SpouseMobileNumber
                        }).ToList();
                        lawyerDetails.Children = familyDetail.Children.Select(c => new ChildViewModel
                        {
                            ChildName = c.ChildName,
                            DateOfBirth = c.DateOfBirth,
                            IdNumber = c.IdNumber,
                            Gender = c.Gender
                        }).ToList();
                    }

                    // تعبئة الحالة الصحية
                    var healthStatus = lawyer.HealthStatuses.FirstOrDefault();
                    if (healthStatus != null)
                    {
                        lawyerDetails.LawyerCondition = healthStatus.LawyerCondition;
                        lawyerDetails.InjuryDetails = healthStatus.InjuryDetails;
                        lawyerDetails.TreatmentNeeded = healthStatus.TreatmentNeeded;
                        lawyerDetails.LawyerDiagnosis = healthStatus.LawyerDiagnosis;
                        lawyerDetails.HasFamilyMembersInjured = healthStatus.HasFamilyMembersInjured;
                        // تم التعديل هنا ليتوافق مع HealthStatus.cs الخاص بك
                        lawyerDetails.NumberOfFamilyMembersInjured = healthStatus.FamilyMembersInjured ?? 0;
                        lawyerDetails.FamilyMemberInjuries = healthStatus.FamilyMemberInjuries.Select(fmi => new FamilyMemberInjuryViewModel
                        {
                            InjuredFamilyMemberName = fmi.InjuredFamilyMemberName,
                            RelationshipToLawyer = fmi.RelationshipToLawyer,
                            InjuryDetails = fmi.InjuryDetails
                        }).ToList();
                    }

                    // تعبئة تفاصيل المكتب
                    var officeDetail = lawyer.OfficeDetails.FirstOrDefault();
                    if (officeDetail != null)
                    {
                        lawyerDetails.OfficeName = officeDetail.OfficeName;
                        lawyerDetails.OfficeAddress = officeDetail.OfficeAddress;
                        lawyerDetails.PropertyType = officeDetail.PropertyType;
                        lawyerDetails.PropertyStatus = officeDetail.PropertyStatus;
                        lawyerDetails.HasPartners = officeDetail.HasPartners;
                        lawyerDetails.NumberOfPartners = officeDetail.NumberOfPartners;
                        lawyerDetails.Partners = officeDetail.Partners.Select(p => new PartnerViewModel
                        {
                            PartnerName = p.PartnerName,
                            // تم التعديل هنا ليتوافق مع Partner.cs الخاص بك
                            MembershipNumber = p.PartnerMembershipNumber
                        }).ToList();
                    }

                    // تعبئة أضرار المنزل
                    var homeDamage = lawyer.HomeDamages.FirstOrDefault();
                    if (homeDamage != null)
                    {
                        lawyerDetails.HasHomeDamage = homeDamage.HasHomeDamage;
                        lawyerDetails.HomeDamageType = homeDamage.DamageType;
                        lawyerDetails.HomeDamageDetails = homeDamage.DamageDetails;
                    }

                    // تعبئة أضرار المكتب
                    var officeDamage = lawyer.OfficeDamages.FirstOrDefault();
                    if (officeDamage != null)
                    {
                        // يجب أن تكون هذه الخاصية موجودة في LawyerDetailsViewModel
                        lawyerDetails.HasOfficeDamage = true; // نفترض أنه يوجد ضرر إذا كان الكائن موجوداً
                        lawyerDetails.OfficeDamageType = officeDamage.DamageType;
                        lawyerDetails.OfficeDamageDetails = officeDamage.DamageDetails;
                    }
                    else
                    {
                        lawyerDetails.HasOfficeDamage = false;
                    }


                    // تعبئة تفاصيل الاعتقال
                    var detentionDetail = lawyer.DetentionDetails.FirstOrDefault();
                    if (detentionDetail != null)
                    {
                        lawyerDetails.WasDetained = detentionDetail.WasDetained;
                        lawyerDetails.DetentionDuration = detentionDetail.DetentionDuration;
                        lawyerDetails.DetentionStartDate = detentionDetail.DetentionStartDate;
                        lawyerDetails.IsStillDetained = detentionDetail.IsStillDetained;
                        lawyerDetails.ReleaseDate = detentionDetail.ReleaseDate;
                        lawyerDetails.DetentionType = detentionDetail.DetentionType;
                        lawyerDetails.DetentionLocation = detentionDetail.DetentionLocation;
                    }

                    // تعبئة معلومات الزملاء
                    var colleagueInfo = lawyer.ColleagueInfos.FirstOrDefault();
                    if (colleagueInfo != null)
                    {
                        lawyerDetails.KnowsOfMartyrColleagues = colleagueInfo.KnowsOfMartyrColleagues;
                        lawyerDetails.HasMartyrs = colleagueInfo.HasMartyrs;
                        lawyerDetails.MartyrColleagues = colleagueInfo.MartyrColleagues.Select(mc => new MartyrColleagueViewModel
                        {
                            MartyrName = mc.MartyrName,
                            ContactNumber = mc.ContactNumber
                        }).ToList();

                        lawyerDetails.KnowsOfDetainedColleagues = colleagueInfo.KnowsOfDetainedColleagues;
                        lawyerDetails.HasDetained = colleagueInfo.HasDetained;
                        lawyerDetails.DetainedColleagues = colleagueInfo.DetainedColleagues.Select(dc => new DetainedColleagueViewModel
                        {
                            DetainedName = dc.DetainedName,
                            ContactNumber = dc.ContactNumber
                        }).ToList();

                        lawyerDetails.KnowsOfInjuredColleagues = colleagueInfo.KnowsOfInjuredColleagues;
                        lawyerDetails.HasInjured = colleagueInfo.HasInjured;
                        lawyerDetails.InjuredColleagues = colleagueInfo.InjuredColleagues.Select(ic => new InjuredColleagueViewModel
                        {
                            InjuredName = ic.InjuredName,
                            ContactNumber = ic.ContactNumber // تم التأكد من وجودها في InjuredColleague.cs و ViewModel
                        }).ToList();
                    }

                    // تعبئة المعلومات العامة
                    var generalInfo = lawyer.GeneralInfos.FirstOrDefault();
                    if (generalInfo != null)
                    {
                        lawyerDetails.PracticesShariaLaw = generalInfo.PracticesShariaLaw;
                        lawyerDetails.ShariaLawPracticeStartDate = generalInfo.ShariaLawPracticeStartDate;
                        lawyerDetails.ReceivedAidFromSyndicate = generalInfo.ReceivedAidFromSyndicate;
                        lawyerDetails.ReceivedAids = generalInfo.ReceivedAids.Select(ra => new ReceivedAidViewModel
                        {
                            AidType = ra.AidType,
                            ReceivedDate = ra.ReceivedDate
                        }).ToList();
                    }

                    // تعبئة المرفقات
                    lawyerDetails.LawyerAttachments = lawyer.LawyerAttachments.Select(la => new LawyerAttachmentViewModel
                    {
                        Id = la.Id,
                        LawyerIdNumber = la.LawyerIdNumber,
                        AttachmentType = la.AttachmentType,
                        ExistingFileName = la.FileName,
                        Notes = la.Notes,
                        UploadDate = la.UploadDate,
                        FilePath = Url.Content("~/App_Data/Uploads/LawyerAttachments/" + la.FileName) // استخدام المسار الصحيح
                    }).ToList();

                }
            }
            else
            {
                TempData["InfoMessage"] = "لم يتم ربط حسابك بملف محامٍ بعد.";
            }

            // يتم تمرير رسائل TempData إلى ViewBag ليتم عرضها في View.
            // TempData يتم مسحه تلقائيًا بعد القراءة، لذا يجب قراءته هنا.
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.InfoMessage = TempData["InfoMessage"];

            return View(lawyerDetails);
        }

        // GET: MyProfile/EditPersonalDetails
        // GET: MyProfile/EditPersonalDetails
        [PermissionAuthorizationFilter("تعديل البيانات الشخصية", "صلاحية تعديل البيانات الشخصية")]
        [AuditLog("تعديل", "تعديل البيانات الشخصية")]
        public async Task<ActionResult> EditPersonalDetails()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            PersonalDetailViewModel model = new PersonalDetailViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var personalDetail = await _context.PersonalDetails.FirstOrDefaultAsync(pd => pd.LawyerIdNumber == lawyerIdNumber);
                if (personalDetail != null)
                {
                    // تعبئة النموذج من الكائن الموجود
                    model.Id = personalDetail.Id;
                    model.Gender = personalDetail.Gender;
                    model.EmailAddress = personalDetail.EmailAddress;
                    model.MobileNumber = personalDetail.MobileNumber;
                    model.AltMobileNumber1 = personalDetail.AltMobileNumber1;
                    model.AltMobileNumber2 = personalDetail.AltMobileNumber2;
                    model.WhatsAppNumber = personalDetail.WhatsAppNumber;
                    model.LandlineNumber = personalDetail.LandlineNumber;
                    model.OriginalGovernorate = personalDetail.OriginalGovernorate;
                    model.CurrentGovernorate = personalDetail.CurrentGovernorate;
                    model.AccommodationType = personalDetail.AccommodationType;
                    model.FullAddress = personalDetail.FullAddress;
                    // 🔥 تعبئة الخصائص المصرفية الجديدة من كائن PersonalDetail
                    model.BankName = personalDetail.BankName;
                    model.BankBranch = personalDetail.BankBranch;
                    model.BankAccountNumber = personalDetail.BankAccountNumber;
                    model.IBAN = personalDetail.IBAN;
                    model.WalletType = personalDetail.WalletType;
                    model.WalletAccountNumber = personalDetail.WalletAccountNumber;
                    model.DateOfBirth = personalDetail.DateOfBirth;
                }
            }

            // تهيئة القوائم المنسدلة
            PopulatePersonalDetailsDropdowns(model);

            // 🔥 إضافة قائمة البنوك إلى النموذج وتحديد العنصر المحدد
            model.BankList = new List<SelectListItem>
    {
        new SelectListItem { Value = "بنك فلسطين", Text = "بنك فلسطين" },
        new SelectListItem { Value = "البنك الإسلامي العربي", Text = "البنك الإسلامي العربي" },
        new SelectListItem { Value = "البنك الإسلامي الفلسطيني", Text = "البنك الإسلامي الفلسطيني" },
        new SelectListItem { Value = "البنك الوطني", Text = "البنك الوطني" },
        new SelectListItem { Value = "بنك القدس", Text = "بنك القدس" },
        new SelectListItem { Value = "البنك العربي", Text = "البنك العربي" }
    };

            // تحديد العنصر المحدد في القائمة بناءً على قيمة النموذج
            if (!string.IsNullOrEmpty(model.BankName))
            {
                var selectedBank = model.BankList.FirstOrDefault(b => b.Value == model.BankName);
                if (selectedBank != null)
                {
                    selectedBank.Selected = true;
                }
            }

            return View(model);
        }

        // POST: MyProfile/EditPersonalDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPersonalDetails(PersonalDetailViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulatePersonalDetailsDropdowns(model);
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var personalDetail = await _context.PersonalDetails.FirstOrDefaultAsync(pd => pd.LawyerIdNumber == lawyerIdNumber);

                if (personalDetail == null)
                {
                    // إنشاء جديد
                    personalDetail = new PersonalDetail();
                    personalDetail.LawyerIdNumber = lawyerIdNumber;
                    _context.PersonalDetails.Add(personalDetail);
                }

                // تحديث الخصائص
                personalDetail.Gender = model.Gender;
                personalDetail.EmailAddress = model.EmailAddress;
                personalDetail.MobileNumber = model.MobileNumber;
                personalDetail.AltMobileNumber1 = model.AltMobileNumber1;
                personalDetail.AltMobileNumber2 = model.AltMobileNumber2;
                personalDetail.WhatsAppNumber = model.WhatsAppNumber;
                personalDetail.LandlineNumber = model.LandlineNumber;
                personalDetail.OriginalGovernorate = model.OriginalGovernorate;
                personalDetail.CurrentGovernorate = model.CurrentGovernorate;
                personalDetail.AccommodationType = model.AccommodationType;
                personalDetail.FullAddress = model.FullAddress;

                // 🔥 إضافة الحقول الجديدة لتحديث الكائن
                personalDetail.BankName = model.BankName;
                personalDetail.BankBranch = model.BankBranch;
                personalDetail.BankAccountNumber = model.BankAccountNumber;
                personalDetail.IBAN = model.IBAN;
                personalDetail.WalletType = model.WalletType;
                personalDetail.WalletAccountNumber = model.WalletAccountNumber;
                personalDetail.DateOfBirth = model.DateOfBirth;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث البيانات الشخصية بنجاح.";
                return RedirectToAction("Index");
            }

            // إذا لم يكن النموذج صالحاً، أعد عرض النموذج مع الأخطاء
           
          
            TempData["InfoMessage"] = "الرجاء تصحيح الأخطاء في النموذج. تأكد من أن جميع الحقول المطلوبة مملوءة بشكل صحيح.";
            PopulatePersonalDetailsDropdowns(model);
            return View(model);
        }

        private void PopulatePersonalDetailsDropdowns(PersonalDetailViewModel model)
        {
            model.Genders = new List<SelectListItem>
            {
                new SelectListItem { Value = "ذكر", Text = "ذكر" },
                new SelectListItem { Value = "انثى", Text = "أنثى" }
            };
            model.Governorates = new List<SelectListItem>
            {
                new SelectListItem { Value = "شمال غزة", Text = "شمال غزة" },
                new SelectListItem { Value = "مدينة غزة", Text = "مدينة غزة" },
                new SelectListItem { Value = "الوسطى", Text = "الوسطى" },
                new SelectListItem { Value = "خانيونس", Text = "خانيونس" },
                new SelectListItem { Value = "رفح", Text = "رفح" },
                new SelectListItem { Value = "الضفة الغربية", Text = "الضفة الغربية" },
                new SelectListItem { Value = "خارج البلاد", Text = "خارج البلاد" }
            };
 
        }


        // GET: MyProfile/EditFamilyDetails
        [PermissionAuthorizationFilter("تعديل تفاصيل العائلة", "صلاحية تعديل تفاصيل العائلة")]
        [AuditLog("تعديل", "تعديل تفاصيل العائلة")]
        public async Task<ActionResult> EditFamilyDetails()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            FamilyDetailViewModel model = new FamilyDetailViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var familyDetail = await _context.FamilyDetails
                                                    .Include(fd => fd.Children)
                                                    .Include(fd => fd.Spouses)
                                                    .FirstOrDefaultAsync(fd => fd.LawyerIdNumber == lawyerIdNumber);
                if (familyDetail != null)
                {
                    model.Id = familyDetail.Id;
                    model.MaritalStatus = familyDetail.MaritalStatus;
                    model.NumberOfSpouses = familyDetail.NumberOfSpouses;
                    model.HasChildren = familyDetail.HasChildren;
                    model.NumberOfChildren = familyDetail.NumberOfChildren;

                    // تعبئة الأطفال والزوجات
                    model.Children = familyDetail.Children.Select(c => new ChildViewModel
                    {
                        Id = c.Id,
                        FamilyDetailId = c.FamilyDetailId,
                        ChildName = c.ChildName,
                        DateOfBirth = c.DateOfBirth,
                        IdNumber = c.IdNumber,
                        Gender = c.Gender
                    }).ToList();

                    model.Spouses = familyDetail.Spouses.Select(s => new SpouseViewModel
                    {
                        Id = s.Id,
                        FamilyDetailId = s.FamilyDetailId,
                        SpouseName = s.SpouseName,
                        SpouseIdNumber = s.SpouseIdNumber,
                        SpouseMobileNumber = s.SpouseMobileNumber
                    }).ToList();
                }
            }

            PopulateFamilyDetailsDropdowns(model);
            // تهيئة قوائم الأطفال والزوجات إذا كانت موجودة
            // (تتم تهيئتها في الـ ViewModel constructor أو عند الجلب)
            return View(model);
        }

        // POST: MyProfile/EditFamilyDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFamilyDetails(FamilyDetailViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateFamilyDetailsDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var familyDetail = await _context.FamilyDetails
                                                    .Include(fd => fd.Children)
                                                    .Include(fd => fd.Spouses)
                                                    .FirstOrDefaultAsync(fd => fd.LawyerIdNumber == lawyerIdNumber);

                if (familyDetail == null)
                {
                    familyDetail = new FamilyDetail();
                    familyDetail.LawyerIdNumber = lawyerIdNumber;
                    _context.FamilyDetails.Add(familyDetail);
                }

                familyDetail.MaritalStatus = model.MaritalStatus;
                familyDetail.NumberOfSpouses = model.NumberOfSpouses;
                familyDetail.HasChildren = model.HasChildren;
                familyDetail.NumberOfChildren = model.NumberOfChildren;

                // تحديث الأطفال
                UpdateCollection(familyDetail.Children, model.Children, (entity, vm) =>
                {
                    entity.ChildName = vm.ChildName;
                    entity.DateOfBirth = vm.DateOfBirth;
                    entity.IdNumber = vm.IdNumber;
                    entity.Gender = vm.Gender;
                });

                // تحديث الزوجات
                UpdateCollection(familyDetail.Spouses, model.Spouses, (entity, vm) =>
                {
                    entity.SpouseName = vm.SpouseName;
                    entity.SpouseIdNumber = vm.SpouseIdNumber;
                    entity.SpouseMobileNumber = vm.SpouseMobileNumber;
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث تفاصيل العائلة بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateFamilyDetailsDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateFamilyDetailsDropdowns(FamilyDetailViewModel model)
        {
            model.MaritalStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Single", Text = "أعزب/عزباء" },
                new SelectListItem { Value = "Married", Text = "متزوج/متزوجة" },
                new SelectListItem { Value = "Divorced", Text = "مطلق/مطلقة" },
                new SelectListItem { Value = "Widowed", Text = "أرمل/أرملة" }
            };
            // تأكد من تهيئة Genders في ChildViewModel إذا تم استخدامها
            if (model.Children != null)
            {
                foreach (var child in model.Children)
                {
                    child.Genders = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "ذكر", Text = "ذكر" },
                        new SelectListItem { Value = "أنثى", Text = "أنثى" }
                    };
                }
            }
        }

        // POST: MyProfile/AddSpouseField (لإضافة حقل زوجة ديناميكي)
        [HttpPost]
        public PartialViewResult AddSpouseField(int index)
        {
            var model = new SpouseViewModel();
            return PartialView("_SpouseFields", model);
        }

        // POST: MyProfile/AddChildField (لإضافة حقل طفل ديناميكي)
        [HttpPost]
        public PartialViewResult AddChildField(int index)
        {
            var model = new ChildViewModel();
            model.Genders = new List<SelectListItem>
            {
                new SelectListItem { Value = "ذكر", Text = "ذكر" },
                new SelectListItem { Value = "أنثى", Text = "أنثى" }
            };
            return PartialView("_ChildFields", model);
        }


        // GET: MyProfile/EditHealthStatus
        [PermissionAuthorizationFilter("تعديل الحالة الصحية", "صلاحية تعديل الحالة الصحية")]
        [AuditLog("تعديل", "تعديل الحالة الصحية")]
        public async Task<ActionResult> EditHealthStatus()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            HealthStatusViewModel model = new HealthStatusViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var healthStatus = await _context.HealthStatuses
                                                    .Include(hs => hs.FamilyMemberInjuries)
                                                    .FirstOrDefaultAsync(hs => hs.LawyerIdNumber == lawyerIdNumber);
                if (healthStatus != null)
                {
                    model.Id = healthStatus.Id;
                    model.LawyerCondition = healthStatus.LawyerCondition;
                    model.InjuryDetails = healthStatus.InjuryDetails;
                    model.TreatmentNeeded = healthStatus.TreatmentNeeded;
                    model.LawyerDiagnosis = healthStatus.LawyerDiagnosis;
                    model.HasFamilyMembersInjured = healthStatus.HasFamilyMembersInjured;
                    // تم التعديل هنا ليتوافق مع HealthStatus.cs الخاص بك
                    model.NumberOfFamilyMembersInjured = healthStatus.FamilyMembersInjured ?? 0;

                    model.FamilyMemberInjuries = healthStatus.FamilyMemberInjuries.Select(fmi => new FamilyMemberInjuryViewModel
                    {
                        Id = fmi.Id,
                        HealthStatusId = fmi.HealthStatusId,
                        InjuredFamilyMemberName = fmi.InjuredFamilyMemberName,
                        RelationshipToLawyer = fmi.RelationshipToLawyer,
                        InjuryDetails = fmi.InjuryDetails
                    }).ToList();
                }
            }

            PopulateHealthStatusDropdowns(model);
            return View(model);
        }

        // POST: MyProfile/EditHealthStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditHealthStatus(HealthStatusViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateHealthStatusDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var healthStatus = await _context.HealthStatuses
                                                    .Include(hs => hs.FamilyMemberInjuries)
                                                    .FirstOrDefaultAsync(hs => hs.LawyerIdNumber == lawyerIdNumber);

                if (healthStatus == null)
                {
                    healthStatus = new HealthStatus();
                    healthStatus.LawyerIdNumber = lawyerIdNumber;
                    _context.HealthStatuses.Add(healthStatus);
                }

                healthStatus.LawyerCondition = model.LawyerCondition;
                healthStatus.InjuryDetails = model.InjuryDetails;
                healthStatus.TreatmentNeeded = model.TreatmentNeeded;
                healthStatus.LawyerDiagnosis = model.LawyerDiagnosis;
                healthStatus.HasFamilyMembersInjured = model.HasFamilyMembersInjured;
                // تم التعديل هنا ليتوافق مع HealthStatus.cs الخاص بك
                healthStatus.FamilyMembersInjured = model.NumberOfFamilyMembersInjured;

                // تحديث إصابات أفراد العائلة
                UpdateCollection(healthStatus.FamilyMemberInjuries, model.FamilyMemberInjuries, (entity, vm) =>
                {
                    entity.InjuredFamilyMemberName = vm.InjuredFamilyMemberName;
                    entity.RelationshipToLawyer = vm.RelationshipToLawyer;
                    entity.InjuryDetails = vm.InjuryDetails;
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث الحالة الصحية بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateHealthStatusDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateHealthStatusDropdowns(HealthStatusViewModel model)
        {
            model.LawyerConditions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Healthy", Text = "سليم" },
                new SelectListItem { Value = "Injured", Text = "مصاب" },
                new SelectListItem { Value = "NeedsTreatment", Text = "يحتاج علاج" }
            };
        }

        // POST: MyProfile/AddFamilyMemberInjuryField (لإضافة حقل إصابة فرد عائلة ديناميكي)
        [HttpPost]
        public PartialViewResult AddFamilyMemberInjuryField(int index)
        {
            var model = new FamilyMemberInjuryViewModel();
            return PartialView("_FamilyMemberInjuryFields", model);
        }


        // GET: MyProfile/EditOfficeDetails
        [PermissionAuthorizationFilter("تعديل تفاصيل المكتب", "صلاحية تعديل تفاصيل المكتب")]
        [AuditLog("تعديل", "تعديل تفاصيل المكتب")]
        public async Task<ActionResult> EditOfficeDetails()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            OfficeDetailViewModel model = new OfficeDetailViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var officeDetail = await _context.OfficeDetails
                                                    .Include(od => od.Partners)
                                                    .FirstOrDefaultAsync(od => od.LawyerIdNumber == lawyerIdNumber);
                if (officeDetail != null)
                {
                    model.Id = officeDetail.Id;
                    model.OfficeName = officeDetail.OfficeName;
                    model.OfficeAddress = officeDetail.OfficeAddress;
                    model.PropertyType = officeDetail.PropertyType;
                    model.PropertyStatus = officeDetail.PropertyStatus;
                    model.HasPartners = officeDetail.HasPartners;
                    model.NumberOfPartners = officeDetail.NumberOfPartners;

                    model.Partners = officeDetail.Partners.Select(p => new PartnerViewModel
                    {
                        Id = p.Id,
                        OfficeDetailId = p.OfficeDetailId,
                        PartnerName = p.PartnerName,
                        // تم التعديل هنا ليتوافق مع Partner.cs الخاص بك
                        MembershipNumber = p.PartnerMembershipNumber
                    }).ToList();
                }
            }

            PopulateOfficeDetailsDropdowns(model);
            return View(model);
        }

        // POST: MyProfile/EditOfficeDetails
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> EditOfficeDetails(OfficeDetailViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateOfficeDetailsDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var officeDetail = await _context.OfficeDetails
                                                    .Include(od => od.Partners)
                                                    .FirstOrDefaultAsync(od => od.LawyerIdNumber == lawyerIdNumber);

                if (officeDetail == null)
                {
                    officeDetail = new OfficeDetail();
                    officeDetail.LawyerIdNumber = lawyerIdNumber;
                    _context.OfficeDetails.Add(officeDetail);
                }

                officeDetail.OfficeName = model.OfficeName;
                officeDetail.OfficeAddress = model.OfficeAddress;
                officeDetail.PropertyType = model.PropertyType;
                officeDetail.PropertyStatus = model.PropertyStatus;
                officeDetail.HasPartners = model.HasPartners;
                officeDetail.NumberOfPartners = model.NumberOfPartners;

                // تحديث الشركاء
                UpdateCollection(officeDetail.Partners, model.Partners, (entity, vm) =>
                {
                    entity.PartnerName = vm.PartnerName;
                    // تم التعديل هنا ليتوافق مع Partner.cs الخاص بك
                    entity.PartnerMembershipNumber = vm.MembershipNumber;
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث تفاصيل المكتب بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateOfficeDetailsDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateOfficeDetailsDropdowns(OfficeDetailViewModel model)
        {
            model.PropertyTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "ملك", Text = "ملك" },
                new SelectListItem { Value = "إيجار", Text = "إيجار" },
                 new SelectListItem { Value = "استضافة", Text = "استضافة" },
                 new SelectListItem { Value = "غرفة في شقة", Text = "غرفة في شقة" },
                 new SelectListItem { Value = "في البيت", Text = "في البيت" },
                 new SelectListItem { Value = "حاصل", Text = "حاصل" },
                  new SelectListItem { Value = "أخرى", Text = "أخرى" }
            };
            model.PropertyStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "متاح", Text = "متاح للعمل" },
                new SelectListItem { Value = "غير متاح", Text = "غير متاح" },
                new SelectListItem { Value = "أخرى", Text = "أخرى" }
            };
        }

        // POST: MyProfile/AddPartnerField (لإضافة حقل شريك ديناميكي)
        [HttpPost]
        public PartialViewResult AddPartnerField(int index)
        {
            var model = new PartnerViewModel();
            return PartialView("_PartnerFields", model);
        }


        // GET: MyProfile/EditHomeDamage
        [PermissionAuthorizationFilter("تعديل أضرار المنزل", "صلاحية تعديل أضرار المنزل")]
        [AuditLog("تعديل", "تعديل أضرار المنزل")]
        public async Task<ActionResult> EditHomeDamage()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            HomeDamageViewModel model = new HomeDamageViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var homeDamage = await _context.HomeDamages.FirstOrDefaultAsync(hd => hd.LawyerIdNumber == lawyerIdNumber);
                if (homeDamage != null)
                {
                    model.Id = homeDamage.Id;
                    model.HasHomeDamage = homeDamage.HasHomeDamage;
                    model.DamageType = homeDamage.DamageType;
                    model.DamageDetails = homeDamage.DamageDetails;
                }
            }

            PopulateDamageTypesDropdowns(model);
            return View(model);
        }

        // POST: MyProfile/EditHomeDamage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditHomeDamage(HomeDamageViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateDamageTypesDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var homeDamage = await _context.HomeDamages.FirstOrDefaultAsync(hd => hd.LawyerIdNumber == lawyerIdNumber);

                if (homeDamage == null)
                {
                    homeDamage = new HomeDamage();
                    homeDamage.LawyerIdNumber = lawyerIdNumber;
                    _context.HomeDamages.Add(homeDamage);
                }

                homeDamage.HasHomeDamage = model.HasHomeDamage;
                homeDamage.DamageType = model.DamageType;
                homeDamage.DamageDetails = model.DamageDetails;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث أضرار المنزل بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateDamageTypesDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateDamageTypesDropdowns(HomeDamageViewModel model)
        {
            model.DamageTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "جزئي", Text = "جزئي" },
                new SelectListItem { Value = "كلي", Text = "كلي" },
                new SelectListItem { Value = "ضرر طفيف", Text = "ضرر طفيف" },
                new SelectListItem { Value = "ضرر كبير", Text = "ضرر كبير" },
                new SelectListItem { Value = "تدمير كامل", Text = "تدمير كامل" },
                new SelectListItem { Value = "أخرى", Text = "أخرى" }
            };
        }


        // GET: MyProfile/EditOfficeDamage
        [PermissionAuthorizationFilter("تعديل أضرار المكتب", "صلاحية تعديل أضرار المكتب")]
        [AuditLog("تعديل", "تعديل أضرار المكتب")]
        public async Task<ActionResult> EditOfficeDamage()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            OfficeDamageViewModel model = new OfficeDamageViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var officeDamage = await _context.OfficeDamages.FirstOrDefaultAsync(od => od.LawyerIdNumber == lawyerIdNumber);
                if (officeDamage != null)
                {
                    model.Id = officeDamage.Id;
                    model.DamageType = officeDamage.DamageType;
                    model.DamageDetails = officeDamage.DamageDetails;
                }
            }

            PopulateOfficeDamageDropdowns(model);
            return View(model);
        }

        // POST: MyProfile/EditOfficeDamage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOfficeDamage(OfficeDamageViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateOfficeDamageDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var officeDamage = await _context.OfficeDamages.FirstOrDefaultAsync(od => od.LawyerIdNumber == lawyerIdNumber);

                if (officeDamage == null)
                {
                    officeDamage = new OfficeDamage();
                    officeDamage.LawyerIdNumber = lawyerIdNumber;
                    _context.OfficeDamages.Add(officeDamage);
                }

                officeDamage.DamageType = model.DamageType;
                officeDamage.DamageDetails = model.DamageDetails;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث أضرار المكتب بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateOfficeDamageDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateOfficeDamageDropdowns(OfficeDamageViewModel model)
        {
            model.DamageTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "جزئي", Text = "جزئي" },
                new SelectListItem { Value = "كلي", Text = "كلي" },
                new SelectListItem { Value = "ضرر طفيف", Text = "ضرر طفيف" },
                new SelectListItem { Value = "ضرر كبير", Text = "ضرر كبير" },
                new SelectListItem { Value = "تدمير كامل", Text = "تدمير كامل" },
                new SelectListItem { Value = "أخرى", Text = "أخرى" }
            };
        }


        // GET: MyProfile/EditDetentionDetails
        [PermissionAuthorizationFilter("تعديل تفاصيل الاعتقال", "صلاحية تعديل تفاصيل الاعتقال")]
        [AuditLog("تعديل", "تعديل تفاصيل الاعتقال")]
        public async Task<ActionResult> EditDetentionDetails()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            DetentionDetailViewModel model = new DetentionDetailViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var detentionDetail = await _context.DetentionDetails.FirstOrDefaultAsync(dd => dd.LawyerIdNumber == lawyerIdNumber);
                if (detentionDetail != null)
                {
                    model.Id = detentionDetail.Id;
                    model.WasDetained = detentionDetail.WasDetained;
                    model.DetentionDuration = detentionDetail.DetentionDuration;
                    model.DetentionStartDate = detentionDetail.DetentionStartDate;
                    model.IsStillDetained = detentionDetail.IsStillDetained;
                    model.ReleaseDate = detentionDetail.ReleaseDate;
                    model.DetentionType = detentionDetail.DetentionType;
                    model.DetentionLocation = detentionDetail.DetentionLocation;
                }
            }

            PopulateDetentionDetailsDropdowns(model);
            return View(model);
        }

        // POST: MyProfile/EditDetentionDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDetentionDetails(DetentionDetailViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                PopulateDetentionDetailsDropdowns(model);
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var detentionDetail = await _context.DetentionDetails.FirstOrDefaultAsync(dd => dd.LawyerIdNumber == lawyerIdNumber);

                if (detentionDetail == null)
                {
                    detentionDetail = new DetentionDetail();
                    detentionDetail.LawyerIdNumber = lawyerIdNumber;
                    _context.DetentionDetails.Add(detentionDetail);
                }

                detentionDetail.WasDetained = model.WasDetained;
                detentionDetail.DetentionDuration = model.DetentionDuration;
                detentionDetail.DetentionStartDate = model.DetentionStartDate;
                detentionDetail.IsStillDetained = model.IsStillDetained;
                detentionDetail.ReleaseDate = model.ReleaseDate;
                detentionDetail.DetentionType = model.DetentionType;
                detentionDetail.DetentionLocation = model.DetentionLocation;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث تفاصيل الاعتقال بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            PopulateDetentionDetailsDropdowns(model);
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        private void PopulateDetentionDetailsDropdowns(DetentionDetailViewModel model)
        {
            model.DetentionDurations = new List<SelectListItem>
            {
                new SelectListItem { Value = "Days", Text = "أيام" },
                new SelectListItem { Value = "Weeks", Text = "أسابيع" },
                new SelectListItem { Value = "Months", Text = "أشهر" },
                new SelectListItem { Value = "Years", Text = "سنوات" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };
            model.DetentionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Administrative", Text = "إداري" },
                new SelectListItem { Value = "Military", Text = "عسكري" },
                new SelectListItem { Value = "Criminal", Text = "جنائي" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };
            model.DetentionLocations = new List<SelectListItem>
            {
                new SelectListItem { Value = "OferPrison", Text = "سجن عوفر" },
                new SelectListItem { Value = "NaqabPrison", Text = "سجن النقب" },
                new SelectListItem { Value = "Other", Text = "آخر" }
            };
        }


        // GET: MyProfile/EditColleagueInfo
        // GET: MyProfile/EditColleagueInfo
        [PermissionAuthorizationFilter("تعديل معلومات الزملاء", "صلاحية تعديل معلومات الزملاء")]
        [AuditLog("تعديل", "تعديل معلومات الزملاء")]
        public async Task<ActionResult> EditColleagueInfo()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            ColleagueInfoViewModel model = new ColleagueInfoViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var colleagueInfo = await _context.ColleagueInfos
                                                    .Include(ci => ci.MartyrColleagues)
                                                    .Include(ci => ci.DetainedColleagues)
                                                    .Include(ci => ci.InjuredColleagues)
                                                    .FirstOrDefaultAsync(ci => ci.LawyerIdNumber == lawyerIdNumber);
                if (colleagueInfo != null)
                {
                    model.Id = colleagueInfo.Id;
                    model.KnowsOfMartyrColleagues = colleagueInfo.KnowsOfMartyrColleagues;
                    model.HasMartyrs = colleagueInfo.HasMartyrs;
                    model.KnowsOfDetainedColleagues = colleagueInfo.KnowsOfDetainedColleagues;
                    model.HasDetained = colleagueInfo.HasDetained;
                    model.KnowsOfInjuredColleagues = colleagueInfo.KnowsOfInjuredColleagues;
                    model.HasInjured = colleagueInfo.HasInjured;

                    model.MartyrColleagues = colleagueInfo.MartyrColleagues.Select(mc => new MartyrColleagueViewModel
                    {
                        Id = mc.Id,
                        ColleagueInfoId = mc.ColleagueInfoId,
                        MartyrName = mc.MartyrName,
                        ContactNumber = mc.ContactNumber
                    }).ToList();

                    model.DetainedColleagues = colleagueInfo.DetainedColleagues.Select(dc => new DetainedColleagueViewModel
                    {
                        Id = dc.Id,
                        ColleagueInfoId = dc.ColleagueInfoId,
                        DetainedName = dc.DetainedName,
                        ContactNumber = dc.ContactNumber
                    }).ToList();

                    model.InjuredColleagues = colleagueInfo.InjuredColleagues.Select(ic => new InjuredColleagueViewModel
                    {
                        Id = ic.Id,
                        ColleagueInfoId = ic.ColleagueInfoId,
                        InjuredName = ic.InjuredName,
                        ContactNumber = ic.ContactNumber // تم التأكد من وجودها في InjuredColleague.cs و ViewModel
                    }).ToList();
                }
            }
            return View(model);
        }

        // POST: MyProfile/EditColleagueInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditColleagueInfo(ColleagueInfoViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var colleagueInfo = await _context.ColleagueInfos
                                                    .Include(ci => ci.MartyrColleagues)
                                                    .Include(ci => ci.DetainedColleagues)
                                                    .Include(ci => ci.InjuredColleagues)
                                                    .FirstOrDefaultAsync(ci => ci.LawyerIdNumber == lawyerIdNumber);

                if (colleagueInfo == null)
                {
                    colleagueInfo = new ColleagueInfo();
                    colleagueInfo.LawyerIdNumber = lawyerIdNumber;
                    _context.ColleagueInfos.Add(colleagueInfo);
                }

                colleagueInfo.KnowsOfMartyrColleagues = model.KnowsOfMartyrColleagues;
                colleagueInfo.HasMartyrs = model.HasMartyrs;
                colleagueInfo.KnowsOfDetainedColleagues = model.KnowsOfDetainedColleagues;
                colleagueInfo.HasDetained = model.HasDetained;
                colleagueInfo.KnowsOfInjuredColleagues = model.KnowsOfInjuredColleagues;
                colleagueInfo.HasInjured = model.HasInjured;

                // تحديث الزملاء الشهداء
                UpdateCollection(colleagueInfo.MartyrColleagues, model.MartyrColleagues, (entity, vm) =>
                {
                    entity.MartyrName = vm.MartyrName;
                    entity.ContactNumber = vm.ContactNumber;
                });

                // تحديث الزملاء المعتقلين
                UpdateCollection(colleagueInfo.DetainedColleagues, model.DetainedColleagues, (entity, vm) =>
                {
                    entity.DetainedName = vm.DetainedName;
                    entity.ContactNumber = vm.ContactNumber;
                });

                // تحديث الزملاء المصابين
                UpdateCollection(colleagueInfo.InjuredColleagues, model.InjuredColleagues, (entity, vm) =>
                {
                    entity.InjuredName = vm.InjuredName;
                    entity.ContactNumber = vm.ContactNumber; // تم التأكد من وجودها في InjuredColleague.cs و ViewModel
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث معلومات الزملاء بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        // POST: MyProfile/AddMartyrColleagueField (لإضافة حقل زميل شهيد ديناميكي)
        [HttpPost]
        public PartialViewResult AddMartyrColleagueField(int index)
        {
            var model = new MartyrColleagueViewModel();
            return PartialView("_MartyrColleagueFields", model);
        }

        // POST: MyProfile/AddDetainedColleagueField (لإضافة حقل زميل معتقل ديناميكي)
        [HttpPost]
        public PartialViewResult AddDetainedColleagueField(int index)
        {
            var model = new DetainedColleagueViewModel();
            return PartialView("_DetainedColleagueFields", model);
        }

        // POST: MyProfile/AddInjuredColleagueField (لإضافة حقل زميل مصاب ديناميكي)
        [HttpPost]
        public PartialViewResult AddInjuredColleagueField(int index)
        {
            var model = new InjuredColleagueViewModel();
            return PartialView("_InjuredColleagueFields", model);
        }


        // GET: MyProfile/EditGeneralInfo
        // GET: MyProfile/EditGeneralInfo
        [PermissionAuthorizationFilter("تعديل المعلومات العامة", "صلاحية تعديل المعلومات العامة")]
        [AuditLog("تعديل", "تعديل المعلومات العامة")]
        public async Task<ActionResult> EditGeneralInfo()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            GeneralInfoViewModel model = new GeneralInfoViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var generalInfo = await _context.GeneralInfos
                                                    .Include(gi => gi.ReceivedAids)
                                                    .FirstOrDefaultAsync(gi => gi.LawyerIdNumber == lawyerIdNumber);
                if (generalInfo != null)
                {
                    model.Id = generalInfo.Id;
                    model.PracticesShariaLaw = generalInfo.PracticesShariaLaw;
                    model.ShariaLawPracticeStartDate = generalInfo.ShariaLawPracticeStartDate;
                    model.ReceivedAidFromSyndicate = generalInfo.ReceivedAidFromSyndicate;

                    model.ReceivedAids = generalInfo.ReceivedAids.Select(ra => new ReceivedAidViewModel
                    {
                        Id = ra.Id,
                        GeneralInfoId = ra.GeneralInfoId,
                        AidType = ra.AidType,
                        ReceivedDate = ra.ReceivedDate
                    }).ToList();
                }
            }
            // AvailableAidTypes يتم تهيئتها في Constructor الخاص بـ GeneralInfoViewModel
            return View(model);
        }

        // POST: MyProfile/EditGeneralInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditGeneralInfo(GeneralInfoViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                // إعادة تهيئة القوائم المنسدلة (AvailableAidTypes يتم تهيئتها في Constructor)
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var generalInfo = await _context.GeneralInfos
                                                    .Include(gi => gi.ReceivedAids)
                                                    .FirstOrDefaultAsync(gi => gi.LawyerIdNumber == lawyerIdNumber);

                if (generalInfo == null)
                {
                    generalInfo = new GeneralInfo();
                    generalInfo.LawyerIdNumber = lawyerIdNumber;
                    _context.GeneralInfos.Add(generalInfo);
                }

                generalInfo.PracticesShariaLaw = model.PracticesShariaLaw;
                generalInfo.ShariaLawPracticeStartDate = model.ShariaLawPracticeStartDate;
                generalInfo.ReceivedAidFromSyndicate = model.ReceivedAidFromSyndicate;

                // تحديث المساعدات المستلمة
                UpdateCollection(generalInfo.ReceivedAids, model.ReceivedAids, (entity, vm) =>
                {
                    entity.AidType = vm.AidType;
                    entity.ReceivedDate = vm.ReceivedDate;
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث المعلومات العامة بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }
            // إعادة تهيئة القوائم المنسدلة إذا لزم الأمر
            // (AvailableAidTypes يتم تهيئتها في Constructor الخاص بـ GeneralInfoViewModel)
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        // POST: MyProfile/AddReceivedAidField (لإضافة حقل مساعدة مستلمة ديناميكي)
        [HttpPost]
        public PartialViewResult AddReceivedAidField(int index)
        {
            var model = new ReceivedAidViewModel();
            // تأكد من تمرير AvailableAidTypes إلى الـ Partial View إذا كانت مطلوبة
            ViewData["AvailableAidTypes"] = new GeneralInfoViewModel().AvailableAidTypes;
            return PartialView("_ReceivedAidFields", model);
        }


        // GET: MyProfile/EditLawyerAttachments
        // GET: MyProfile/EditLawyerAttachments
        [PermissionAuthorizationFilter("تعديل مرفقات المحامي", "صلاحية تعديل مرفقات المحامي")]
        [AuditLog("تعديل", "تعديل مرفقات المحامي")]
        public async Task<ActionResult> EditLawyerAttachments()
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            LawyerAttachmentsListViewModel model = new LawyerAttachmentsListViewModel();

            if (!string.IsNullOrEmpty(lawyerIdNumber))
            {
                var lawyer = await _context.Lawyers
                                            .Include(l => l.LawyerAttachments)
                                            .FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);
                if (lawyer != null)
                {
                    model.LawyerIdNumber = lawyer.IdNumber;
                    model.Attachments = lawyer.LawyerAttachments.Select(la => new LawyerAttachmentViewModel
                    {
                        Id = la.Id,
                        LawyerIdNumber = la.LawyerIdNumber,
                        AttachmentType = la.AttachmentType,
                        ExistingFileName = la.FileName,
                        Notes = la.Notes,
                        UploadDate = la.UploadDate,
                        FilePath = Url.Content("~/App_Data/Uploads/LawyerAttachments/" + la.FileName) // استخدام المسار الصحيح
                    }).ToList();
                }
            }
            // AvailableAttachmentTypes يتم تهيئتها في Constructor الخاص بـ LawyerAttachmentsListViewModel
            // أو في LawyerAttachmentViewModel constructor
            model.AvailableAttachmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "PersonalPhoto", Text = "صورة شخصية" },
                new SelectListItem { Value = "IDPhotoWithSlip", Text = "صورة هوية مع السليب" },
                new SelectListItem { Value = "GraduationCertificate", Text = "شهادة تخرج" },
                new SelectListItem { Value = "PracticeCertificate", Text = "شهادة مزاولة مهنة" },
                new SelectListItem { Value = "RentalContract", Text = "عقد إيجار" },
                new SelectListItem { Value = "LawyerPrisonerAffidavit", Text = "افادة ان المحامي اسير" },
                new SelectListItem { Value = "SyndicateDocuments", Text = "مستندات او افادات او ايصالات او بطاقات صادرة عن النقابة" },
                new SelectListItem { Value = "MedicalReport", Text = "تقرير طبي" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };
            return View(model);
        }

        // POST: MyProfile/EditLawyerAttachments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLawyerAttachments(LawyerAttachmentsListViewModel model)
        {
            string lawyerIdNumber = await GetCurrentLawyerIdNumber();
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "لا يوجد محامٍ مرتبط بهذا الحساب.";
                // إعادة تهيئة القوائم المنسدلة
                model.AvailableAttachmentTypes = new List<SelectListItem> // إعادة تهيئة القائمة في حالة الخطأ
                {
                    new SelectListItem { Value = "PersonalPhoto", Text = "صورة شخصية" },
                    new SelectListItem { Value = "IDPhotoWithSlip", Text = "صورة هوية مع السليب" },
                    new SelectListItem { Value = "GraduationCertificate", Text = "شهادة تخرج" },
                    new SelectListItem { Value = "PracticeCertificate", Text = "شهادة مزاولة مهنة" },
                    new SelectListItem { Value = "RentalContract", Text = "عقد إيجار" },
                    new SelectListItem { Value = "LawyerPrisonerAffidavit", Text = "افادة ان المحامي اسير" },
                    new SelectListItem { Value = "SyndicateDocuments", Text = "مستندات او افادات او ايصالات او بطاقات صادرة عن النقابة" },
                    new SelectListItem { Value = "MedicalReport", Text = "تقرير طبي" },
                    new SelectListItem { Value = "Other", Text = "أخرى" }
                };
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            if (ModelState.IsValid)
            {
                var lawyer = await _context.Lawyers
                                            .Include(l => l.LawyerAttachments)
                                            .FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);

                if (lawyer == null)
                {
                    TempData["ErrorMessage"] = "لم يتم العثور على ملف المحامي.";
                    model.AvailableAttachmentTypes = new List<SelectListItem> // إعادة تهيئة القائمة في حالة الخطأ
                    {
                        new SelectListItem { Value = "PersonalPhoto", Text = "صورة شخصية" },
                        new SelectListItem { Value = "IDPhotoWithSlip", Text = "صورة هوية مع السليب" },
                        new SelectListItem { Value = "GraduationCertificate", Text = "شهادة تخرج" },
                        new SelectListItem { Value = "PracticeCertificate", Text = "شهادة مزاولة مهنة" },
                        new SelectListItem { Value = "RentalContract", Text = "عقد إيجار" },
                        new SelectListItem { Value = "LawyerPrisonerAffidavit", Text = "افادة ان المحامي اسير" },
                        new SelectListItem { Value = "SyndicateDocuments", Text = "مستندات او افادات او ايصالات او بطاقات صادرة عن النقابة" },
                        new SelectListItem { Value = "MedicalReport", Text = "تقرير طبي" },
                        new SelectListItem { Value = "Other", Text = "أخرى" }
                    };
                    return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
                }

                // معالجة المرفقات الموجودة والمضافة حديثًا
                foreach (var attachmentVm in model.Attachments)
                {
                    LawyerAttachment existingAttachment = null;
                    if (attachmentVm.Id > 0)
                    {
                        existingAttachment = lawyer.LawyerAttachments.FirstOrDefault(la => la.Id == attachmentVm.Id);
                    }

                    if (existingAttachment == null)
                    {
                        // مرفق جديد
                        existingAttachment = new LawyerAttachment();
                        existingAttachment.LawyerIdNumber = lawyerIdNumber;
                        lawyer.LawyerAttachments.Add(existingAttachment);
                    }

                    // تحديث الخصائص المشتركة
                    existingAttachment.AttachmentType = attachmentVm.AttachmentType;
                    existingAttachment.Notes = attachmentVm.Notes;

                    // معالجة تحميل الملف الجديد
                    if (attachmentVm.File != null && attachmentVm.File.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(attachmentVm.File.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}"; // لضمان اسم فريد
                        var path = Path.Combine(Server.MapPath("~/App_Data/Uploads/LawyerAttachments"), uniqueFileName);

                        // إنشاء المجلد إذا لم يكن موجودًا
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        attachmentVm.File.SaveAs(path);
                        existingAttachment.FileName = uniqueFileName;
                        existingAttachment.FilePath = path; // حفظ المسار الكامل
                        existingAttachment.UploadDate = DateTime.Now; // تعيين تاريخ الرفع
                        existingAttachment.FileSize = attachmentVm.File.ContentLength; // حفظ حجم الملف
                        existingAttachment.ContentType = attachmentVm.File.ContentType; // حفظ نوع المحتوى
                    }
                    else if (existingAttachment.Id > 0 && string.IsNullOrEmpty(attachmentVm.ExistingFileName))
                    {
                        // إذا كان مرفقًا موجودًا وتم مسح اسم الملف الحالي (أي تم حذفه من الواجهة)
                        // يمكنك إضافة منطق لحذف الملف الفعلي من الخادم هنا
                        if (!string.IsNullOrEmpty(existingAttachment.FilePath) && System.IO.File.Exists(existingAttachment.FilePath))
                        {
                            System.IO.File.Delete(existingAttachment.FilePath);
                        }
                        existingAttachment.FileName = null;
                        existingAttachment.FilePath = null;
                        existingAttachment.UploadDate = null;
                        existingAttachment.FileSize = 0;
                        existingAttachment.ContentType = null;
                    }
                }

                // حذف المرفقات التي لم تعد موجودة في النموذج المرسل
                var attachmentsToDelete = lawyer.LawyerAttachments
                                                .Where(la => !model.Attachments.Any(vm => vm.Id == la.Id))
                                                .ToList();
                foreach (var attachment in attachmentsToDelete)
                {
                    // يمكنك إضافة منطق لحذف الملف الفعلي من الخادم هنا قبل حذفه من DB
                    if (!string.IsNullOrEmpty(attachment.FilePath) && System.IO.File.Exists(attachment.FilePath))
                    {
                        System.IO.File.Delete(attachment.FilePath);
                    }
                    _context.LawyerAttachments.Remove(attachment);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث المرفقات بنجاح.";
                return RedirectToAction("Index"); // إعادة توجيه لعرض الرسالة
            }

            // إعادة تهيئة القوائم المنسدلة إذا لزم الأمر
            model.AvailableAttachmentTypes = new List<SelectListItem> // إعادة تهيئة القائمة في حالة الخطأ
            {
                new SelectListItem { Value = "PersonalPhoto", Text = "صورة شخصية" },
                new SelectListItem { Value = "IDPhotoWithSlip", Text = "صورة هوية مع السليب" },
                new SelectListItem { Value = "GraduationCertificate", Text = "شهادة تخرج" },
                new SelectListItem { Value = "PracticeCertificate", Text = "شهادة مزاولة مهنة" },
                new SelectListItem { Value = "RentalContract", Text = "عقد إيجار" },
                new SelectListItem { Value = "LawyerPrisonerAffidavit", Text = "افادة ان المحامي اسير" },
                new SelectListItem { Value = "SyndicateDocuments", Text = "مستندات او افادات او ايصالات او بطاقات صادرة عن النقابة" },
                new SelectListItem { Value = "MedicalReport", Text = "تقرير طبي" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };
            return View(model); // هنا لا نستخدم RedirectToAction لأننا نريد عرض نفس النموذج مع أخطاء التحقق
        }

        // POST: MyProfile/AddLawyerAttachmentField (لإضافة حقل مرفق ديناميكي)
        [HttpPost]
        public PartialViewResult AddLawyerAttachmentField(int index)
        {
            var model = new LawyerAttachmentViewModel();
            // تأكد من تمرير AvailableAttachmentTypes إلى الـ Partial View إذا كانت مطلوبة
            ViewData["AvailableAttachmentTypes"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "PersonalPhoto", Text = "صورة شخصية" },
                new SelectListItem { Value = "IDPhotoWithSlip", Text = "صورة هوية مع السليب" },
                new SelectListItem { Value = "GraduationCertificate", Text = "شهادة تخرج" },
                new SelectListItem { Value = "PracticeCertificate", Text = "شهادة مزاولة مهنة" },
                new SelectListItem { Value = "RentalContract", Text = "عقد إيجار" },
                new SelectListItem { Value = "LawyerPrisonerAffidavit", Text = "افادة ان المحامي اسير" },
                new SelectListItem { Value = "SyndicateDocuments", Text = "مستندات او افادات او ايصالات او بطاقات صادرة عن النقابة" },
                new SelectListItem { Value = "MedicalReport", Text = "تقرير طبي" },
                new SelectListItem { Value = "Other", Text = "أخرى" }
            };
            return PartialView("_LawyerAttachmentFields", model);
        }

        // دالة مساعدة لتحديث المجموعات (إضافة/تعديل/حذف)
        private void UpdateCollection<TEntity, TViewModel>(ICollection<TEntity> existingCollection, List<TViewModel> newViewModels, System.Action<TEntity, TViewModel> updateAction)
            where TEntity : class, new()
            where TViewModel : class, new()
        {
            // الحصول على معرفات العناصر الموجودة حاليًا في قاعدة البيانات
            var existingIds = new HashSet<int>(existingCollection.Select(e => (int)e.GetType().GetProperty("Id").GetValue(e)));

            foreach (var viewModel in newViewModels)
            {
                int vmId = (int)viewModel.GetType().GetProperty("Id").GetValue(viewModel);

                if (vmId == 0) // عنصر جديد
                {
                    TEntity newEntity = new TEntity();
                    updateAction(newEntity, viewModel);
                    // تعيين المفتاح الخارجي إذا كان موجودًا في الكائن الأب
                    // هذا الجزء يتطلب معرفة بمعرف الكائن الأب (LawyerIdNumber أو FamilyDetailId أو HealthStatusId إلخ)
                    // يمكن تمريره كمعامل إضافي لهذه الدالة أو جلبة من السياق
                    // حالياً، سيتم تعيينه في الإجراءات الفردية
                    existingCollection.Add(newEntity);
                }
                else // عنصر موجود
                {
                    TEntity existingEntity = existingCollection.FirstOrDefault(e => (int)e.GetType().GetProperty("Id").GetValue(e) == vmId);
                    if (existingEntity != null)
                    {
                        updateAction(existingEntity, viewModel);
                    }
                }
            }

            // حذف العناصر التي تم إزالتها من الـ ViewModel
            var vMIds = new HashSet<int>(newViewModels.Select(vm => (int)vm.GetType().GetProperty("Id").GetValue(vm)));
            var itemsToDelete = existingCollection.Where(e => !vMIds.Contains((int)e.GetType().GetProperty("Id").GetValue(e)) && (int)e.GetType().GetProperty("Id").GetValue(e) != 0).ToList();

            foreach (var item in itemsToDelete)
            {
                _context.Entry(item).State = EntityState.Deleted;
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
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
