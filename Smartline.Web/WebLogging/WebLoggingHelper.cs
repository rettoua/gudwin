using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using Enyim.Caching.Memcached;
using Smartline.Mapping;

namespace Smartline.Web.WebLogging {

    public class WebLoggingHelper {

        public static void AddAction(LoggingAction action) {
            //HttpSessionState session = HttpContext.Current.Session;
            //IUser user = (IUser)Helper.GetInternalUserFromSession(session) ?? Helper.GetUserFromSession(session);
            //if (user == null) { return; }
            //var newWebLogging = new Mapping.WebLogging() {
            //    ActionTime = DateTime.Now,
            //    LoggingAction = action,
            //    UserId = user.Id,
            //    UserName = user.UserName
            //};
            //SaveAction(newWebLogging);
        }

        private static void SaveAction(Mapping.WebLogging logging) {
            try {
                CouchbaseManager.SaveToMonitoringBucket(StoreMode.Add, Guid.NewGuid().ToString(), logging);
            } catch {
                //do nothing
            }
        }

        public static List<Mapping.WebLogging> GetWebLogging(ulong userId, DateTime from, DateTime toTime) {
            return CouchbaseManager.GetWebLoggings(userId + "", from, toTime);
        }
    }
}