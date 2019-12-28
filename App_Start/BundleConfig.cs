using System.Configuration;
using System.Web;
using System.Web.Optimization;
using TilerElements;

namespace TilerFront
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static string OldLog;
        public static string LogLocation;
        public static string BigDataocation;
        public static void RegisterBundles(BundleCollection bundles)
        {
            OldLog = HttpContext.Current.Server.MapPath("~\\OldLogs\\");
            LogLocation = HttpContext.Current.Server.MapPath("~\\WagTapCalLogs\\");//initializes the log location\
            BigDataocation = HttpContext.Current.Server.MapPath("~\\BigDataLogs\\");//initializes the log location
            //LogLocation = @"C:\Users\OluJerome\Documents\Visual Studio 2010\Projects\LearnCuDAVS2010\LearnCUDAConsoleApplication\bin\Debug\WagTapCalLogs\";
            //LogLocation = @"C:\Users\OluJerome\Documents\Visual Studio 2010\Projects\LearnCuDAVS2010\LearnCUDAConsoleApplication\WagTapCalLogs\";
            TilerFront.LogControl.UpdateBigDataLogLocation(BigDataocation);
            TilerFront.LogControl.UpdateLogLocation(LogLocation);

            string apiKey = ConfigurationManager.AppSettings["googleMapsApiKey"];
            Location.updateApiKey(apiKey);
            bundles.Add(new ScriptBundle("~/Scripts/signalR").Include(
                        "~/Scripts/jquery.signalR-{version}.js"));

            
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      //"~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/TilerCss").Include(
                    "~/CSS/TexGyreFont.css",
                    "~/CSS/WeatherIcon.css",
                    "~/CSS/jquery.timepicker.css",
                    "~/CSS/MainUI.css",
                    "~/CSS/SubCalEventStyles.css",
                    "~/CSS/LoggedHomeUser.css",
                    "~/CSS/SelectedSubEvent.css",
                    "~/CSS/bootstrap-datepicker.css"
                    ));

            bundles.Add(new StyleBundle("~/Content/RegistrationJS").Include(
                    "~/Scripts/RegisterIni.js"
                ));

            bundles.Add(new StyleBundle("~/Content/ManagePageJS").Include(
                    "~/Scripts/ManageController.js"
                ));

            bundles.Add(new StyleBundle("~/Content/RegistrationCss").Include(
                    "~/CSS/MainUI.css",
                    "~/CSS/Registration.css"
                ));
            
            

            bundles.Add(new ScriptBundle("~/Content/TilerJS").Include(
                    "~/Scripts/jquery-2.1.1.js",
                    "~/Scripts/Utility.js",
                    "~/Scripts/Chart.min.js",
                    "~/Scripts/moment.min.js",
                    "~/Scripts/moment-timezone-with-data.min.js",
                    "~/Scripts/fullcalendar.min.js",
                    "~/Scripts/highcharts.js",
                    "~/Scripts/exporting.js",
                    "~/Scripts/WebControl.js",
                    "~/Scripts/HomePageControl.js",
                    "~/Scripts/SelectedEvent.js",
                    "~/Scripts/SearchHandler.js",
                    "~/Scripts/jquery.timepicker.min.js",
                    "~/Scripts/datepair.js",
                    "~/Scripts/heartcode-canvasloader-min-0.9.1.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/TilerDesktopCss").Include(
                    "~/CSS/WeatherIcon.css",
                    "~/CSS/jquery-ui.css",
                    "~/CSS/jquery.timepicker.css",
                    "~/CSS/SubCalEventStyles.css",
                    "~/CSS/MonthOverviewIni.css",
                    "~/CSS/fullcalendar.css",
                    "~/CSS/AddNewEventDesktop.css",
                    "~/CSS/bootstrap-datepicker.css",
                    "~/CSS/SearchDesktop.css"
                    ));

            bundles.Add(new ScriptBundle("~/Content/TilerDesktopJS").Include(
                    "~/Scripts/Utility.js",
                    "~/Scripts/MonthOverviewIni.js",
                    "~/Scripts/AddNewEvent_Desktop0.js",
                    "~/Scripts/jquery-ui.min.js",
                    "~/Scripts/SeachHandlerDesktop.js",
                    "~/Scripts/bootstrap-datepicker.js",
                    "~/Scripts/jquery.signalR-{version}.js",
                    "~/Scripts/moment.min.js",
                    "~/Scripts/moment-timezone-with-data.min.js",
                    "~/Scripts/Preview/Index.js",
                    "~/Scripts/Preview/PreviewDay.js",
                    "~/Scripts/Preview/Sleep.js",
                    "~/Scripts/Preview/Tardy.js"
                      ));

            bundles.Add(new ScriptBundle("~/Content/TilerMobileJS").Include(
                    "~/Scripts/Utility.js",
                    "~/Scripts/yui-min.js",
                    "~/Scripts/MapData.js",
                    "~/Scripts/jquery.knob.js",
                    "~/Scripts/blur.min.js",
                    "~/Scripts/jquery.foggy.min.js",
                    "~/Scripts/IndexIni.js",
                    "~/Scripts/SearchHandler.js",
                    "~/Scripts/AddNewEvent.js",
                    "~/Scripts/jquery.simpleWeather.js",
                    "~/Scripts/Notification.js",
                    "~/Scripts/jquery.mousewheel.min.js",
                    "~/Scripts/jquery.mobile-1.4.4.min.js",
                    "~/Scripts/jqm-datebox-1.4.4.core.min.js",
                    "~/Scripts/jqm-datebox-1.4.4.mode.slidebox.min.js",
                    "~/Scripts/jqm-datebox-1.4.4.mode.flipbox.min.js",
                    "~/Scripts/moment.min.js",
                    "~/Scripts/moment-timezone-with-data.min.js",
                    //"~/Scripts/YuiCombo/3.18.1/oop/oop-min.js&3.18.1/event…min.js&3.18.1/node-core/node-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/oop/oop-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-focus/event-focus-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-core/node-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dom-style/dom-style-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-base/node-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dial/dial-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-synthetic/event-synthetic-min.js",
                    "~/Scripts/YuiCombo/3.18.1/transition/transition-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-key/event-key-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-style/node-style-min.js",
                    "~/Scripts/YuiCombo/3.18.1/widget-base/widget-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/widget-htmlparser/widget-htmlparser-min.js",
                    "~/Scripts/YuiCombo/3.18.1/widget-skin/widget-skin-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-delegate/event-delegate-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-event-delegate/node-event-delegate-min.js",
                    "~/Scripts/YuiCombo/3.18.1/widget-uievents/widget-uievents-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-pluginhost/node-pluginhost-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dom-screen/dom-screen-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-screen/node-screen-min.js",
                    "~/Scripts/YuiCombo/3.18.1/base-build/base-build-min.js",
                    "~/Scripts/YuiCombo/3.18.1/yui-throttle/yui-throttle-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dd-ddm-base/dd-ddm-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/selector-css2/selector-css2-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-touch/event-touch-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-flick/event-flick-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-move/event-move-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dd-drag/dd-drag-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dd-gestures/dd-gestures-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-mouseenter/event-mouseenter-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-key/event-key-min.js",
                    "~/Scripts/YuiCombo/3.18.1/oop/oop-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-custom-base/event-custom-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-custom-complex/event-custom-complex-min.js",
                    "~/Scripts/YuiCombo/3.18.1/intl/intl-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dial/lang/dial_en.js",
                    "~/Scripts/YuiCombo/3.18.1/attribute-core/attribute-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/attribute-observable/attribute-observable-min.js",
                    "~/Scripts/YuiCombo/3.18.1/attribute-extras/attribute-extras-min.js",
                    "~/Scripts/YuiCombo/3.18.1/attribute-base/attribute-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/attribute-complex/attribute-complex-min.js",
                    "~/Scripts/YuiCombo/3.18.1/base-core/base-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/base-observable/base-observable-min.js",
                    "~/Scripts/YuiCombo/3.18.1/base-base/base-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/pluginhost-base/pluginhost-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/pluginhost-config/pluginhost-config-min.js",
                    "~/Scripts/YuiCombo/3.18.1/base-pluginhost/base-pluginhost-min.js",
                    "~/Scripts/YuiCombo/3.18.1/classnamemanager/classnamemanager-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-base/event-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dom-core/dom-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/dom-base/dom-base-min.js",
                    "~/Scripts/YuiCombo/3.18.1/selector-native/selector-native-min.js",
                    "~/Scripts/YuiCombo/3.18.1/selector/selector-min.js",
                    "~/Scripts/YuiCombo/3.18.1/node-core/node-core-min.js",
                    "~/Scripts/YuiCombo/3.18.1/event-focus/event-focus-min.js",
                    "~/Scripts/Preview/Index.js",
                    "~/Scripts/Preview/PreviewDay.js",
                    "~/Scripts/Preview/Sleep.js",
                    "~/Scripts/Preview/Tardy.js"
                      ));


            bundles.Add(new StyleBundle("~/Content/TilerMobileCss").Include(
                    "~/CSS/TilerMobile.css",
                    "~/CSS/TexGyreFont.css",
                    "~/CSS/Search.css",
                    "~/CSS/ui-lightness/jquery-ui-1.10.4.custom.css",
                    "~/CSS/WeatherIcon.css",
                    "~/CSS/jquery.timepicker.css",
                    "~/CSS/MainUI.css",
                    "~/CSS/SubCalEventStyles.css",
                    "~/CSS/AddNewEvent.css",
                    "~/CSS/LoggedHomeUser.css",
                    "~/CSS/SelectedSubEvent.css",
                    "~/CSS/bootstrap-datepicker.css",
                    "~/CSS/jquery.mobile-1.4.4.min.css",
                    "~/CSS/jqm-datebox-1.4.4.min.css",
                    "~/Scripts/YuiCombo/3.18.1/widget-base/assets/skins/sam/widget-base.css",
                    "~/Scripts/YuiCombo/3.18.1/dial/assets/skins/sam/dial.css",
                    "~/CSS/Notification.css",
                    "~/CSS/SearchMobile.css"
                    ));
            bundles.Add(new StyleBundle("~/Content/TilerDesktopSettingsCss").Include(
                    "~/CSS/SettingsDesktop.css"
                ));

            bundles.Add(new ScriptBundle("~/Content/HomePageJS").Include(
                    "~/Scripts/cbpAnimatedHeader.js",
                    "~/Scripts/classie.js",
                    "~/Scripts/contact_me.js",
                    "~/Scripts/freelancer.js",
                    "~/Scripts/cbpAnimatedHeader.js",
                    "~/Scripts/jqBootstrapValidation.js"
                      ));
            bundles.Add(new ScriptBundle("~/Content/HomePageCSS").Include(
                    "~/Scripts/cbpAnimatedHeader.js",
                    "~/Scripts/classie.js",
                    "~/Scripts/contact_me.js",
                    "~/Scripts/freelancer.js",
                    "~/Scripts/cbpAnimatedHeader.js",
                    "~/Scripts/jqBootstrapValidation.js"
                      ));
            
        }
    }
}
