using System;
using System.Web;
using System.Web.Optimization;
using Smartline.Web.Add_Start;

namespace Smartline.Web {
    public class Global : HttpApplication {

        protected void Application_Start(object sender, EventArgs e) {
            Application["Ext.Net.LicenseKey"] = "OTQyNjM2ODMsMiw5OTk5LTEyLTMx";
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(object sender, EventArgs e) {

        }

        protected void Application_BeginRequest(object sender, EventArgs e) {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e) {

        }

        protected void Application_Error(object sender, EventArgs e) {

        }

        protected void Session_End(object sender, EventArgs e) {

        }

        protected void Application_End(object sender, EventArgs e) {

        }
    }
}