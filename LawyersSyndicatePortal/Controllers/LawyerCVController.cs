using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Collections.Generic; // Required for List<SelectListItem>
using System;
using System.Net;
using System.IO;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    // A placeholder for the service layer to handle data retrieval.
    // This interface is used for dependency injection.
    public interface ILawyerService
    {
        Task<Lawyer> GetLawyerWithDetailsAsync(string lawyerIdNumber);
        Task<LawyerAttachment> GetAttachmentDetailsAsync(int attachmentId);
    }

    // A placeholder for the service implementation.
    // In a real application, this would be in a separate file/project.
    public class LawyerService : ILawyerService
    {
        private readonly ApplicationDbContext _context;

        public LawyerService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Method to get a lawyer with only the necessary details to map to the ViewModel.
        // This is more efficient than loading the entire object graph.
        public async Task<Lawyer> GetLawyerWithDetailsAsync(string lawyerIdNumber)
        {
            return await _context.Lawyers
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
                                 .Include(l => l.LawyerAttachments)
                                 .FirstOrDefaultAsync(l => l.IdNumber == lawyerIdNumber);
        }

        public async Task<LawyerAttachment> GetAttachmentDetailsAsync(int attachmentId)
        {
            return await _context.LawyerAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);
        }
    }

    // A placeholder for the PermissionAuthorizationFilter.
    // In a real application, this would contain the actual permission check logic.
 

    // A placeholder for the AuditLogAttribute.
    // In a real application, this would log the action details.
 
    [Authorize]
    public class LawyerCVController : Controller
    {
        private readonly ILawyerService _lawyerService; // Using a service layer for better structure.
        private ApplicationUserManager _userManager;

        // Constructor with dependency injection.
        // Note: For this to work, you need to configure a dependency injection container.
        public LawyerCVController(ILawyerService lawyerService)
        {
            _lawyerService = lawyerService;
        }

        // Default constructor for scenarios without DI (e.g., unit testing or simple applications).
        public LawyerCVController() : this(new LawyerService(new ApplicationDbContext()))
        {
            // The default constructor now uses the new service layer.
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

        // GET: LawyerCV/ViewMyCV
        [PermissionAuthorizationFilter("عرض تقرير التحديث", "عرض السيرة الذاتية الخاصة بالمحامي")]
        public async Task<ActionResult> ViewMyCV()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = await UserManager.FindByIdAsync(currentUserId);

            if (currentUser == null || string.IsNullOrEmpty(currentUser.LinkedLawyerIdNumber))
            {
                TempData["ErrorMessage"] = "خطأ: لا يمكن تحديد رقم هوية المحامي المرتبط بحسابك. يرجى التأكد من ربط حسابك بملف محامٍ.";
                return RedirectToAction("Index", "MyProfile");
            }

            return RedirectToAction("ViewCV", new { lawyerIdNumber = currentUser.LinkedLawyerIdNumber });
        }

        // GET: LawyerCV/ViewCV/{lawyerIdNumber}
        [PermissionAuthorizationFilter("عرض السيرة الذاتية", "عرض السيرة الذاتية لأي محامي برقم الهوية")]
        public async Task<ActionResult> ViewCV(string lawyerIdNumber)
        {
            if (string.IsNullOrEmpty(lawyerIdNumber))
            {
                TempData["ErrorMessage"] = "الرجاء توفير رقم هوية المحامي لعرض السيرة الذاتية.";
                return RedirectToAction("Index", "Home");
            }

            // Using the service layer to retrieve data.
            var lawyer = await _lawyerService.GetLawyerWithDetailsAsync(lawyerIdNumber);

            if (lawyer == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على سيرة ذاتية لهذا المحامي.";
                return RedirectToAction("Index", "Home");
            }

            // The following ViewModel population can be simplified using a tool like AutoMapper.
            var model = new LawyerDetailsViewModel();
            model.LawyerIdNumber = lawyer.IdNumber;
            model.LawyerFullName = lawyer.FullName;
            // ... (populate other properties as before) ...

            // Example of mapping PersonalDetails
            var personalDetail = lawyer.PersonalDetails.FirstOrDefault();
            if (personalDetail != null)
            {
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
                model.BankName = personalDetail.BankName;
                model.BankBranch = personalDetail.BankBranch;
                model.BankAccountNumber = personalDetail.BankAccountNumber;
                model.IBAN = personalDetail.IBAN;
                model.WalletType = personalDetail.WalletType;
                model.WalletAccountNumber = personalDetail.WalletAccountNumber;
                model.DateOfBirth = personalDetail.DateOfBirth;
            }

            // ... (continue populating the rest of the ViewModel) ...
            var lawyerAttachments = lawyer.LawyerAttachments;
            if (lawyerAttachments != null)
            {
                model.LawyerAttachments = lawyerAttachments.Select(la => new LawyerAttachmentViewModel
                {
                    Id = la.Id,
                    LawyerIdNumber = la.LawyerIdNumber,
                    AttachmentType = la.AttachmentType,
                    ExistingFileName = la.FileName,
                    Notes = la.Notes,
                    UploadDate = la.UploadDate,
                    FilePath = Url.Content("~/App_Data/Uploads/LawyerAttachments/" + la.FileName)
                }).ToList();
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.InfoMessage = TempData["InfoMessage"];

            return View("ViewMyCV", model);
        }

        // GET: LawyerCV/DisplayAttachment/{attachmentId}
        // This action now includes a basic authorization check.
        [PermissionAuthorizationFilter("عرض المرفقات في صفحة المستخدم", "عرض مرفق السيرة الذاتية")]
        public async Task<ActionResult> DisplayAttachment(int attachmentId)
        {
            var attachment = await _lawyerService.GetAttachmentDetailsAsync(attachmentId);

            if (attachment == null)
            {
                return HttpNotFound("المرفق غير موجود.");
            }

            // Get the current user's linked lawyer ID number.
            string currentUserId = User.Identity.GetUserId();
            var currentUser = await UserManager.FindByIdAsync(currentUserId);
            string linkedLawyerIdNumber = currentUser?.LinkedLawyerIdNumber;

            // Check if the current user is the owner of the attachment or has 'Admin' role.
            bool isAuthorized = (linkedLawyerIdNumber == attachment.LawyerIdNumber) || User.IsInRole("Admin");

            if (!isAuthorized)
            {
                // Return a Forbidden status code if the user is not authorized.
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "ليس لديك صلاحية الوصول إلى هذا المرفق.");
            }

            string filePath = Server.MapPath($"~/App_Data/Uploads/LawyerAttachments/{attachment.FileName}");

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("ملف المرفق غير موجود على الخادم.");
            }

            string contentType = MimeMapping.GetMimeMapping(attachment.FileName);

            return File(filePath, contentType);
        }

        // The Dispose method remains correct and is good practice.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                // The service layer should handle its own context disposal.
                // If it's a single-use service, dispose it here.
                // (e.g. (_lawyerService as LawyerService)?.Dispose();)
            }
            base.Dispose(disposing);
        }
    }
}
