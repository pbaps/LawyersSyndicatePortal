using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LawyersSyndicatePortal.Services
{
    public static class FirebaseNotificationService
    {


      //  private const string Path2 = "App_Data\\firebase-adminsdk.json";
        private const string Path2 = @"App_Data\firebase-adminsdk.json";
        private static bool isInitialized = false;

        private static void InitializeFirebase()
        {
            if (isInitialized)
            {
                return;
            }

            // يجب عليك استبدال "path/to/your/serviceAccountKey.json" بالمسار الفعلي لملف JSON الخاص بك.
            var credential = GoogleCredential.FromFile(Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, Path2));

            FirebaseApp.Create(new AppOptions()
            {
                Credential = credential,
            });

            isInitialized = true;
        }

        public static async Task SendNotification(string deviceToken, string title, string body, string url)
        {
            // ... (your existing FirebaseApp initialization logic) ...

            var message = new Message()
            {
                Token = deviceToken,
            /*    Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                    // أضف أيقونة أو عناصر بصرية أخرى هنا إذا أردت
                  ///  Icon = "/Content/images/logo.png"
                },
                */
                // هذا هو الجزء الجديد: حمولة بيانات للواجهة الأمامية
                Data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "url", url }
            }
            };

            // Send the message...
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            Console.WriteLine($"Successfully sent message: {response}");
        }
    }
}