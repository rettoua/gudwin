using System;
using System.Web;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web {
    public partial class settings : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                LoadJavaScriptFiles();
                SetUser();
                LoadSettings();
                if (Helper.IsDemoUser(Session)) {
                    SetDemoMode();
                }
            }
        }

        private void LoadJavaScriptFiles() {
            Download.AddJScriptDynamicaly("settings", this);
        }

        private void SetUser() {
            var user = ((User)Session["user"]);
            txtUserName.Text = user.UserName;
            txtName.Text = user.Name;
            LoadEvosIntegration(user);
        }

        private void LoadSettings() {
            UserSettings settings = CouchbaseManager.GetUserSettings(Session["user"] as User);
            if (settings != null) {
                fldPoints.SetValue(settings.PointsInPath);
                //fldWeight.SetValue(Math.Min(3, settings.Weight));
            }
        }

        private void LoadEvosIntegration(User user) {
            chkOnIntegration.Checked = user.EvosIntegration != null && user.EvosIntegration.Available;
            X.Call("changeIntegarationButtonState");
            btnSaveEvosIntegration.Disabled = true;
            if (user.EvosIntegration == null) {
                return;
            }
            txtIntegrationLogin.Text = user.EvosIntegration.Login;
        }

        private void SetDemoMode() {
            FieldSet1.Disable();
            cntSecurity.Disable();
            chkOnIntegration.ReadOnly = true;
            btnSaveEvosIntegration.Disable();

        }

        protected void btnChange_Click(object sender, DirectEventArgs e) {
            var userFromSession = Session["user"] as User;
            if (userFromSession != null) {
                string userName = userFromSession.UserName;
                string oldSecret = txtOldSecret.Text;
                string computedOldSecret = Mapping.User.ComputeSecretNew(oldSecret);
                var user = CouchbaseManager.AuthenticateUser(userName, computedOldSecret);
                if (user == null) {
                    X.Msg.Alert("Старый пароль", "Старый пароль введен неверно!").Show();
                    return;
                }
                user.Secret = Mapping.User.ComputeSecretNew(txtPassword.Text);
                user.Update();
            }
            Window1.Hide();
        }

        protected void ButtonSaveClick_user(object sender, DirectEventArgs e) {
            var user = (User)Session["user"];
            user.Name = txtName.Text;
            user.Update();
            X.Msg.Notify("Сохранение", "Данные пользователя успешно сохранены").Show();
        }

        [DirectMethod]
        public static void SaveEvosIntegration(bool available, string login, string secret) {
            var user = HttpContext.Current.Session["user"] as User;
            if (user == null) {
                throw new Exception("Пользователь не найдет. Перелогинтесь и повторите операцию");
            }
            if (user.EvosIntegration == null) {
                user.EvosIntegration = new EvosIntegration();
            }
            user.EvosIntegration.Available = available;
            user.EvosIntegration.Login = login;
            user.EvosIntegration.Password = Mapping.User.ComputeSecretNew(secret);
            if (!CouchbaseManager.UpdateUserWithCas(user, CouchbaseAssignHelper.AssingUserEvosIntergation)) {
                throw new Exception("Ошибка при сохранении. Повторите попытку");
            }
        }

        [DirectMethod]
        public static void UpdateSettingsPoints(int points) {
            var user = HttpContext.Current.Session["user"] as User;
            if (user == null) {
                return;
            }
            var settings = CouchbaseManager.GetUserSettings(user) ?? new UserSettings {
                UserId = (int)user.Id
            };
            settings.PointsInPath = points;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateSettingsStroke(int stroke) {
            var user = HttpContext.Current.Session["user"] as User;
            if (user == null) {
                return;
            }
            var settings = CouchbaseManager.GetUserSettings(user) ?? new UserSettings {
                UserId = (int)user.Id
            };
            settings.Weight = stroke;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static bool UserNameExist(string userName, int uid) {
            return Helper.UserNameExist(userName, (ulong)uid);
        }
    }
}