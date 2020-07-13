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
    public partial class tracker : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!this.Page.IsPostBack) {
                var user = CouchbaseManager.GetUser(User.Identity.Name);
                authUser.Text = GetDisplayName(user);
                Session["user"] = user;
            }
        }

        protected void ClickSignOut(object sender, DirectEventArgs e) {
            FormsAuthentication.SignOut();
            ExtNet.Redirect(FormsAuthentication.LoginUrl);
        }

        private string GetDisplayName(User user) {
            if (!string.IsNullOrEmpty(user.Name)) {
                return string.Format("{0} ({1})", user.Name, user.UserName);
            }
            return user.UserName;
        }
    }
}