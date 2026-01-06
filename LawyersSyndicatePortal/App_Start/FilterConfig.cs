using System.Web;
using System.Web.Mvc;

namespace LawyersSyndicatePortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // هذا السطر يقوم بتسجيل HandleErrorAttribute ليعالج الاستثناءات غير المعالجة
            // وتوجيهها إلى صفحة الخطأ الافتراضية المحددة في Web.config (أو Error.cshtml افتراضياً).
            filters.Add(new HandleErrorAttribute());
        }
    }
}
