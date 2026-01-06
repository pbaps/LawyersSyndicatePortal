using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LawyersSyndicatePortal.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        // الإجراء الخاص بالخطأ 404 (الصفحة غير موجودة)
        public ActionResult NotFound()
        {
            // تعيين رمز حالة HTTP إلى 404
            Response.StatusCode = 404;
            // إخبار IIS بتجاوز معالجة الأخطاء المخصصة الخاصة به
            // هذا يضمن أن MVC هو من يعرض الصفحة ولا يتم إعادة توجيه إضافية
            Response.TrySkipIisCustomErrors = true;

            // تمرير رسالة إلى View
            ViewBag.Message = "الصفحة التي تبحث عنها غير موجودة.";

            // إرجاع View الخاص بـ NotFound
            return View();
        }

        // الإجراء الخاص بالخطأ 500 (خطأ في الخادم)
        public ActionResult ServerError()
        {
            // تعيين رمز حالة HTTP إلى 500
            Response.StatusCode = 500;
            // إخبار IIS بتجاوز معالجة الأخطاء المخصصة الخاصة به
            Response.TrySkipIisCustomErrors = true;

            // تمرير رسالة إلى View
            ViewBag.Message = "حدث خطأ غير متوقع على الخادم. يرجى المحاولة مرة أخرى لاحقاً.";

            // إرجاع View الخاص بـ ServerError
            return View();
        }

        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            Response.StatusDescription = "Forbidden";
            return View();
        }

        // GET: Error/NotFound
        // يمكنك إضافة إجراءات أخرى لأنواع الأخطاء الأخرى مثل 404
 

        // إجراء عام للتعامل مع الأخطاء الأخرى التي يتم اعتراضها بواسطة HandleErrorAttribute
        // هذا الإجراء يُستخدم افتراضياً إذا لم يتم تحديد View محدد لـ HandleErrorAttribute
        public ActionResult Error()
        {
            // تعيين رمز حالة HTTP إلى 500 كافتراضي للأخطاء العامة
            Response.StatusCode = 500;
            // إخبار IIS بتجاوز معالجة الأخطاء المخصصة الخاصة به
            Response.TrySkipIisCustomErrors = true;

            // تمرير رسالة إلى View
            ViewBag.Message = "حدث خطأ أثناء معالجة طلبك.";

            // إرجاع View الخاص بـ Error (عادةً ما يكون في Views/Shared/Error.cshtml)
            return View();
        }
    }
}