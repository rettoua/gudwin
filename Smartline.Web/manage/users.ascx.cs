using System;
using System.Collections.Generic;
using System.Linq;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class users : System.Web.UI.UserControl {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                Download.AddJScriptDynamicaly("admin", Page);
                Download.AddJScriptDynamicaly("HeaderFilter", Page);
                var user = (User)Session["User"];
                LoadUsers(user);
                SetPermissions(user);
                LoadAvailableTrackers(user);
                //gridUsers.Plugins.Add(new HeaderFilter());
            }
        }

        private void LoadAvailableTrackers(User user) {
            if (user.UserName != "Administrator") {
                List<TrackerInfo> source = CouchbaseManager.GetAvailableTrackerInfoByAdmin(user.UserName).OrderBy(o => o.TrackerId).ToList();
                StoreTrackers.DataSource = source;
                StoreTrackers.DataBind();
                txtIsAdmin.Checked = false;
            } else {
                txtIsAdmin.Checked = true;
            }
        }

        private void SetPermissions(User user) {
            if (user != null && user.UserName != "Administrator") {
                ColumnIsAdministrator.Hide();
                txtIsAdmin.Hide();
            } else if (user.UserName == "Administrator") {
                gridUserTrackers.Hide();
            }
        }

        private void LoadUsers(User user) {
            var users = ManageHelper.LoadDependedUsers(user);
            storeUsers.DataSource = users;
            storeUsers.DataBind();
            //storeAvailableUser.DataSource = users;
            //storeAvailableUser.DataBind();
        }

        [DirectMethod]
        protected void btnSaveClick(object sender, DirectEventArgs e) {
            var user = JSON.Deserialize<User>(e.ExtraParams["user"]);
            var isNew = Convert.ToBoolean(e.ExtraParams["new"]);
            User actualUser;
            if (isNew) {
                user.Secret = User.ComputeSecretNew(user.Secret);
                if (CouchbaseManager.AuthenticateUser(user.UserName, user.Secret) != null) {
                    X.Msg.Alert("Дубликат логина/пароля", "Введите другой пароль").Show();
                    return;
                }
                user.Id = Increments.GenerateUserId();
                actualUser = user;
            } else {
                actualUser = CouchbaseManager.AuthenticateUser(user.Id);
                if (!string.IsNullOrEmpty(user.Secret)) {
                    user.Secret = User.ComputeSecretNew(user.Secret);
                } else {
                    if (!NewPasswordCreated(actualUser.UserName, actualUser.Secret)) {
                        X.Msg.Alert("Пароль", "Для обновления аккаунта необходимо ввести пароль").Show();
                        return;
                    }
                }
            }
            var currentUser = (User)Session["User"];
            actualUser.Name = user.Name;
            if (!string.IsNullOrEmpty(user.Secret)) { actualUser.Secret = user.Secret; }
            actualUser.IsAdmin = user.IsAdmin;
            actualUser.IsBlocked = user.IsBlocked;
            actualUser.Reason = user.Reason;
            actualUser.Owner = currentUser.UserName;
            actualUser.UserName = user.UserName;

            CouchbaseManager.SetUser(actualUser);

            X.Call("clearUserControlData");
            X.Msg.Notify("Сохранение", "Данные успешно сохранены").Show();
            LoadUsers(CouchbaseManager.GetUser(currentUser.Id + ""));
        }

        private static bool NewPasswordCreated(string userName, string password) {
            return CouchbaseManager.AuthenticateUser(userName, password) != null;
        }

        [DirectMethod]
        protected void btnAddTrackerToUser_click(object sender, DirectEventArgs e) {
            var user = CouchbaseManager.AuthenticateUser(Convert.ToUInt32(hdnId.Value));
            var trackers = cmbInfoTrackers.SelectedItems.Select(item => item.Value).ToList();
            for (int i = 0; i < trackers.Count(); i++) {
                var iViewRow = CouchbaseManager.GetTrackerInfo(Convert.ToInt32(trackers[i]));
                var trackerInfo = JSON.Deserialize<TrackerInfo>(iViewRow.GetItem() + "");
                var newTracker = new Tracker {
                    Id = trackerInfo.Id,
                    Name = "Авто " + trackerInfo.TrackerId,
                    Description = "Описание " + trackerInfo.TrackerId,
                    TrackerId = trackerInfo.TrackerId,
                    OldTracker = trackerInfo.OldTracker
                };
                user.Trackers.Add(newTracker);
                if (CouchbaseManager.SetUser(user)) {
                    trackerInfo.User = user.UserName;
                    trackerInfo.UserId = user.Id;
                    trackerInfo.ApplyTime = DateTime.Now;
                    CouchbaseManager.SetTrackerInfo(iViewRow.ItemId, trackerInfo);
                }
            }
            windowTracker.Hide();
            LoadAvailableTrackers((User)Session["User"]);
            X.AddScript("loadTrackerByUser(App.gridUsers, App.gridUserTrackers);");
            X.Msg.Notify("Добавление терекера(-ов)", "Трекер(-ы) успешно добавлены пользователю").Show();
        }

        [DirectMethod]
        protected void btnDeleteUser_click(object sender, DirectEventArgs e) {
            ulong userId = Convert.ToUInt32(e.ExtraParams["id"] + "");
            var actualUser = CouchbaseManager.AuthenticateUser(userId);
            if (actualUser == null) {
                X.Msg.Alert("Удаление", "Ошибка удаления пользователя. Пользователь не найден.").Show();
            } else {
                CouchbaseManager.RemoveUser(actualUser);
                LoadUsers((User)Session["User"]);
                X.Msg.Notify("Удаление", "Пользователь успешно удален.").Show();
            }
        }
    }
}