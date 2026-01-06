using System.Web;
using System.Web.Optimization;

namespace LawyersSyndicatePortal
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // 1. jQuery Core - The foundation for most scripts.
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // 2. SignalR - Crucial addition to fix the error.
            //    It MUST be loaded after jQuery.
            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                        "~/Scripts/jquery.signalR-{version}.min.js"));

            // 3. jQuery Validation - Must be loaded after jQuery Core.
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js"));

            // 4. Bootstrap - Core UI library.
            //    Use Bootstrap JS from a local file if needed, otherwise use CDN in HTML.
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));

            // 5. MDB (Material Design for Bootstrap)
            bundles.Add(new ScriptBundle("~/bundles/mdb").Include(
                      "~/Scripts/mdb.min.js"));

            // 6. Modernizr - Often placed in the <head> of the document.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // For CSS bundles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/site.css",
                      "~/Content/mdb.rtl.min.css"));
        }
    }
}
