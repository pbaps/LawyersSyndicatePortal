// Path: LawyersSyndicatePortal\Controllers\MessagesController.cs
using System;
using System.Collections.Generic; // Required for List<T>
using System.Data.Entity; // Required for EntityFramework operations like Include, ToListAsync, CountAsync
using System.Linq; // Required for LINQ extensions like Where, Select, OrderByDescending, DistinctBy
using System.Threading.Tasks; // Required for async/await
using System.Web.Mvc; // Required for Controller, ActionResult, SelectListItem, etc.
using Microsoft.AspNet.Identity; // Required for UserManager and Identity methods (e.g., GetUserId)
using Microsoft.AspNet.Identity.EntityFramework; // Required for UserStore
using LawyersSyndicatePortal.Models; // To ensure access to your models (ApplicationUser, Message, Broadcast, UserBroadcastReadStatus, ApplicationDbContext, ContactMessage - ADDED ContactMessage)
using LawyersSyndicatePortal.ViewModels; // To ensure access to your ViewModels (MessageListViewModel, SendMessageViewModel, BroadcastListViewModel, ComposeBroadcastViewModel, UserBroadcastViewModel, AdminMessageListViewModel - ADDED AdminMessageListViewModel, ReplyMessageViewModel - ADDED ReplyMessageViewModel)
using LawyersSyndicatePortal.Filters;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using LawyersSyndicatePortal.Services;

namespace LawyersSyndicatePortal.Controllers
{
    // Restrict access to this Controller to authenticated users only
    [Authorize]
    public class MessagesController : Controller
    {
        // Declare private readonly fields for DbContext and UserManager
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to initialize DbContext and UserManager
        public MessagesController()
        {
            _context = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
        }

        // Helper method to get the current user's ID number
        private string GetCurrentUserIdNumber()
        {
            string userId = User.Identity.GetUserId();
            var user = _userManager.FindById(userId);
            return user?.IdNumber;
        }

