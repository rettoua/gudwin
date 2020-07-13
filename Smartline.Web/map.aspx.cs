using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using Ext.Net;
using Newtonsoft.Json;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.Relays;
using Smartline.Web.WebLogging;

namespace Smartline.Web {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class map : Page, IReadOnlySessionState {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {

                SetSignalrUrl();
                LoadSettings();                
                LoadCar();
                InitFilterField();
                LoadMaps();
                Helper.HideMask();
            }
        }

        private void SetSignalrUrl() {
            X.AddScript("signalrUrl = '{0}';", LocalIpAddress());
        }
        
        public static string LocalIpAddress() {
            string localIp = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }

        private void LoadCar() {
            Store store = gridPanelCars.GetStore();
            User user = Helper.GetUserFromSession(Session);
            InternalUser internalUser = Helper.GetInternalUserFromSession(Session);
            if (user == null) { return; }
            User userFromDb = CouchbaseManager.AuthenticateUser(user.Id);
            List<Repository> result = internalUser == null ? CouchbaseManager.CarsRepository.GetCarListForUser(userFromDb) :
                CouchbaseManager.CarsRepository.GetCarListForUser(userFromDb, internalUser);
            store.DataSource = result;
            store.DataBind();
        }

        private void InitFilterField() {
            dfFrom.SelectedDate = DateTime.Now.Date;
            dfTo.SelectedDate = DateTime.Now.Date;
        }

        private void LoadSettings() {
            User user = Helper.GetUserFromSession(Session);
            InternalUser internalUser = Helper.GetInternalUserFromSession(Session);
            UserSettings settings = CouchbaseManager.GetUserSettings(internalUser == null ? (int)user.Id : (int)internalUser.Id);
            string userSettings = settings == null ? JSON.Serialize(new UserSettings { UserId = Helper.GetUserId(user, internalUser) }) : JSON.Serialize(settings);
            X.AddScript("globalUserSettings = {0};", userSettings);
            if (settings != null) {
                chkShowTrack.Pressed = settings.ShowTracking;
                btnPan.Pressed = settings.Pan;
            }
        }

        private void LoadMaps() {            
            X.Call("main_start");
            
        }

        [DirectMethod]
        public static List<Gp> GetRepositories(IncomeRepositoryRequest[] items) {
            var values = new List<Gp>();
            try {
                Dictionary<string, Gp> onlinePoints = CouchbaseManager.GetMultipleValuesFromOnline<Gp>(items.Select(o => o.Id + ""));
                foreach (IncomeRepositoryRequest value in items) {
                    if (onlinePoints.ContainsKey(value.Id + "")) {
                        if (!value.UseHotTracking) {  
                            values.Add(onlinePoints[value.Id + ""]);
                        } else {   
                            values.AddRange(LoadHotTracking(value));
                        } 
                    }
                }
                //var realPoints = values.Where(point => point.LastPoint != null && point.LastPoint.Speed > 0).ToList();
                //if (realPoints.Count > 0) {
                //    decimal averageLatitude = realPoints.Select(o => o.LastPoint.Latitude).Average();
                //    decimal averageLongitude = realPoints.Select(o => o.LastPoint.Longitude).Average();
                //    UpdateSettingsPoint(averageLatitude, averageLongitude);
                //}
                //return JSON.Serialize(new DirectMethodReturnObject { Date = DateTime.Now, Value = values });                
            } catch {
            }
            return values;
        }

        private static IEnumerable<Gp> LoadHotTracking(IncomeRepositoryRequest tracker) {
            if (tracker.Date.HasValue) {
                tracker.Date = tracker.Date.Value.AddSeconds(-1);
                return CouchbaseManager.LoadOnlinePoint(tracker.Id, tracker.Date.Value);
            }
            var gp = CouchbaseManager.GetLastGp(tracker.Id);
            return new List<Gp> { gp };
        }

        [DirectMethod]
        public static List<Gp> LoadTrack(int tracker, DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan) {
            WebLoggingHelper.AddAction(LoggingAction.LoadTracking);
            from = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            to = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);

