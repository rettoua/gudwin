using System;
using System.Collections.Generic;
using Ext.Net;
using Newtonsoft.Json;
using Smartline.Mapping;
using Smartline.Server.Runtime.Accounting;
using Smartline.Web.WebLogging;

namespace Smartline.Web {
    public partial class tracker : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                User user = Helper.GetUserFromSession(Session);
                if (user == null) {
                    WebLoggingHelper.AddAction(LoggingAction.Logout);
                    Helper.Logout(Response);
                    return;
                }
                InternalUser internalUser = Helper.GetInternalUserFromSession(Session);
                string loggedText = internalUser != null ? GetDisplayName(internalUser.UserName, internalUser.Name) : GetDisplayName(user.UserName, user.Name);
                authUser.Text = loggedText;
                HiddenTabs(user, internalUser);

                btnAccounting.Hidden = true;
               X.Call("HideLogoLoad");
            }
        }

        private void HiddenTabs(User user, InternalUser internalUser) {
            if (internalUser != null) {
                tabs.Items["tabMap"].Hidden = false;
                tabs.Items["tabSettings"].Hidden = true;
                tabs.Items["tabReport"].Hidden = true;
                tabs.Items["tabAdmin"].Hidden = true;
                tabs.Items["tabStateMonitor"].Hidden = true;
                tabs.Items["tabTraffic"].Hidden = true;
                tabs.Items["tabAccounting"].Hidden = true;
                return;
            }
            if (user.UserName == "Administrator" || user.IsAdmin) {
                tabs.Items["tabMap"].Hidden = true;
                tabs.Items["tabSettings"].Hidden = true;
                tabs.Items["tabReport"].Hidden = true;
                tabs.Items["tabAdmin"].Hidden = false;
                tabs.Items["tabStateMonitor"].Hidden = false;
                tabs.Items["tabTraffic"].Hidden = false;
                tabs.Items["tabAccounting"].Hidden = false;
                tabs.SetActiveTab(tabs.Items["tabAdmin"]);
            }
        }

        protected void ClickSignOut(object sender, DirectEventArgs e) {
            WebLoggingHelper.AddAction(LoggingAction.Logout);
            Helper.Logout(Response);
        }

        private string GetDisplayName(string userName, string name) {
            if (!string.IsNullOrEmpty(name)) {
                return string.Format("{0} ({1})", userName, name);
            }
            return userName;
        }

        [DirectMethod]
        public static string CreatePayment(int userId, int amount) {
            var request = PaymentManager.CreatePaymentRequest(userId, amount);
            PaymentManager.SavePaymentRequest(request);
            var url = PaymentManager.GetPaymentUrl(request);
            return url;
        }

        [DirectMethod]
        public static List<PaymentConfirmationShort> GetPayments(int userId, DateTime from, DateTime to) {
            return CouchbaseManager.GetDoneTransation<PaymentConfirmationShort>(userId, from, to);
        }

        [DirectMethod]
        public static List<WriteOff> GetWriteOffs(int userId, DateTime from, DateTime to) {
            return CouchbaseManager.GetDoneWriteOffs<WriteOff>(userId, from, to);
        }

        /// <summary>
        /// uses as short description of PaymentConfirmation class
        /// </summary>
        public class PaymentConfirmationShort {
            [JsonProperty("userId")]
            public int UserId { get { return Convert.ToInt32(Description); } }

            [JsonProperty("paymentTime")]
            public DateTime PaymentTime { get; set; }

            /// <summary>
            /// sandbox status means test mode
            /// </summary>
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("orderId")]
            public ulong OrderId { get; set; }

            /// <summary>
            /// represent userid
            /// </summary>
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("acc_amount_before")]
            public double AccountAmountBefore { get; set; }

            [JsonProperty("acc_amount_after")]
            public double AccountAmountAfter { get; set; }
        }
    }
}