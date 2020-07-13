using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ext.Net;
using Smartline.Mapping;
using Smartline.Web.manage;

namespace Smartline.Web {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class admin : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            LoadContentByPermissions();
            if (!Page.IsPostBack) {                
                Helper.HideMask();                
            }
        }

        private void LoadContentByPermissions() {
            PanelAdmin.ContentControls.Add(LoadControl("users.ascx"));
            var user = Session["user"] as User;
            if (user != null && user.UserName == "Administrator") {
                PanelTrackers.ContentControls.Add(LoadControl("trackers.ascx"));
            } else {
                PanelTrackers.Hide();
            }
        }

        [DirectMethod]
        public static bool ExistTrackerId(int trackerId) {
            return CouchbaseManager.ExistTrackerId(trackerId);
        }

        [DirectMethod]
        public static string LoadTrackers(int userId) {
            var user = CouchbaseManager.AuthenticateUser((ulong)userId);
            if (user == null) {
                return string.Empty;
            }
            return JSON.Serialize(user.Trackers);
        }

        [DirectMethod]
        public static string GetUsersByCurrentUser(int excludeUser) {
            var currentUser = System.Web.HttpContext.Current.Session["user"] as User;
            List<User> users = ManageHelper.LoadDependedUsers(currentUser);
            IEnumerable<User> userWithoutExcludedUser = users.Where(user => user.Id != (ulong)excludeUser);
            return JSON.Serialize(userWithoutExcludedUser);
        }

        [DirectMethod]
        public static bool ResetTrackers(int fromUserId, int toUserId, int[] trackers) {
            User fromUser = CouchbaseManager.GetUser(fromUserId.ToString(CultureInfo.InvariantCulture));
            User toUser = CouchbaseManager.GetUser(toUserId.ToString(CultureInfo.InvariantCulture));
            IEnumerable<Tracker> trackersToChange = fromUser.Trackers.Where(track => trackers.Contains(track.Id));
            toUser.Trackers.AddRange(trackersToChange);
            CouchbaseManager.UpdateUserWithCas(toUser, CouchbaseAssignHelper.AddNewTrackers);
            fromUser.Trackers.RemoveAll(track => trackers.Contains(track.Id));
            CouchbaseManager.UpdateUserWithCas(fromUser, CouchbaseAssignHelper.RemoveTrackers);
            return true;
        }
    }
}