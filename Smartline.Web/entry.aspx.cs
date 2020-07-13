using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ext.Net;
using Smartline.Mapping.couchbase;

namespace Smartline.Web {
    [Ext.Net.DirectMethodProxyID(IDMode = Ext.Net.DirectMethodProxyIDMode.None)]
    public partial class entry : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {

        }

        protected void btnLogin_Click(object sender, DirectEventArgs e) {
            string userName = this.txtUsername.Text;
            string password = this.txtPassword.Text;
            string computeSecret = Smartline.Mapping.couchbase.User.ComputeSecret(userName, password);

            var user = CouchbaseManager.GetUser(userName, computeSecret);
            if (user != null) {
                FormsAuthentication.RedirectFromLoginPage(userName, false);
                Response.Redirect(Request.QueryString["ReturnUrl"] + "" == ""
                                      ? FormsAuthentication.DefaultUrl
                                      : Request.QueryString["ReturnUrl"]);
            } else {
                lblCheckError.Show();
            }
        }
    }
}