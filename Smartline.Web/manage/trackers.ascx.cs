using System;
using System.Collections.Generic;
using Ext.Net;
using Ext.Net.Utilities;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class trackers : System.Web.UI.UserControl {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                BindStores();
            }
        }

        private void BindStores() {
            LoadTrackers();
            LoadAdminUsers();
        }

        protected void LoadTrackers() {
            storeTrackers.DataSource = CouchbaseManager.GetTrackersInfo();
            storeTrackers.DataBind();
        }

        protected void LoadAdminUsers() {
            StoreAdminUsers.DataSource = CouchbaseManager.GetAdminUsers().ToArray();
            StoreAdminUsers.DataBind();
        }

        private int TrackersCountToCreate {
            get { return string.IsNullOrWhiteSpace(txtTrackerCount.Text) ? 1 : Convert.ToInt32(txtTrackerCount.Text); }
        }

        #region Direct methods

        [DirectMethod]
        protected void btnSaveNewTrackerInfo_click(object sender, DirectEventArgs e) {
            CreateNewTrackersInfo(TrackersCountToCreate);
        }

        [DirectMethod]
        protected void btnValidate_click(object sender, DirectEventArgs e) {
            AvailableTrackersForCreate availableTrackersForCreate = GetAvailableTrackers(Convert.ToInt32(txtTrackerId.Text), TrackersCountToCreate);
            string messageAlreadySelectedTrackers = string.Empty;
            if (availableTrackersForCreate.SelectedTrackers.Count > 0) {
                messageAlreadySelectedTrackers = string.Format("Трекеры со следующими ID уже существуют: <br /><b>{0}</b><br />", availableTrackersForCreate.SelectedTrackers.Join(", "));
            }
            string messageWithAvailableTrackersId = string.Format("Доступны следующие ID для трекеров: <br /><b>{0}</b><br />{1}Добавить трекеры с доступными ID?", availableTrackersForCreate.AvailableTrackers.Join(", "), messageAlreadySelectedTrackers);
            X.Msg.Confirm("Добавление трекера(-ов)", messageWithAvailableTrackersId, new JFunction("showResult")).Show();
        }

        [DirectMethod]
        public void RemoveTrackerInfo(string trackerInfoStr) {
            var trackerInfo = JSON.Deserialize<TrackerInfo>(trackerInfoStr);
            CouchbaseManager.RemoveTrackerInfo(trackerInfo.Id);
            var trackerViewRow = CouchbaseManager.GetTrackerViewRow(trackerInfo.TrackerId);
            if (trackerViewRow != null) {
                string userId = trackerViewRow.ItemId;
                var userStr = CouchbaseManager.Main.Get(userId);
                var user = JSON.Deserialize<User>(userStr + "");
                if (user != null) {
                    if (user.Trackers.RemoveAll(tracker => tracker.TrackerId == trackerInfo.TrackerId) > 0) {
                        user.Update();
                    }
                }
            }
            //LoadTrackers();
            X.Msg.Notify("Удаление трекера", "Трекер успешно удален").Show();
        }

        #endregion

        private void CreateNewTrackersInfo(int trackersCount) {
            int trackerId = Convert.ToInt32(txtTrackerId.Text);
            AvailableTrackersForCreate availableTrackersForCreate = GetAvailableTrackers(trackerId, TrackersCountToCreate);
            var trackerAlreadyExist = new List<int>();
            var addedBy = (User)Session["User"];
            foreach (int id in availableTrackersForCreate.AvailableTrackers) {
                var newTrackerInfo = new TrackerInfo {
                    Id = Convert.ToInt32(Increments.GetTrackerId() + 1),
                    TrackerId = id,
                    AddTime = DateTime.Now,
                    AddedBy = addedBy.UserName,
                    Owner = cmbUser.Text,
                    IP = Request.UserHostAddress,
                    OldTracker = chkOldTracker.Checked
                };
                if (CouchbaseManager.ExistTrackerId(newTrackerInfo.TrackerId)) {
                    trackerAlreadyExist.Add(newTrackerInfo.TrackerId);
                } else {
                    CouchbaseManager.AddTrackerInfo(newTrackerInfo);
                    Increments.GenerateTrackerId();
                }
            }
            LoadTrackers();
            windowTrack.Hide();
            if (trackerAlreadyExist.Count > 0) {
                X.Msg.Notify("не сохраненные трекеры", string.Join(" ;", trackerAlreadyExist)).Show();
            }
        }

        private AvailableTrackersForCreate GetAvailableTrackers(int startTrackerId, int count) {
            var availableTrackers = new List<int>();
            var selectedTrackers = new List<int>();
            for (int i = 0; i < count; i++) {
                while (CouchbaseManager.ExistTrackerId(startTrackerId)) {
                    selectedTrackers.Add(startTrackerId);
                    startTrackerId++;
                }
                availableTrackers.Add(startTrackerId++);
            }
            return new AvailableTrackersForCreate { AvailableTrackers = availableTrackers, SelectedTrackers = selectedTrackers };
        }

        [DirectMethod]
        public object BindData(string action, Dictionary<string, object> extraParams) {
            var prms = new StoreRequestParameters(extraParams);
            object data = CouchbaseManager.GetTrackersInfo(prms.Start, prms.Limit);
            return data;
        }

        private class AvailableTrackersForCreate {
            internal List<int> AvailableTrackers { get; set; }
            internal List<int> SelectedTrackers { get; set; }
        }
    }
}