        // Helper method to get the Admin's ID number
        private async Task<string> GetAdminIdNumber()
        {
            // Assuming the admin user has a specific role or a known ID number
            // For simplicity, let's assume the admin user has a role "Admin"
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Roles.Any(r => r.RoleId == adminRole.Id));
                return adminUser?.IdNumber;
            }
            return null;
        }

        // Helper method to get the Admin's User ID (Identity ID)
        private async Task<string> GetAdminUserId()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Roles.Any(r => r.RoleId == adminRole.Id));
                return adminUser?.Id;
            }
            return null;
        }

        // GET: Messages/Inbox (User's Inbox)
        // GET: Messages/Inbox (User's Inbox)
        [PermissionAuthorizationFilter("صندوق الرسائل", "صلاحية الوصول إلى صندوق الوارد الخاص بالمستخدم")]
        [AuditLog("عرض", "عرض صندوق الوارد")]
        public async Task<ActionResult> Inbox(int page = 1, int pageSize = 10, string searchString = "")
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            string currentUserIdNumber = currentUser?.IdNumber;

            IQueryable<Message> messagesQuery = _context.Messages
                .Include(m => m.SenderUser)
                .Include(m => m.ReceiverUser)
                .Where(m => m.ReceiverId == currentUserId && !m.IsAdminBroadcast); // Exclude admin broadcasts from private inbox

            if (!string.IsNullOrEmpty(searchString))
            {
                messagesQuery = messagesQuery.Where(m =>
                    m.Subject.Contains(searchString) ||
                    m.Body.Contains(searchString) ||
                    m.SenderUser.FullName.Contains(searchString) ||
                    m.SenderUser.IdNumber.Contains(searchString));
            }

            int totalRecords = await messagesQuery.CountAsync();
            int current_pageSize = pageSize; // Use the passed pageSize

            var messages = await messagesQuery
                                .OrderByDescending(m => m.SentDate)
                                .Skip((page - 1) * current_pageSize)
                                .Take(current_pageSize)
                                .ToListAsync();

            var model = new MessageListViewModel
            {
                Messages = messages,
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize),
                TotalRecords = totalRecords,
                SearchString = searchString
            };

            // Populate PageSizes for the dropdown
            model.PageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" },
                new SelectListItem { Value = "100", Text = "100" }
            };

            return View(model);
        }

        // GET: Messages/Sent (User's Sent Messages)
        // GET: Messages/Inbox (User's Inbox)
        // GET: Messages/Sent (User's Sent Messages)

        [PermissionAuthorizationFilter("الرسائل المرسلة", "صلاحية الوصول إلى الرسائل المرسلة من قبل المستخدم")]
        [AuditLog("عرض", "عرض الرسائل المرسلة")]
        public async Task<ActionResult> Sent(int page = 1, int pageSize = 10, string searchString = "")
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            string currentUserIdNumber = currentUser?.IdNumber;

            IQueryable<Message> messagesQuery = _context.Messages
                .Include(m => m.SenderUser)
                .Include(m => m.ReceiverUser)
                .Where(m => m.SenderId == currentUserId)
                .Where(m => !m.IsAdminBroadcast); // Exclude admin broadcasts from sent messages

            if (!string.IsNullOrEmpty(searchString))
            {
                messagesQuery = messagesQuery.Where(m =>
                    m.Subject.Contains(searchString) ||
                    m.Body.Contains(searchString) ||
                    m.ReceiverUser.FullName.Contains(searchString) ||
                    m.ReceiverUser.IdNumber.Contains(searchString));
            }

            int totalRecords = await messagesQuery.CountAsync();
            int current_pageSize = pageSize;

            var messages = await messagesQuery
                                .OrderByDescending(m => m.SentDate)
                                .Skip((page - 1) * current_pageSize)
                                .Take(current_pageSize)
                                .ToListAsync();

            var model = new MessageListViewModel
            {
                Messages = messages,
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize),
                TotalRecords = totalRecords,
                SearchString = searchString
            };

            // Populate PageSizes for the dropdown
            model.PageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" },
                new SelectListItem { Value = "100", Text = "100" }
            };

            return View(model);
        }

        // GET: Messages/Inbox (User's Inbox)
        [PermissionAuthorizationFilter("صندوق الرسائل", "صلاحية الوصول إلى صندوق الوارد الخاص بالمستخدم")]
        [AuditLog("عرض", "عرض صندوق الوارد")]
        public async Task<ActionResult> AdminInbox(int page = 1, int pageSize = 10, string searchString = "")
        {
            string adminId = User.Identity.GetUserId(); // Get current admin's UserId

            IQueryable<Message> messagesQuery = _context.Messages
                .Include(m => m.SenderUser)
                .Include(m => m.ReceiverUser)
                .Where(m => m.ReceiverId == adminId); // Messages sent to the admin

            if (!string.IsNullOrEmpty(searchString))
            {
                messagesQuery = messagesQuery.Where(m =>
                    m.Subject.Contains(searchString) ||
                    m.Body.Contains(searchString) ||
                    m.SenderUser.FullName.Contains(searchString) ||
                    m.SenderUser.IdNumber.Contains(searchString));
            }

            int totalRecords = await messagesQuery.CountAsync();
            int current_pageSize = pageSize;

            var messages = await messagesQuery
                                .OrderByDescending(m => m.SentDate)
                                .Skip((page - 1) * current_pageSize)
                                .Take(current_pageSize)
                                .ToListAsync();

            var model = new MessageListViewModel
            {
                Messages = messages,
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize),
                TotalRecords = totalRecords,
                SearchString = searchString
            };

            return View(model);
        }


        // GET: Messages/AdminBroadcasts (Admin's Broadcasts - Sent by Admin)
        // GET: Messages/AdminInbox (Admin's Inbox)
        // GET: Messages/AdminBroadcasts (Admin's Broadcasts - Sent by Admin)
        [PermissionAuthorizationFilter("إدارة التعميمات", "صلاحية إدارة وعرض التعميمات الإدارية")]
        [AuditLog("إدارة", "إدارة وعرض التعميمات الإدارية")]
        public async Task<ActionResult> AdminBroadcasts(int page = 1, int pageSize = 10, string searchString = "")
        {
            string adminId = User.Identity.GetUserId();

            IQueryable<Broadcast> broadcastsQuery = _context.Broadcasts
                .Include(b => b.SenderUser)
                .Where(b => b.SenderId == adminId); // Only broadcasts sent by this admin

            if (!string.IsNullOrEmpty(searchString))
            {
                broadcastsQuery = broadcastsQuery.Where(b =>
                    b.Subject.Contains(searchString) ||
                    b.Body.Contains(searchString));
            }

            int totalRecords = await broadcastsQuery.CountAsync();
            int current_pageSize = pageSize;

            var broadcasts = await broadcastsQuery
                                .OrderByDescending(b => b.SentDate)
                                .Skip((page - 1) * current_pageSize)
                                .Take(current_pageSize)
                                .ToListAsync();

            var model = new BroadcastListViewModel // Use BroadcastListViewModel
            {
                // For AdminBroadcasts, we don't need per-user read status in this view,
                // so we'll just map the Broadcast objects directly to UserBroadcastViewModel
                // with IsReadForCurrentUser set to false or a default value, as it's not relevant here.
                Broadcasts = broadcasts.Select(b => new UserBroadcastViewModel { Broadcast = b, IsReadForCurrentUser = false }).ToList(),
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize),
                TotalRecords = totalRecords,
                SearchString = searchString
            };

            return View(model);
        }

        // NEW: Action to display broadcasts for general users
        // NEW: Action to display broadcasts for general users
        [PermissionAuthorizationFilter("عرض التعميمات", "صلاحية عرض التعميمات الموجهة للمستخدمين")]
        [AuditLog("عرض", "عرض التعميمات")]
        public async Task<ActionResult> UserBroadcasts(int page = 1, int pageSize = 10, string searchString = "")
        {
            string currentUserId = User.Identity.GetUserId();

            var broadcastsQuery = _context.Broadcasts
                                          .Include(b => b.SenderUser)
                                          .AsQueryable(); // Start with IQueryable to build the query

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                broadcastsQuery = broadcastsQuery.Where(b => b.Subject.Contains(searchString) ||
                                                             b.Body.Contains(searchString) ||
                                                             b.SenderUser.FullName.Contains(searchString));
            }

            int totalRecords = await broadcastsQuery.CountAsync();
            int current_pageSize = pageSize > 0 ? pageSize : 10;
            int totalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize);

            var broadcasts = await broadcastsQuery
                                .OrderByDescending(b => b.SentDate)
                                .Skip((page - 1) * current_pageSize)
                                .Take(current_pageSize)
                                .ToListAsync();

            // Prepare the list of UserBroadcastViewModel
            var userBroadcasts = new List<UserBroadcastViewModel>();
            foreach (var broadcast in broadcasts)
            {
                // Check if a read status exists for the current user and this broadcast
                var readStatus = await _context.UserBroadcastReadStatuses
                                               .FirstOrDefaultAsync(ubrs => ubrs.UserId == currentUserId && ubrs.BroadcastId == broadcast.Id);

                // If no read status exists, create a new one (as unread)
                if (readStatus == null)
                {
                    readStatus = new UserBroadcastReadStatus
                    {
                        UserId = currentUserId,
                        BroadcastId = broadcast.Id,
                        IsRead = false // Default to unread
                    };
                    _context.UserBroadcastReadStatuses.Add(readStatus);
                    await _context.SaveChangesAsync(); // Save the new unread status
                }

                userBroadcasts.Add(new UserBroadcastViewModel
                {
                    Broadcast = broadcast,
                    IsReadForCurrentUser = readStatus.IsRead
                });
            }

            var model = new BroadcastListViewModel
            {
                Broadcasts = userBroadcasts, // Pass the new list of UserBroadcastViewModel
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                SearchString = searchString
            };

            // Populate PageSizes for the dropdown
            model.PageSizes = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" },
                new SelectListItem { Value = "100", Text = "100" }
            };

            return View(model);
        }


        // GET: Messages/Details/{id}
        // GET: Messages/Details/{id}
        [PermissionAuthorizationFilter("عرض تفاصيل الرسالة", "صلاحية عرض تفاصيل الرسائل أو التعميمات")]
        [AuditLog("عرض", "عرض تفاصيل الرسالة أو التعميم")]
        public async Task<ActionResult> Details(int id, string type)
        {
            string currentUserId = User.Identity.GetUserId();

            if (type == "broadcast")
            {
                var broadcast = await _context.Broadcasts
                                              .Include(b => b.SenderUser)
                                              .FirstOrDefaultAsync(b => b.Id == id);

                if (broadcast == null)
                {
                    return HttpNotFound();
                }

                // Mark broadcast as read for the current user
                var readStatus = await _context.UserBroadcastReadStatuses
                                               .FirstOrDefaultAsync(ubrs => ubrs.UserId == currentUserId && ubrs.BroadcastId == id);

                if (readStatus == null)
                {
                    // Create new read status if it doesn't exist
                    readStatus = new UserBroadcastReadStatus
                    {
                        UserId = currentUserId,
                        BroadcastId = id,
                        IsRead = true,
                        // ReadDate = DateTime.Now // Uncomment if you added ReadDate
                    };
                    _context.UserBroadcastReadStatuses.Add(readStatus);
                }
                else
                {
                    // Update existing read status to true
                    readStatus.IsRead = true;
                    // readStatus.ReadDate = DateTime.Now; // Uncomment if you added ReadDate
                    _context.Entry(readStatus).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();

                // Create a Message object from Broadcast for the Details view
                var message = new Message
                {
                    Id = broadcast.Id,
                    Subject = broadcast.Subject,
                    Body = broadcast.Body,
                    SentDate = broadcast.SentDate,
                    SenderUser = broadcast.SenderUser,
                    IsAdminBroadcast = true, // Indicate it's a broadcast
                    // Set other properties as needed for display in Details view
                };
                ViewBag.IsBroadcast = true; // Flag for the view
                return View(message);
            }
            else // Regular message
            {
                var message = await _context.Messages
                                            .Include(m => m.SenderUser)
                                            .Include(m => m.ReceiverUser)
                                            .FirstOrDefaultAsync(m => m.Id == id && (m.ReceiverId == currentUserId || m.SenderId == currentUserId));

                if (message == null)
                {
                    return HttpNotFound();
                }

                // Mark message as read if the current user is the receiver and it's not already read
                if (message.ReceiverId == currentUserId && !message.IsRead)
                {
                    message.IsRead = true;
                    _context.Entry(message).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                ViewBag.IsBroadcast = false; // Flag for the view
                return View(message);
            }
        }


        // GET: Compose new private message (Admin only)
        // GET: Compose new private message (Admin only)
        [PermissionAuthorizationFilter("إنشاء رسالة خاصة", "صلاحية إنشاء وإرسال رسالة خاصة")]
        [AuditLog("إنشاء", "عرض صفحة إنشاء رسالة خاصة")]
        public async Task<ActionResult> Compose()
        {
            var model = new SendMessageViewModel
            {
                AvailableReceivers = await GetAvailableRecipientsForAdminCompose()
            };
            return View(model);
        }

        // POST: Compose new private message (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GET: Compose new private message (Admin only)
        [PermissionAuthorizationFilter("إنشاء رسالة خاصة", "صلاحية إنشاء وإرسال رسالة خاصة")]
        [AuditLog("إرسال", "إرسال رسالة خاصة")] // هنا نطبق المرشح
        public async Task<ActionResult> Compose(SendMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                var senderUser = await _userManager.FindByIdAsync(currentUserId);

                // Determine recipient(s) based on RecipientType
                List<ApplicationUser> recipients = new List<ApplicationUser>();

                if (model.RecipientType == "Specific" && !string.IsNullOrEmpty(model.ReceiverIdNumber))
                {
                    var specificRecipient = await _context.Users.FirstOrDefaultAsync(u => u.IdNumber == model.ReceiverIdNumber);
                    if (specificRecipient != null)
                    {
                        recipients.Add(specificRecipient);
                    }
                    else
                    {
                        ModelState.AddModelError("", "المستلم المحدد غير موجود.");
                        model.AvailableReceivers = await GetAvailableRecipientsForAdminCompose();
                        return View(model);
                    }
                }
                else if (model.RecipientType == "AllLawyers")
                {
                    recipients = await _context.Users.Where(u => u.LinkedLawyerIdNumber != null).ToListAsync();
                }
                else if (model.RecipientType == "AllTrainees")
                {
                    recipients = await _context.Users.Where(u => u.LinkedLawyerIdNumber != null && u.LinkedLawyer.IsTrainee).ToListAsync();
                }
                else if (model.RecipientType == "AllLawyersAndTrainees")
                {
                    recipients = await _context.Users.Where(u => u.LinkedLawyerIdNumber != null).ToListAsync();
                }
                else
                {
                    ModelState.AddModelError("", "الرجاء تحديد نوع المستلم.");
                    model.AvailableReceivers = await GetAvailableRecipientsForAdminCompose();
                    return View(model);
                }

                if (!recipients.Any())
                {
                    ModelState.AddModelError("", "لا يوجد مستلمون مطابقون للمعايير المحددة.");
                    model.AvailableReceivers = await GetAvailableRecipientsForAdminCompose();
                    return View(model);
                }

                foreach (var recipient in recipients)
                {
                    var message = new Message
                    {
                        SenderId = currentUserId,
                        ReceiverId = recipient.Id,
                        Subject = model.Subject,
                        Body = model.Body,
                        SentDate = DateTime.Now,
                        IsRead = false, // New messages are unread by default
                        IsAdminBroadcast = false // This is a private message
                    };
                    _context.Messages.Add(message);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إرسال الرسالة بنجاح!";
                return RedirectToAction("AdminInbox");
            }

            model.AvailableReceivers = await GetAvailableRecipientsForAdminCompose();
            return View(model);
        }

        // Helper to get available recipients for admin compose (excluding admin itself)
        // This is the correct, single definition of the method.
 
        private async Task<IEnumerable<SelectListItem>> GetAvailableRecipientsForAdminCompose()
        {
            string adminId = User.Identity.GetUserId();
            // Fetch users who are not the current admin
            var users = await _context.Users
                                      .Where(u => u.Id != adminId)
                                      .OrderBy(u => u.FullName)
                                      .Select(u => new SelectListItem
                                      {
                                          Value = u.IdNumber,
                                          Text = u.FullName // Display full name
                                      })
                                      .ToListAsync();
            return users;
        }

        // GET: Compose a broadcast (Admin only)
        // GET: Messages/ComposeBroadcast (Admin only)
        // GET: Messages/ComposeBroadcast (Admin only)
        [PermissionAuthorizationFilter("إنشاء تعميم إداري", "صلاحية إنشاء وإرسال تعميم إداري")]
        [AuditLog("عرض", "عرض صفحة إنشاء تعميم إداري")]
        public ActionResult ComposeBroadcast()
        {
            // Ensure this action returns a ComposeBroadcastViewModel
            return View(new ComposeBroadcastViewModel());
        }

        // POST: Compose a broadcast (Admin only)
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

                // خطوة جديدة: إرسال إشعار لجميع المستخدمين
                try
                {
                    var allDeviceTokens = await _context.DeviceTokens.Select(t => t.Token).ToListAsync();

                    if (allDeviceTokens != null && allDeviceTokens.Any())
                    {
                        // إنشاء عنوان URL لصفحة تفاصيل التعميم
                        // نفترض أن لديك دالة ViewBroadcast تستقبل مُعرّفًا (ID)
                        var broadcastUrl = Url.Action("ViewBroadcast", "Messages", new { id = broadcast.Id }, Request.Url.Scheme);

                        foreach (var deviceToken in allDeviceTokens)
                        {
                            // تمرير معامل URL الجديد إلى خدمتك
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

        // Add this action method inside your MessagesController
        public async Task<ActionResult> ViewBroadcast(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var broadcast = await _context.Broadcasts.FindAsync(id);

            if (broadcast == null)
            {
                return HttpNotFound();
            }

            return View(broadcast);
        }
        // GET: Messages/Reply/{id}
        [PermissionAuthorizationFilter("الرد على رسالة", "صلاحية الرد على الرسائل")]
        [AuditLog("عرض", "عرض صفحة الرد على رسالة")]
        public async Task<ActionResult> Reply(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Message originalMessage = await _context.Messages
                                                    .Include(m => m.SenderUser)
                                                    .Include(m => m.ReceiverUser)
                                                    .FirstOrDefaultAsync(m => m.Id == id);
            if (originalMessage == null)
            {
                TempData["ErrorMessage"] = "الرسالة الأصلية غير موجودة.";
                return RedirectToAction("Inbox");
            }

            string currentUserId = User.Identity.GetUserId();
            // Ensure the current user is either the sender or receiver of the original message
            if (originalMessage.SenderId != currentUserId && originalMessage.ReceiverId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "ليس لديك صلاحية للرد على هذه الرسالة.";
                return RedirectToAction("Inbox");
            }

            // Determine the recipient of the reply
            string replyRecipientId = (originalMessage.SenderId == currentUserId) ? originalMessage.ReceiverId : originalMessage.SenderId;
            string replyRecipientIdNumber = (originalMessage.SenderId == currentUserId) ? originalMessage.ReceiverUser?.IdNumber : originalMessage.SenderUser?.IdNumber;
            string replyRecipientFullName = (originalMessage.SenderId == currentUserId) ? originalMessage.ReceiverUser?.FullName : originalMessage.SenderUser?.FullName;

            var model = new SendMessageViewModel
            {
                ReceiverIdNumber = replyRecipientIdNumber,
                Subject = $"RE: {originalMessage.Subject}",
                Body = $"\n\n--- الرد على الرسالة الأصلية ---\nمن: {originalMessage.SenderUser?.FullName} ({originalMessage.SenderUser?.IdNumber})\nالتاريخ: {originalMessage.SentDate:yyyy-MM-dd HH:mm}\nالموضوع: {originalMessage.Subject}\n\n{originalMessage.Body}",
                AvailableReceivers = new List<SelectListItem>
                {
                    new SelectListItem { Value = replyRecipientIdNumber, Text = $"{replyRecipientFullName} ({replyRecipientIdNumber})", Selected = true }
                }
            };
            return View(model);
        }

        // POST: Messages/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("الرد على رسالة", "صلاحية الرد على الرسائل")]
        [AuditLog("إرسال", "إرسال الرد على رسالة")]
        public async Task<ActionResult> Reply(SendMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                var currentUser = await _userManager.FindByIdAsync(currentUserId);

                // Find the receiver by IdNumber
                var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.IdNumber == model.ReceiverIdNumber);

                if (receiverUser == null)
                {
                    ModelState.AddModelError("", "المستلم المحدد غير صالح.");
                    // Re-populate AvailableReceivers for the view
                    model.AvailableReceivers = new List<SelectListItem>
                    {
                        new SelectListItem { Value = model.ReceiverIdNumber, Text = "المستلم غير صالح", Selected = true }
                    };
                    return View(model);
                }

                var replyMessage = new Message
                {
                    SenderId = currentUserId,
                    SenderLawyerIdNumber = currentUser?.IdNumber,
                    SenderLawyerFullName = currentUser?.FullName,
                    ReceiverId = receiverUser.Id,
                    Subject = model.Subject,
                    Body = model.Body,
                    SentDate = DateTime.Now,
                    IsRead = false,
                    IsAdminBroadcast = false
                };

                _context.Messages.Add(replyMessage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال الرد بنجاح!";
                return RedirectToAction("Sent");
            }
            // If model state is not valid, re-populate AvailableReceivers
            model.AvailableReceivers = new List<SelectListItem>
            {
                new SelectListItem { Value = model.ReceiverIdNumber, Text = "المستلم غير صالح", Selected = true }
            };
            return View(model);
        }

        // NEW: GET: Messages/ComposeToAdmin (for regular users to send message to admin)
        // NEW: GET: Messages/ComposeToAdmin (for regular users to send message to admin)
        [PermissionAuthorizationFilter("إنشاء رسالة للمسؤول", "صلاحية إنشاء وإرسال رسالة للمسؤول")]
        [AuditLog("عرض", "عرض صفحة إنشاء رسالة للمسؤول")]
        public async Task<ActionResult> ComposeToAdmin()
        {
            // Get the admin's ID number
            string adminIdNumber = await GetAdminIdNumber();
            string adminUserId = await GetAdminUserId();
            var adminUser = await _userManager.FindByIdAsync(adminUserId);


            if (string.IsNullOrEmpty(adminIdNumber) || adminUser == null)
            {
                TempData["ErrorMessage"] = "لا يمكن العثور على حساب المسؤول لإرسال الرسائل.";
                return RedirectToAction("Index", "Home"); // Redirect to home or an error page
            }

            var model = new SendMessageViewModel
            {
                ReceiverIdNumber = adminIdNumber, // Pre-fill with admin's ID number
                // Make the AvailableReceivers list contain only the admin, and make it read-only for the user
                AvailableReceivers = new List<SelectListItem>
                {
                    new SelectListItem { Value = adminIdNumber, Text = $"المسؤول ({adminUser.FullName})", Selected = true }
                },
                RecipientType = "Specific" // Ensure this is set to specific
            };

            ViewBag.IsComposeToAdmin = true; // Flag to indicate this is the ComposeToAdmin view
            return View("ComposeToAdmin", model); // <--- تم التعديل هنا ليرجع View باسم "ComposeToAdmin"
        }

        // NEW: POST: Messages/ComposeToAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إنشاء رسالة للمسؤول", "صلاحية إنشاء وإرسال رسالة للمسؤول")]
        [AuditLog("إرسال", "إرسال رسالة إلى المسؤول")]
        public async Task<ActionResult> ComposeToAdmin(SendMessageViewModel model)
        {
            // Get the admin's ID number and User ID
            string adminIdNumber = await GetAdminIdNumber();
            string adminUserId = await GetAdminUserId();

            if (string.IsNullOrEmpty(adminIdNumber) || string.IsNullOrEmpty(adminUserId))
            {
                ModelState.AddModelError("", "لا يمكن العثور على حساب المسؤول لإرسال الرسائل.");
                // Re-populate AvailableReceivers for the view
                model.AvailableReceivers = new List<SelectListItem>
                {
                    new SelectListItem { Value = adminIdNumber, Text = "المسؤول غير موجود", Selected = true }
                };
                ViewBag.IsComposeToAdmin = true;
                return View("ComposeToAdmin", model); // <--- تم التعديل هنا ليرجع View باسم "ComposeToAdmin"
            }

            // Force the receiver to be the admin, regardless of user input
            model.ReceiverIdNumber = adminIdNumber;

            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                var currentUser = await _userManager.FindByIdAsync(currentUserId);

                var message = new Message
                {
                    SenderId = currentUserId,
                    SenderLawyerIdNumber = currentUser?.IdNumber, // Can be null if not a lawyer
                    SenderLawyerFullName = currentUser?.FullName,
                    ReceiverId = adminUserId, // Use the actual Identity ID of the admin
                    Subject = model.Subject,
                    Body = model.Body,
                    SentDate = DateTime.Now,
                    IsRead = false,
                    IsAdminBroadcast = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال رسالتك إلى المسؤول بنجاح!";
                return RedirectToAction("Sent"); // Redirect to user's sent messages
            }

            // If ModelState is not valid, re-populate AvailableReceivers
            model.AvailableReceivers = new List<SelectListItem>
            {
                new SelectListItem { Value = adminIdNumber, Text = "المسؤول غير موجود", Selected = true }
            };
            ViewBag.IsComposeToAdmin = true;
            return View("ComposeToAdmin", model); // <--- تم التعديل هنا ليرجع View باسم "ComposeToAdmin"
        }


        // GET: Messages/SearchUsers (for dynamic search in admin compose)
        // GET: Messages/SearchUsers (for dynamic search in admin compose)
        [PermissionAuthorizationFilter("البحث عن مستخدمين", "صلاحية البحث عن مستخدمين")]
        [AuditLog("بحث", "البحث عن مستخدمين")]
        public async Task<JsonResult> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
            }

            string currentUserId = User.Identity.GetUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            string currentUserIdNumber = currentUser?.IdNumber;

            // جلب المستخدمين (باستثناء المسؤول الحالي) الذين يحتوي اسمهم الكامل أو رقم هويتهم على "searchTerm"
            // أولاً، جلب بيانات المستخدمين الخام
            var usersData = await _context.Users
                                          .Where(u => u.IdNumber != currentUserIdNumber &&
                                                      (u.FullName.Contains(searchTerm) || u.IdNumber.Contains(searchTerm)))
                                          .OrderBy(u => u.FullName)
                                          .Select(u => new { u.IdNumber, u.FullName }) // تحديد الخصائص الضرورية فقط
                                          .ToListAsync(); // تنفيذ الاستعلام وجلب البيانات إلى الذاكرة

            // ثم، تحويلها إلى قائمة "SelectListItem" في الذاكرة
            var users = usersData.Select(u => new SelectListItem
            {
                Value = u.IdNumber,
                Text = $"{u.FullName} ({u.IdNumber})" // تنسيق النص في الذاكرة
            }).ToList();

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        // NEW: GET: Messages/AdminContactMessages (Admin's Contact Messages List)
        [PermissionAuthorizationFilter("عرض رسائل التواصل مع الإدارة", "صلاحية عرض رسائل التواصل مع الإدارة")]
        [AuditLog("عرض", "عرض رسائل التواصل مع الإدارة")]
        public async Task<ActionResult> AdminContactMessages(int page = 1, int pageSize = 10, string searchString = "", bool? isReadFilter = null, bool? isRepliedFilter = null)
        {
            IQueryable<ContactMessage> messagesQuery = _context.ContactMessages.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                messagesQuery = messagesQuery.Where(m =>
                    m.FullName.Contains(searchString) ||
                    m.Email.Contains(searchString) ||
                    m.Subject.Contains(searchString) ||
                    m.MessageBody.Contains(searchString));
            }

            // Apply read status filter
            if (isReadFilter.HasValue)
            {
                messagesQuery = messagesQuery.Where(m => m.IsRead == isReadFilter.Value);
            }

            // 💡 Add replied status filter
            if (isRepliedFilter.HasValue)
            {
                // If true, filter messages that have a ReplyDate (meaning they have been replied to)
                if (isRepliedFilter.Value)
                {
                    messagesQuery = messagesQuery.Where(m => m.ReplyDate.HasValue);
                }
                // If false, filter messages that do not have a ReplyDate
                else
                {
                    messagesQuery = messagesQuery.Where(m => !m.ReplyDate.HasValue);
                }
            }

            int totalRecords = await messagesQuery.CountAsync();
            int current_pageSize = pageSize > 0 ? pageSize : 10;
            int totalPages = (int)Math.Ceiling((double)totalRecords / current_pageSize);

            var messages = await messagesQuery
                .OrderByDescending(m => m.SentDate) // Order by latest messages first
                .Skip((page - 1) * current_pageSize)
                .Take(current_pageSize)
                .ToListAsync();

            var model = new AdminMessageListViewModel
            {
                Messages = messages,
                SearchString = searchString,
                IsReadFilter = isReadFilter,
                IsRepliedFilter = isRepliedFilter, // 💡 Pass the new filter value to the view model
                CurrentPage = page,
                PageSize = current_pageSize,
                TotalMessages = totalRecords,
                TotalPages = totalPages,
                PageSizes = new List<SelectListItem>
        {
            new SelectListItem { Value = "10", Text = "10" },
            new SelectListItem { Value = "20", Text = "20" },
            new SelectListItem { Value = "50", Text = "50" },
            new SelectListItem { Value = "100", Text = "100" }
        }
            };

            return View(model);
        }
        //////////////////////////////////////////////

        // NEW: GET: Messages/AdminContactMessageDetails (Admin's Contact Message Details)

        [PermissionAuthorizationFilter("عرض تفاصيل رسالة تواصل", "صلاحية عرض تفاصيل رسالة تواصل مع الإدارة")]
        [AuditLog("عرض", "عرض تفاصيل رسالة تواصل")]
        public async Task<ActionResult> AdminContactMessageDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactMessage message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return HttpNotFound();
            }

            // Mark message as read when admin views details
            if (!message.IsRead)
            {
                message.IsRead = true;
                _context.Entry(message).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            // Prepare ReplyMessageViewModel for the reply form
            var replyModel = new ReplyMessageViewModel
            {
                OriginalMessageId = message.Id,
                SenderName = message.FullName,
                SenderEmail = message.Email,
                OriginalSubject = message.Subject,
                OriginalMessageBody = message.MessageBody,
                ReplySubject = $"RE: {message.Subject}" // Default reply subject
            };

            ViewBag.ReplyModel = replyModel; // Pass the reply model to the view
            return View(message);
        }

        //---

        // NEW: POST: Messages/ReplyToContactMessage (Admin replies to a contact message)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Crucial for accepting HTML from TinyMCE
        [PermissionAuthorizationFilter("الرد على رسالة تواصل", "صلاحية الرد على رسائل التواصل مع الإدارة")]
        [AuditLog("إرسال", "إرسال رد على رسالة تواصل")]
        public async Task<ActionResult> ReplyToContactMessage(ReplyMessageViewModel model)
        {
            // 1. Check for Model Validation BEFORE database interaction
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "يرجى التحقق من صحة بيانات الرد المدخلة. بعض الحقول المطلوبة قد تكون فارغة أو غير صالحة.";
                // If validation fails, return to the details page, preserving the ID
                return RedirectToAction("AdminContactMessageDetails", new { id = model.OriginalMessageId });
            }

            // 2. Fetch the original message from the database
            ContactMessage originalMessage = await _context.ContactMessages.FindAsync(model.OriginalMessageId);

            // 3. Handle cases where the message is not found
            if (originalMessage == null)
            {
                TempData["ErrorMessage"] = "الرسالة الأصلية غير موجودة للرد عليها.";
                return RedirectToAction("AdminContactMessagesIndex");
            }

            try
            {
                // 4. Encapsulate email logic in a separate, reusable method
                await SendEmailAsync(originalMessage.Email, model.ReplySubject, model.ReplyBody);

                // 5. Update message status in the database
                originalMessage.ReplyDate = DateTime.Now;
                originalMessage.IsRead = true;
                _context.Entry(originalMessage).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال الرد بنجاح وتحديث حالة الرسالة.";
                return RedirectToAction("AdminContactMessageDetails", new { id = model.OriginalMessageId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Email sending failed: {ex.Message}");
                TempData["ErrorMessage"] = $"فشل إرسال الرد: {ex.Message}";
                return RedirectToAction("AdminContactMessageDetails", new { id = model.OriginalMessageId });
            }
        }

        /// <summary>
 
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Use a more descriptive constant for the app password
            const string fromEmail = "pbagaza.hr@gmail.com";
            const string appPassword = "emri gqvw nbyx aeug"; // App Password

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, "نقابة المحامين - غزة- الدعم الفني ");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body; // The body is already HTML from TinyMCE
                mail.IsBodyHtml = true;

                // Force TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }

        // NEW: POST: Messages/MarkContactMessageAsRead
        [HttpPost]
        [PermissionAuthorizationFilter("وضع علامة مقروءة", "صلاحية وضع علامة 'مقروءة' على رسائل التواصل")]
        [AuditLog("تحديث", "وضع علامة 'مقروءة' على رسالة تواصل")]
        public async Task<JsonResult> MarkContactMessageAsRead(int id)
        {
            ContactMessage message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new { success = false, message = "الرسالة غير موجودة." });
            }

            message.IsRead = true;
            _context.Entry(message).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تم وضع علامة 'مقروءة'." });
        }

        // NEW: POST: Messages/MarkContactMessageAsUnread
        [HttpPost]
        [PermissionAuthorizationFilter("وضع علامة غير مقروءة", "صلاحية وضع علامة 'غير مقروءة' على رسائل التواصل")]
        [AuditLog("تحديث", "وضع علامة 'غير مقروءة' على رسالة تواصل")]
        public async Task<JsonResult> MarkContactMessageAsUnread(int id)
        {
            ContactMessage message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new { success = false, message = "الرسالة غير موجودة." });
            }

            message.IsRead = false;
            _context.Entry(message).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تم وضع علامة 'غير مقروءة'." });
        }

        [HttpPost]
        [PermissionAuthorizationFilter("حذف رسالة تواصل", "صلاحية حذف رسائل التواصل مع الإدارة")]
        [AuditLog("حذف", "حذف رسالة تواصل")]
        public ActionResult DeleteContactMessage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var message = _context.ContactMessages.Find(id);
                if (message == null)
                {
                    TempData["ErrorMessage"] = "لم يتم العثور على الرسالة.";
                    return Json(new { success = false, message = "لم يتم العثور على الرسالة." });
                }

                // حذف الرسالة من قاعدة البيانات
                _context.ContactMessages.Remove(message);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "تم حذف الرسالة بنجاح.";
                return Json(new { success = true, message = "تم حذف الرسالة بنجاح." });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الرسالة: " + ex.Message;
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الرسالة." });
            }
        }
        /// <summary>
        [PermissionAuthorizationFilter("عرض رسائل التواصل مع الإدارة", "صلاحية عرض رسائل التواصل مع الإدارة")]
        [AuditLog("عرض", "عرض رسائل التواصل مع الإدارة")]
        public ActionResult AdminContactMessagesIndex()
        {
            // تنفيذ المنطق لاسترداد وعرض رسائل الاتصال للمسؤول
            var messages = _context.ContactMessages.OrderByDescending(m => m.SentDate).ToList();
            return View(messages);
        }




        // Action to display the reply form for a specific message.
        // GET: /Messages/AdmiinReplyToContactMessage/5
        [PermissionAuthorizationFilter("عرض رسائل التواصل مع الإدارة", "صلاحية عرض رسائل التواصل مع الإدارة")]
        [AuditLog("عرض", "عرض رسائل التواصل مع الإدارة")]
        [ValidateInput(false)]
        public async Task<ActionResult> ContactMessages()
        {
            var messages = await _context.ContactMessages
                                        .OrderByDescending(m => m.SentDate)
                                        .ToListAsync();
            return View(messages);
        }


        [PermissionAuthorizationFilter("الرد على رسالة تواصل", "صلاحية الرد على رسائل التواصل مع الإدارة")]
        [AuditLog("عرض", "عرض صفحة الرد على رسالة تواصل")]
        public async Task<ActionResult> AdmiinReplyToContactMessage(int id)
        {
            var originalMessage = await _context.ContactMessages.FindAsync(id);

            if (originalMessage == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ReplyMessageViewModel
            {
                OriginalMessageId = originalMessage.Id,
                SenderName = originalMessage.FullName,
                SenderEmail = originalMessage.Email,
                OriginalSubject = originalMessage.Subject,
                OriginalMessageBody = originalMessage.MessageBody,
                ReplySubject = $"RE: {originalMessage.Subject}" // Pre-fill with RE:
            };

            // Set the message as read when the admin views the reply page.
            originalMessage.IsRead = true;
            await _context.SaveChangesAsync();

            return View(viewModel);
        }

        // هذا هو الإجراء الذي يستقبل بيانات النموذج (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("الرد على رسالة تواصل", "صلاحية الرد على رسائل التواصل مع الإدارة")]
        [AuditLog("إرسال", "إرسال رد على رسالة تواصل")]
        public async Task<ActionResult> AdmiinReplyToContactMessage(ReplyMessageViewModel model)
        {
            // لأغراض الأمان، يفضل تخزين هذا في ملف Web.config.
            const string fromPassword = "emri gqvw nbyx aeug";

            if (ModelState.IsValid)
            {
                var originalMessage = await _context.ContactMessages.FindAsync(model.OriginalMessageId);
                if (originalMessage == null)
                {
                    return HttpNotFound();
                }

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var fromAddress = new MailAddress("pbagaza.hr@gmail.com", " نقابة المحامين - غزة- الدعم الفني  ");
                    var toAddress = new MailAddress(originalMessage.Email);

                    // قم بتنسيق نص الرسالة باستخدام HTML
                    string emailBody = $"<p style='direction: rtl; font-weight: bold;'>{model.ReplyBody}</p>";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = model.ReplySubject,
                        Body = emailBody, // استخدم سلسلة الـ HTML هنا
                        IsBodyHtml = true // قم بتعيين هذا إلى true لعرض الـ HTML
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }

                    originalMessage.ReplyDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إرسال الرد بنجاح.";
                    return RedirectToAction("AdminContactMessages");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to send email: {ex.Message}");
                    TempData["ErrorMessage"] = $"حدث خطأ أثناء إرسال الرد: {ex.Message}";
                    return View(model);
                }
            }

            TempData["ErrorMessage"] = "يرجى التحقق من صحة البيانات المدخلة.";
            return View(model);
        }
        /////////////////انتهاء الرد على البريد 

        // Dispose method to release resources
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose(); // Dispose UserManager as well
            }
            base.Dispose(disposing);
        }
    }

    // Extension method for DistinctBy (if not using MoreLinq)
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
