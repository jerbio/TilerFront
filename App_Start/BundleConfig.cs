using System.Web;
using System.Web.Optimization;

namespace TilerFront
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/TilerCss").Include(
                    "~/CSS/TexGyreFont.css",
                    "~/CSS/ui-lightness/jquery-ui-1.10.4.custom.css",
                    "~/CSS/WeatherIcon.css",
                    "~/CSS/jquery.timepicker.css",
                    "~/CSS/MainUI.css",
                    "~/CSS/SubCalEventStyles.css",
                    "~/CSS/MonthOverviewIni.css",
                    "~/CSS/fullcalendar.css",
                    "~/CSS/AddNewEventDesktop.css",
                    "~/CSS/LoggedHomeUser.css",
                    "~/CSS/SelectedSubEvent.css",
                    "~/CSS/bootstrap-datepicker.css",
                    "~/CSS/SearchDesktop.css"
                    ));

            bundles.Add(new ScriptBundle("~/Content/TilerJS").Include(
                      "~/Scripts/moment.min.js",
                    "~/Scripts/fullcalendar.min.js",
                    "~/Scripts/WebControl.js",
                    "~/Scripts/MonthOverviewIni.js",
                    "~/Scripts/HomePageControl.js",
                    "~/Scripts/SelectedEvent.js",
                    "~/Scripts/AddNewEvent_Desktop0.js",
                    "~/Scripts/SearchHandler.js",
                    "~/Scripts/SeachHandlerDesktop.js",
                    "~/Scripts/bootstrap-datepicker.js",
                    "~/Scripts/jquery.timepicker.min.js",
                    "~/Scripts/datepair.js",
                    "~/Scripts/heartcode-canvasloader-min-0.9.1.js"
                      ));
        }
    }
}
