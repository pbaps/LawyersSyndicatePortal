using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Diagnostics;
using System.Web.Hosting;

namespace LawyersSyndicatePortal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                // **الطريقة الموصى بها: قراءة محتوى حساب الخدمة من ملف خارجي لأغراض الأمان.**
                // تأكد من أن ملف firebase-adminsdk.json موجود في مجلد App_Data.
                string serviceAccountFilePath = HostingEnvironment.MapPath("~/App_Data/firebase-adminsdk.json");

                // التحقق من وجود الملف قبل محاولة قراءته.
                if (!File.Exists(serviceAccountFilePath))
                {
                    Debug.WriteLine("خطأ: لم يتم العثور على ملف حساب خدمة Firebase في المسار المحدد.");
                    // يمكنك اختيار رمي استثناء لإيقاف التطبيق إذا كان هذا الملف حرجاً.
                    return;
                }

                // قراءة محتوى الملف بالكامل في سلسلة نصية واحدة.
                string serviceAccountContent = File.ReadAllText(serviceAccountFilePath);

                // تهيئة Firebase Admin SDK باستخدام محتوى السلسلة النصية لـ JSON.
                AppOptions options = new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(serviceAccountContent)
                };

                // إنشاء نسخة التطبيق من Firebase.
                FirebaseApp.Create(options);

                Debug.WriteLine("تمت تهيئة Firebase Admin SDK بنجاح.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"حدث خطأ فادح أثناء تهيئة Firebase: {ex.Message}");
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
