using System.Web;
using System.Web.Optimization;

namespace Expedicao
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/mask").Include(
                        "~/Scripts/jquery.mask.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/tingleStyle").Include(
                "~/Content/tingle.css"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                        "~/Scripts/highcharts.js"));

            bundles.Add(new ScriptBundle("~/bundles/utilService").Include(
                        "~/Scripts/util.service.js"));

            bundles.Add(new ScriptBundle("~/bundles/report").Include(
                      "~/Scripts/report.js"));

            bundles.Add(new ScriptBundle("~/bundles/envioViewModel").Include(
                "~/Scripts/ViewModel/EnvioViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/batchSends").Include(
                "~/Scripts/batchSends.js"));

            bundles.Add(new ScriptBundle("~/bundles/tingle").Include(
                "~/Scripts/tingle.js"));

            bundles.Add(new ScriptBundle("~/bundles/sendList").Include(
                "~/Scripts/sendList.js"));
        }
    }
}
