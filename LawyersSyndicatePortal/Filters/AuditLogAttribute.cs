using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using System;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using PagedList;
using System.Data.Entity;
using System.Linq;

namespace LawyersSyndicatePortal.Filters
{
    /// <summary>
    /// مرشح إجراء مخصص لتسجيل الإجراءات الإدارية تلقائياً.
    /// </summary>
    public class AuditLogAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// اسم الإجراء الذي سيتم تسجيله.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// وصف اختياري للإجراء يمكن تعيينه عند تطبيق المرشح.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// دالة بناء افتراضية لضمان التوافق مع الاستخدامات القديمة للسمة.
        /// </summary>
        public AuditLogAttribute() { }

        /// <summary>
        /// دالة بناء جديدة تقبل اسم الإجراء ووصفه.
        /// </summary>
        /// <param name="name">اسم الإجراء.</param>
        /// <param name="description">وصف الإجراء.</param>
        public AuditLogAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // إنشاء مهمة جديدة لتسجيل الإجراء بشكل غير متزامن
            Task.Run(() =>
            {
                // يجب أن تكون عملية الاتصال بقاعدة البيانات منفصلة عن السياق الرئيسي للطلب
                using (var dbContext = new ApplicationDbContext())
                {
                    try
                    {
                        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                        var actionName = filterContext.ActionDescriptor.ActionName;

                        string adminName = "Guest";
                        string tableName = controllerName; // افتراض أن اسم الجدول هو اسم المتحكم
                        int? entityId = null;

                        // الحصول على اسم المستخدم الكامل
                        if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                        {
                            var userId = filterContext.HttpContext.User.Identity.GetUserId();
                            var user = dbContext.Users.Find(userId);
                            if (user != null)
                            {
                                adminName = user.FullName;
                            }
                            else
                            {
                                adminName = filterContext.HttpContext.User.Identity.Name;
                            }
                        }

                        // محاولة استخراج المفتاح الأساسي للكيان من معاملات الإجراء
                        var idParam = filterContext.ActionParameters.FirstOrDefault(p => p.Key.ToLower() == "id");
                        if (idParam.Value != null)
                        {
                            entityId = Convert.ToInt32(idParam.Value);
                        }

                        // إنشاء سجل تدقيق جديد
                        var log = new AuditLog
                        {
                            AdminName = adminName,
                            ControllerName = controllerName,
                            Action = actionName,
                            Details = string.IsNullOrWhiteSpace(this.Description) ? this.Name : this.Description,
                            Timestamp = DateTime.Now,
                            TableName = tableName,
                            EntityId = entityId
                        };

                        dbContext.AuditLogs.Add(log);
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error logging audit: {ex.Message}");
                    }
                }
            });

            base.OnActionExecuting(filterContext);
        }
    }
}
