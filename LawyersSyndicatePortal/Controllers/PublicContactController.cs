// Path: LawyersSyndicatePortal\Controllers\PublicContactController.cs
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models; // لضمان الوصول إلى مودل ContactMessage

namespace LawyersSyndicatePortal.Controllers
{
 
    public class PublicContactController : Controller
    {
        // استخدام ApplicationDbContext للوصول إلى قاعدة البيانات
        private readonly ApplicationDbContext _context;

        public PublicContactController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: PublicContact/Contact
        // يعرض نموذج الاتصال العام
        public ActionResult Contact()
        {
            return View();
        }

        // POST: PublicContact/Contact
        // يتعامل مع إرسال نموذج الاتصال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                // تعيين تاريخ الإرسال قبل الحفظ
                model.SentDate = DateTime.Now;
                // تعيين حالة القراءة الافتراضية
                model.IsRead = false;
                // تعيين تاريخ الرد كـ null افتراضياً
                model.ReplyDate = null;

                try
                {
                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();
                    // تعيين رسالة نجاح لعرضها في نفس الصفحة
                    ViewBag.SuccessMessage = "تم إرسال رسالتك بنجاح. سنتواصل معك قريباً!";
                    // لا تقم بإعادة التوجيه، بل أعد نفس العرض مع الرسالة
                    ModelState.Clear(); // مسح حالة النموذج لعرض نموذج فارغ بعد النجاح
                    return View(new ContactMessage()); // أعد نموذجًا جديدًا فارغًا
                }
 


                catch (Exception ex)
                {
                    // تم تعديل هذا الجزء لحل تحذير CS0168
                    // يمكنك استخدام 'ex' هنا لتسجيل الخطأ الفعلي
                    // هذا يساعد في تصحيح الأخطاء في المستقبل
                    System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");

                    // تعيين رسالة خطأ لعرضها في نفس الصفحة
                    ViewBag.ErrorMessage = "حدث خطأ أثناء إرسال رسالتك. يرجى المحاولة مرة أخرى لاحقاً.";

                    // يمكنك أيضًا إظهار رسالة الخطأ للمستخدم لأغراض التصحيح فقط
                    // ViewBag.ErrorMessage = $"حدث خطأ أثناء إرسال رسالتك: {ex.Message}";

                    // إعادة عرض النموذج بنفس البيانات لإظهار رسالة الخطأ
                    return View("Index", model);
                }




            }

            // إذا كان النموذج غير صالح أو حدث خطأ، أعد العرض مع أخطاء التحقق من الصحة أو رسالة الخطأ
            return View(model);
        }

        // GET: PublicContact/ContactConfirmation
        // يعرض صفحة تأكيد بعد إرسال الرسالة بنجاح
        // تم الإبقاء على هذا الأكشن في حال أردت استخدامه لأغراض أخرى
        public ActionResult ContactConfirmation()
        {
            return View();
        }

        // Dispose method to release resources
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