            List<Gp> gps = GpsDayHelper.LoadTrack(tracker, from, to);
            //GpsTrack result = TrackMaker.MakeTrack(gps);
            return gps;
        }

        [DirectMethod]
        public static GpsTrack LoadTrackNew(LoadTrackObject trackObject) {
            WebLoggingHelper.AddAction(LoggingAction.LoadTracking);
            DateTime from = trackObject.GetFromDate();
            DateTime to = trackObject.GetToDate();

            List<Gp> gps = GpsDayHelper.LoadTrack(trackObject.Id, from, to);
            GpsTrack result = TrackMaker.MakeTrack(gps);
            return result;
        }

        [DirectMethod]
        public static void UpdateSettingsShowHideSensors(bool showSensors) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            if (settings.ShowSensors == showSensors) { return; }
            settings.ShowSensors = showSensors;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateSettingsShowHideTracking(bool showTracking) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            if (settings.ShowTracking == showTracking) { return; }
            settings.ShowTracking = showTracking;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateSettingsPan(bool pan) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            settings.Pan = pan;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateWindowSize(int width, int height) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            if (settings.WindowSettings.Width == width && settings.WindowSettings.Height == height) { return; }
            settings.WindowSettings.Width = width;
            settings.WindowSettings.Height = height;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateWindowLocation(double x, double y) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            if (settings.WindowSettings.X.Equals(x) && settings.WindowSettings.Y.Equals(y)) { return; }
            settings.WindowSettings.X = x;
            settings.WindowSettings.Y = y;
            CouchbaseManager.UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void UpdateWindowExpandedState(bool collapsed) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            if (settings.WindowSettings.Collapsed == collapsed) { return; }
            settings.WindowSettings.Collapsed = collapsed;
            UpdateUserSettings(user, settings);
        }

        [DirectMethod]
        public static void SaveSpeedLimits(SpeedLimits limits) {
            User user = GetUser();
            UserSettings settings = GetUserSettings(user);
            if (settings == null) { return; }
            settings.SpeedLimits = limits;
            UpdateUserSettings(user, settings);
        }

        private static void UpdateUserSettings(User user, UserSettings userSettings) {
            CouchbaseManager.UpdateUserSettings(user, userSettings);
        }

        private static User GetUser() {
            return HttpContext.Current.Session["user"] as User;
        }

        private static UserSettings GetUserSettings(User user) {
            if (user == null) { return null; }
            var settings = CouchbaseManager.GetUserSettings(user) ?? new UserSettings {
                UserId = (int)user.Id
            };
            return settings;
        }

        [DirectMethod]
        public static List<Gp> LoadUnreachableTrackers(int[] trackerIds) {
            var gps = new List<Gp>();
            foreach (int trackerId in trackerIds) {
                Gp lastGp = GpsDayHelper.GetLastGp(trackerId);
                if (lastGp == null) { continue; }
                gps.Add(lastGp);
            }
            return gps;
        }

        [DirectMethod]
        public static Tracker GetTracker(int trackerId, int userId) {
            WebLoggingHelper.AddAction(LoggingAction.LoadTrackerInfo);
            var tracker = CouchbaseManager.GetTracker<Tracker>(trackerId);
            if (tracker.Relay == null) {
                tracker.InitializeRelays();
                return DoUpdateTracker(tracker, userId);
            }
            return tracker;
        }

        [DirectMethod]
        public static ulong GetNextTrackerId() {
            return Increments.GetTrackerId() + 1;
        }

        [DirectMethod]
        public static Tracker UpdateTracker(Tracker newTracker, int odometerValue, int userId) {
            var odometer = CouchbaseManager.GetSingleValueFromMonitoring<Odometer>(Tracker.GetOdometerDocumentId(newTracker.Id)) ??
                          new Odometer { InitialDate = DateTime.Now, TrackerUid = newTracker.Id, TrackerId = newTracker.TrackerId };
            if (odometerValue != odometer.Meters) {
                odometer.Meters = odometerValue;
                odometer.Save();
            }
            return DoUpdateTracker(newTracker, userId);
        }

        private static Tracker DoUpdateTracker(Tracker newTracker, int userId) {
            User user = CouchbaseManager.GetUser(userId + "");
            Tracker trackerFromDb = user.Trackers.FirstOrDefault(o => o.Id == newTracker.Id);
            if (trackerFromDb == null) { return null; }
            trackerFromDb.Update(newTracker);
            user.Update();
            return trackerFromDb;
        }

        [DirectMethod]
        public static Odometer LoadOdometer(int trackerUid, int trackerId) {
            var odometer = CouchbaseManager.GetSingleValueFromMonitoring<Odometer>(Tracker.GetOdometerDocumentId(trackerUid)) ??
                           new Odometer { InitialDate = DateTime.MinValue, TrackerUid = trackerUid, TrackerId = trackerId };
            odometer.Update();
            return odometer;
        }

        [DirectMethod]
        public static void TurnOnRelay(ulong userId, int trackerId, int relayId) {
            WebLoggingHelper.AddAction(LoggingAction.TurnOnRelay);
            RelayController.Instance.SaveTurnOnAction(userId, trackerId, relayId);
        }

        [DirectMethod]
        public static void TurnOffRelay(ulong userId, int trackerId, int relayId) {
            WebLoggingHelper.AddAction(LoggingAction.TurnOffRelay);
            RelayController.Instance.SaveTurnOffAction(userId, trackerId, relayId);
        }

        [DirectMethod]
        public static void TurnOffAlarmingSensor(ulong userId, int trackerId) {
            WebLoggingHelper.AddAction(LoggingAction.TurnOffSos);
            RelayController.Instance.SaveTurnOffAlarmingAction(userId, trackerId);
        }

        [DirectMethod]
        public static List<CarImage> LoadCarIcons() {
            return CarImage.Images;
        }

        #region internal classes
        internal class GpsHotTracking {
            public int Id { get; set; }
            public Gp LastPoint { get; set; }
            public List<Gp> StandardPoints { get; set; }
        }

        public class DirectMethodReturnObject {
            public DateTime Date { get; set; }
            public object Value { get; set; }
        }

        public class IncomeRepositoryRequest {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("permit")]
            public bool Permit { get; set; }
            [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? Date { get; set; }
            [JsonProperty("hottrack")]
            public bool UseHotTracking { get; set; }
        }

        #endregion
    }
}