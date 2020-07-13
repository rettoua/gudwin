using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web {
    public class Helper {
        public const string CookieName = "gsdjghdfjksghdfjkslghl";

        public static void Logout(HttpResponse response) {
            FormsAuthentication.SignOut();
            var aCookie = new HttpCookie(CookieName) { Expires = DateTime.Now.AddDays(-1) };
            response.Cookies.Add(aCookie);
            X.Redirect(FormsAuthentication.LoginUrl);
        }

        public static User AuthUser(string userName, string password) {
            string computeSecret = User.ComputeSecretNew(password);
            var user = CouchbaseManager.AuthenticateUser(userName, computeSecret);
            if (user != null) {
                return user;
            }
            computeSecret = User.ComputeSecret(userName, password);
            user = CouchbaseManager.AuthenticateUser(userName, computeSecret);
            return user;
        }

        public static User AuthUser(string id) {
            return CouchbaseManager.GetUser(id);
        }

        public static bool IsDemoUser(HttpSessionState session) {
            var user = session["user"] as User;
            return user != null && user.UserName == "Demo";
        }

        public static InternalUser GetInternalUser(User user, string userName, string secret) {
            if (user == null || !user.Operators.Any()) { return null; }
            return user.Operators.FirstOrDefault(o => o.UserName == userName && o.Secret == User.ComputeSecretNew(secret));

        }

        public static void HideMask() {
            X.Call("parent.HideLogoLoad");
        }

        public static bool UserNameExist(string userName) {
            List<User> users = GetUsers(userName);
            return users.Count > 0;
        }

        public static bool UserNameExist(string userName, ulong userUid) {
            List<User> users = GetUsers(userName);
            if (users.Count == 0) {
                return false;
            }
            foreach (User user in users) {
                if (user.UserName == userName && user.Id != userUid) {
                    return true;
                }
                foreach (InternalUser internalUser in user.Operators) {
                    if (internalUser.UserName == userName && internalUser.Id != userUid) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<User> GetUsers(string userName) {
            return CouchbaseManager.GetUsersByUserName(userName);
        }

        public static User GetUserFromSession(HttpSessionState session) {
            if (session == null) { return null; }
            return session["user"] as User;
        }

        public static InternalUser GetInternalUserFromSession(HttpSessionState session) {
            if (session == null) { return null; }
            return session["internalUser"] as InternalUser;
        }

        public static int GetUserId(User user, InternalUser internalUser) {
            if (internalUser != null) {
                return (int)internalUser.Id;
            }
            return (int)user.Id;
        }
    }
}