using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Ext.Net;
using Smartline.Mapping;
using Smartline.Web.WebLogging;

namespace Smartline.Web {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class entry : System.Web.UI.Page {

        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                LoadJavaScriptFiles();
                TryAuthenticateUser();
            }
        }

        private void LoadJavaScriptFiles() {
            Download.AddJScriptDynamicaly("entry", this);
        }

        private void TryAuthenticateUser() {
            HttpCookie cookie = Request.Cookies[Helper.CookieName];
            if (cookie == null) { return; }
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            var authData = JSON.Deserialize<AuthCookie>(ticket.UserData);
            if (authData == null || !authData.Remember) { return; }
            User user = Helper.AuthUser(authData.UserId);
            var internalUser = user.Operators.FirstOrDefault(o => o.Id + "" == authData.InternalUserId);
            AuthUser(user, authData.Remember, internalUser);
        }

        protected void btnLogin_Click(object sender, DirectEventArgs e) {
            Response.Cookies.Add(new HttpCookie("testcookeid"));
            string userName = txtUsername.Text;
            string password = txtPassword.Text;
            bool remember = chkSaveMe.Checked;
            User user = Helper.AuthUser(userName, password);
            InternalUser internalUser = Helper.GetInternalUser(user, userName, password);
            AuthUser(user, remember, internalUser);
        }

        private void AuthUser(User user, bool remember, InternalUser internalUser) {
            if (user != null) {
                Session["user"] = user;
                Session["internalUser"] = internalUser;
                if (internalUser != null && internalUser.IsBlocked) {
                    lblCheckError.Text = @"Пользователь заблокирован";
                    lblCheckError.Show();
                    X.AddScript("App.WindowLogin.el.unmask();");
                    return;
                }
                WebLoggingHelper.AddAction(LoggingAction.Login);
                FormsAuthentication.SetAuthCookie(FormsAuthentication.FormsCookieName, remember);
                ApplyRememberMe(user, internalUser, remember);
                FormsAuthentication.RedirectFromLoginPage(FormsAuthentication.FormsCookieName, remember);
                Response.Redirect(Request.QueryString["ReturnUrl"] + "" == ""
                                      ? FormsAuthentication.DefaultUrl
                                      : Request.QueryString["ReturnUrl"]);
            } else {
                lblCheckError.Text = @"Проверьте правильность ввода логина и пароля";
                lblCheckError.Show();
                X.AddScript("App.WindowLogin.el.unmask();");
            }
        }

        private void ApplyRememberMe(User user, InternalUser internalUser, bool remember) {
            ClearCookies();
            var expiryDate = remember ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(20);
            var authCookieData = new AuthCookie { UserId = user.Id + "", Remember = remember, InternalUserId = internalUser != null ? internalUser.Id + "" : "" };
            var ticket = new FormsAuthenticationTicket(2, user.Id + "", DateTime.Now, expiryDate, true, JSON.Serialize(authCookieData));
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authenticationCookie = new HttpCookie(Helper.CookieName, encryptedTicket) {
                Expires = ticket.Expiration
            };
            Response.Cookies.Add(authenticationCookie);
            var httpCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
            if (httpCookie != null) { httpCookie.Expires = authenticationCookie.Expires; }
        }

        private void ClearCookies() {
            Response.Cookies.Remove(Helper.CookieName);
        }

        private class AuthCookie {
            public string UserId { get; set; }
            public string InternalUserId { get; set; }
            public bool Remember { get; set; }
        }
    }
}