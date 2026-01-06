using System.Web.Mvc;
using System.Threading.Tasks;
using LawyersSyndicatePortal.Services;
using LawyersSyndicatePortal.Models;
using System.Data.Entity;
using System;
using Microsoft.AspNet.Identity;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.ViewModels;
using System.Linq;

namespace LawyersSyndicatePortal.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController()
        {
            _context = new ApplicationDbContext();
        }

        // دالة لإرسال إشعار دفع.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إنشاء تعميم إداري", "صلاحية إنشاء وإرسال تعميم إداري")]
        [AuditLog("إرسال", "إرسال تعميم إداري جديد")]
        public async Task<ActionResult> ComposeBroadcast(ComposeBroadcastViewModel model)
        {
            if (ModelState.IsValid)
            {
                var broadcast = new Broadcast
                {
                    Subject = model.Subject,
                    Body = model.Body,
                    SenderId = User.Identity.GetUserId(),
                    SentDate = DateTime.Now,
                    IsRead = false
                };

                _context.Broadcasts.Add(broadcast);
                await _context.SaveChangesAsync();

                // New Step: Send a notification to all users
                try
                {
                    var allDeviceTokens = await _context.DeviceTokens.Select(t => t.Token).ToListAsync();

                    if (allDeviceTokens != null && allDeviceTokens.Any())
                    {
                        // CORRECTED LINE: Generate the URL for the broadcast details page
                        // Assuming you have an action 'ViewBroadcast' that displays broadcast details by ID
                        var broadcastUrl = Url.Action("ViewBroadcast", "Messages", new { id = broadcast.Id }, Request.Url.Scheme);

                        foreach (var deviceToken in allDeviceTokens)
                        {
                            // CORRECTED LINE: Now you are passing the URL to the SendNotification method
                            await FirebaseNotificationService.SendNotification(deviceToken, model.Subject, model.Body, broadcastUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to send Firebase broadcast notification: {ex.Message}");
                }

                TempData["SuccessMessage"] = "تم إرسال التعميم بنجاح.";
                return RedirectToAction("AdminBroadcasts");
            }
            return View(model);
        }
        // هذه الدالة تمثل المنطق الذي ستقوم بإنشائه لجلب الرمز من قاعدة البيانات.
        private async Task<string> GetDeviceTokenForUser(string userId)
        {
            var deviceToken = await _context.DeviceTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return deviceToken?.Token;
        }


        // مكان إضافة الإجراء في ملف NotificationsController.cs
        [HttpPost]
        public async Task<ActionResult> SaveDeviceToken(string token)
        {
            // الحصول على هوية المستخدم الحالي.
            // تأكد من أن هذا الجزء يعمل بشكل صحيح في مشروعك.
            var userId = User.Identity.GetUserId();

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
            {
                var existingToken = await _context.DeviceTokens.FirstOrDefaultAsync(t => t.UserId == userId);

                if (existingToken == null)
                {
                    _context.DeviceTokens.Add(new DeviceToken
                    {
                        UserId = userId,
                        Token = token,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    existingToken.Token = token;
                }

                await _context.SaveChangesAsync();
                return new HttpStatusCodeResult(200);
            }
            return new HttpStatusCodeResult(400);
        }
        // دالة Dispose لتحرير الموارد
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
