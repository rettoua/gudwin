using System.Collections.Generic;
using System.Linq;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    internal class ManageHelper {
        internal static List<User> LoadDependedUsers(User user) {
            List<User> users;
            if (user.UserName == "Administrator") {
                users = CouchbaseManager.GetUsers<User>().Where(o => o.IsAdmin).ToList();
                users.RemoveAll(o => o.UserName == "Administrator");
            } else {
                users = CouchbaseManager.GetUsers(user.UserName);
            }
            return users;
        }
    }
}