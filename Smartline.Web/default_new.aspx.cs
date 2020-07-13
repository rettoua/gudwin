using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web {
    public partial class _default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack && !X.IsAjaxRequest) {
                var user = Session["user"] as User;
                if (user == null) {
                    Helper.Logout(Response);
                    return;
                }
                Desktop1.StartMenu.Title = string.Format("Пользователь: {0}", GetDisplayName(user));
            }
        }

        private static string GetDisplayName(User user) {
            if (!string.IsNullOrEmpty(user.Name)) {
                return string.Format("{0} ({1})", user.Name, user.UserName);
            }
            return user.UserName;
        }

        protected void ClickSignOut(object sender, DirectEventArgs e) {
            Helper.Logout(Response);
        }
    }
}