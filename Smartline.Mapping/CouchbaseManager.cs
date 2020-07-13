using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Couchbase;
using Couchbase.Configuration;
using Couchbase.Extensions;
using Enyim.Caching.Memcached;
//using Couchbase.Extensions;
//using Couchbase.Extensions;
//using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public static class CouchbaseManager {
        private static readonly CouchbaseClient _main;
        private static readonly CouchbaseClient _gps;
        private static readonly CarsRepository _carsRepository;
        private static readonly CouchbaseClient _online;
        private static readonly CouchbaseClient _monitoring;
        private static readonly CouchbaseClient _accounting;

        static CouchbaseManager() {
            try {
                List<Uri> uris = GetUris();
                _main = new CouchbaseClient(CreateClientConfiguration(uris, "main", "main"));
                _gps = new CouchbaseClient(CreateClientConfiguration(uris, "gps", "gps"));
                _online = new CouchbaseClient(CreateClientConfiguration(uris, "online", "online"));
                _monitoring = new CouchbaseClient(CreateClientConfiguration(uris, "monitoring", "monitoring"));
                //_accounting = new CouchbaseClient(CreateClientConfiguration(uris, "accounting", "accounting"));

                _carsRepository = new CarsRepository();
            } catch (Exception exception) {
                //Logger.Write(exception);
                throw exception;
            }
        }

        public static CouchbaseClient Main { get { return _main; } }
        public static CouchbaseClient Gps { get { return _gps; } }
        public static CarsRepository CarsRepository { get { return _carsRepository; } }
        public static CouchbaseClient Online { get { return _online; } }
        public static CouchbaseClient Monitoring { get { return _monitoring; } }
        public static CouchbaseClient Accounting { get { return _accounting; } }

        private static List<Uri> GetUris() {
            var settings = ServerSettings.Get();
            return settings.DataBaseIps.Select(ConvertStringToUri).ToList();
        }

        private static Uri ConvertStringToUri(string ip) {
            return new Uri(string.Format("http://{0}:8091/pools", ip));
        }

        private static CouchbaseClientConfiguration CreateClientConfiguration(List<Uri> uris, string bucket, string bucketPassword) {
            var couchbaseClientConfiguration = new CouchbaseClientConfiguration();
            uris.ForEach(couchbaseClientConfiguration.Urls.Add);
            couchbaseClientConfiguration.Bucket = bucket;
            couchbaseClientConfiguration.BucketPassword = bucketPassword;
            return couchbaseClientConfiguration;
        }

        public static void RemoveUser(User user) {
            Main.Remove(user.Id + "");
        }

        public static bool SetUser(User user) {
            return Main.StoreJson(StoreMode.Set, user.Id + "", user);
        }

        public static bool SetUser(User user, int validDays) {
            return Main.Store(StoreMode.Set, user.Id + "", JSON.Serialize(user), new TimeSpan(validDays, 0, 0, 0));
        }

        public static User AuthenticateUser(string userName, string secret) {
            if (Main == null)
            {
                throw new NullReferenceException();
            }
            var user = Main.GetView<User>("users", "users").Stale(StaleMode.UpdateAfter).Key(new object[] { userName, secret }).FirstOrDefault();
            return user;
        }

        public static User AuthenticateEvosIntegrationUser(string userName, string secret) {
            var user = Main.GetView<User>("users", "evos_integration_users", true).Stale(StaleMode.UpdateAfter).Key(new object[] { userName, secret }).FirstOrDefault();
            return user;
        }

        public static List<User> AuthenticateEvosIntegrationUsers(string userName, string secret) {
            var users = Main.GetView<User>("users", "evos_integration_users", true).Stale(StaleMode.UpdateAfter).Key(new object[] { userName, secret }).ToList();
            return users;
        }

        public static List<User> GetUsersOld() {
            var users = Main.GetView<User>("users", "users").Stale(StaleMode.False).ToList();
            return users;
        }

        public static User GetUser(string id) {
            return GetSingleValueFromMain<User>(id);
        }

        public static User GetUser(int id) {
            return GetUser(id + "");
        }

        public static List<User> GetUsersByUserName(string userName) {
            List<User> users = Main.GetView<User>("users", "users", true).Stale(StaleMode.False)
                .StartKey(new object[] { userName, "\u0000" })
                .EndKey(new object[] { userName, "\u0fff" }).ToList();
            return users;
        }

        public static List<U> GetUsers<U>() {
            var users = Main.GetView<U>("users", "all_users", true).Stale(StaleMode.UpdateAfter).ToList();
            return users;
        }

        public static List<User> GetUsers(string owner) {
            var users = Main.GetView<User>("users", "users_by_owner", true).Key(owner).Stale(StaleMode.False).ToList();
            return users;
        }

        public static User AuthenticateUser(ulong id) {
            return GetUser(id + "");
            //var user = Main.GetView<User>("users", "all_users", true).Stale(StaleMode.False).StartKey(new object[] { id, "\u0000" }).EndKey(new object[] { id, "\u0fff" }).FirstOrDefault();
            //return user;
        }

        public static List<Gp> LoadOnlinePoint(int trackerId, DateTime dateTime) {
            var result = Gps.GetView<Gp>("gps", "gps", true).StartKey(new object[]
                                                                                 { trackerId,
                                                                                     dateTime                                                                                     
                                                                                 }).EndKey(new object[]{
                                                                                     trackerId,
                                                                                     "\u0fff"}).WithInclusiveEnd(true).ToList();
            return result;
        }

        public static Gp LoadOnlinePoint(int trackerId) {
            return GetSingleValueFromOnline<Gp>(trackerId + "");
        }

        public static IDictionary<string, object> LoadOnlinePoints(string[] trackerIds) {
            var value = Online.Get(trackerIds);

            return value;
        }

        public static List<T> LoadTrack<T>(int trackerId, DateTime from, DateTime to) {

            try {
                List<T> result = Gps.GetView<T>("gps", "gps", true).Stale(StaleMode.UpdateAfter).StartKey(new object[]
                                                                                 { trackerId,
                                                                                     from
                                                                                     
                                                                                 }).EndKey(new object[]{
                                                                                     trackerId,
                                                                                 to}).ToList();
                return result;
            } catch (Exception exception) {
                return null;
            }
        }

        public static bool IsPackageExists(int trackerId, DateTime from, DateTime to) {
            try {
                IView<IViewRow> gps = Gps.GetView("gps", "gps").Stale(StaleMode.UpdateAfter).StartKey(new object[]
                                                                                 { trackerId,
                                                                                     from
                                                                                 }).EndKey(new object[]{
                                                                                     trackerId,
                                                                                     to}).Limit(1);
                return gps.Any();
            } catch {
                return false;
            }
        }

        public static Gp GetLastGp(int id, StaleMode staleMode = StaleMode.False) {
            Gp onlinePoint = LoadOnlinePoint(id);
            if (onlinePoint != null) {
                return onlinePoint;
            }
            Gp lastState = Gps.GetView<Gp>("gps", "gps", true).Stale(staleMode).StartKey(new object[]
                                                                                 { id,
                                                                                     "\u0fff"
                                                                                 }).EndKey(new object[]{
                                                                                     id,
                                                                                     "\u0000"}).Descending(true).Limit(1).FirstOrDefault();
            return lastState;
        }

        public static Gp GetFirstGp(int id, StaleMode staleMode = StaleMode.False) {
            try {
                Gp lastState = Gps.GetView<Gp>("gps", "gps", true).Stale(staleMode).StartKey(new object[]
                                                                                 { id,
                                                                                     "\u0000"
                                                                                 }).EndKey(new object[]{
                                                                                     id,
                                                                                     "\u0fff"}).Limit(1).FirstOrDefault();
                return lastState;
            } catch {
                return null;
            }
        }

        public static List<TrackerInfo> GetTrackersInfo() {
            List<TrackerInfo> result = Main.GetView<TrackerInfo>("tracker_by_trackerid", "trackers_info", true)
                .Stale(StaleMode.UpdateAfter)                
                .ToList();
            return result;
        }

        public static object GetTrackersInfo(int skip, int take) {
            IView<TrackerInfo> result = Main.GetView<TrackerInfo>("tracker_by_trackerid", "trackers_info", true)
                .Stale(StaleMode.UpdateAfter)
                .Skip(skip)
                .Limit(take);
            return new { data = result.ToList(), total = result.TotalRows };
        }

        public static IViewRow GetTrackerInfo(int id) {
            var result =
                Main.GetView("tracker_by_trackerid", "trackers_info").Stale(StaleMode.False).Key(id).ToList();
            if (result.Count > 0) {
                var trackerInfo = result[0];
                return trackerInfo;
            }
            return null;
        }

        public static T GetTracker<T>(int id) where T : class {
            try {
                return Main.GetView<T>("tracker_by_trackerid", "tracker_by_trackerid").Key(id).Stale(StaleMode.UpdateAfter).Limit(1).FirstOrDefault();
            } catch (Exception) {
                return null;
            }
        }

        public static IUserTracker GetUserTracker(int trackerId) {
            try {
                IViewRow result = Main.GetView("tracker_by_trackerid", "tracker_by_trackerid").Key(trackerId).Stale(StaleMode.UpdateAfter).Limit(1).FirstOrDefault();
                if (result != null) {
                    var user = JSON.Deserialize<User>(result.GetItem() + "");
                    return new UserTracker(user, trackerId);
                }
                return null;
            } catch (Exception) {
                return null;
            }
        }

        public static List<Tracker> GetTrackers(int[] trackers) {
            return Main.GetView<Tracker>("tracker_by_trackerid", "tracker_by_id").Keys(trackers).Stale(StaleMode.False).ToList();
        }

        public static User GetUserNameOfTracker(int id) {
            var iViewRow = Main.GetView("tracker_by_trackerid", "tracker_by_trackerid").Key(id).Stale(StaleMode.UpdateAfter).Limit(1).FirstOrDefault();
            if (iViewRow == null) { return null; }
            var value = Main.Get<string>(iViewRow.ItemId);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<User>(value);
            return lastState;
        }

        public static List<T> GetAllTrackers<T>() {
            return Main.GetView<T>("tracker_by_trackerid", "tracker_by_trackerid").Stale(StaleMode.False).ToList();
        }

        public static List<User> GetAdminUsers() {
            return Main.GetView<User>("users", "admin_users", true).Stale(StaleMode.False).ToList();
        }

        public static bool AddTrackerInfo(TrackerInfo trackerInfo) {
            //return Main.StoreJson(StoreMode.Set, Guid.NewGuid().ToString(), trackerInfo);        
            var id = Guid.NewGuid().ToString();
            var result = Main.ExecuteStore(StoreMode.Set, id, JSON.Serialize(trackerInfo));
            if (!result.Success) {
                if (result.Exception != null) {
                    throw result.Exception;
                }
                throw new Exception(result.Message);
            }
            //throw new Exception(id);
            return true;
        }

        public static bool ExistTrackerId(int trackerId) {
            var result = Main.GetView("tracker_by_trackerid", "trackers_id").Key(trackerId).Stale(StaleMode.False).ToList();
            return result.Count > 0;
        }

        public static void RemoveTrackerInfo(int id) {
            var result = Main.GetView("tracker_by_trackerid", "trackers_info").Key(id).Stale(StaleMode.False).ToList();
            if (result.Count > 0) {
                var trackerInfo = result[0];
                Main.Remove(trackerInfo.ItemId);
            }
        }

        public static IViewRow GetTrackerViewRow(int trackerId) {
            return Main.GetView("tracker_by_trackerid", "tracker_by_trackerid").Key(trackerId).Stale(StaleMode.False).FirstOrDefault();
        }

        public static List<TrackerInfo> GetAvailableTrackerInfoByAdmin(string userName) {
            var result = Main.GetView<TrackerInfo>("tracker_by_trackerid", "available_trackers_info_by_admin", true).Stale(StaleMode.False).Key(userName).ToList();
            return result;
        }

        public static bool SetTrackerInfo(string id, TrackerInfo trackerInfo) {
            return Main.StoreJson(StoreMode.Set, id, trackerInfo);
        }

        public static UserSettings GetUserSettings(int userId) {
            var result = Main.Get<string>(UserSettings.GetUserSettingsId(userId));
            if (string.IsNullOrWhiteSpace(result)) {
                return null;
            }
            return JSON.Deserialize<UserSettings>(result);
        }

        public static UserSettings GetUserSettings(User user) {
            return GetUserSettings((int)user.Id);
        }

        public static void UpdateUserSettings(User user, UserSettings settings) {
            Main.StoreJson(StoreMode.Set, UserSettings.GetUserSettingsId(user), settings);
        }

        public static IViewRow GetUserViewRow(ulong id) {
            var viewRow = Main.GetView("users", "all_users").Stale(StaleMode.False).StartKey(new object[] { id, "\u0000" }).EndKey(new object[] { id, "\u0fff" }).FirstOrDefault();
            return viewRow;
        }

        public static bool UpdateUserWithCas(User user, Action<User, User> assign) {
            while (true) {
                CasResult<object> userWithCas = Main.GetWithCas(user.Id.ToString(CultureInfo.InvariantCulture));
                var userFromDb = JSON.Deserialize<User>(userWithCas.Result.ToString());
                if (userFromDb == null) {
                    return false;
                }
                assign(user, userFromDb);
                if (Main.Cas(StoreMode.Set, user.Id.ToString(CultureInfo.InvariantCulture), JSON.Serialize(userFromDb), userWithCas.Cas).Result) {
                    return true;
                }
                Thread.Sleep(50);
            }
        }

        public static string GetStateMonitor() {
            return Online.Get(Common.Runtime.Common.CommunicationStateManagerCollectionId) + "";
        }

        public static List<JsonObject> LoadMonitoring(DateTime dateTime) {
            List<JsonObject> result = Monitoring.GetView<JsonObject>("monitoring", "connection", true).StartKey(new object[] { dateTime, "\u0000" }).ToList();
            return result;
        }

        public static JsonObject LoadOnlineActivity() {
            var result = Online.Get<string>(ActualServerState.Id);
            if (string.IsNullOrWhiteSpace(result)) {
                return new JsonObject();
            }
            return JSON.Deserialize<JsonObject>(result);
        }

        public static Traffic GetTraffic(string id) {
            var result = Monitoring.Get<string>(id);
            if (string.IsNullOrWhiteSpace(result)) {
                return null;
            }
            try {
                return JSON.Deserialize<Traffic>(result);
            } catch (Exception) {
                return null;
            }
        }

        public static void SetTraffic(Traffic traffic) {
            Monitoring.Store(StoreMode.Set, traffic.Id, JSON.Serialize(traffic), new TimeSpan(30, 0, 0));
        }

        public static void SaveOnlineTrackers(int[] trackers) {
            Online.Store(StoreMode.Set, TrackersCollection.Id, JSON.Serialize(trackers), new TimeSpan(0, 15, 0));
        }

        public static int[] GetOnlineTrackers() {
            var result = Online.Get<string>(TrackersCollection.Id);
            if (string.IsNullOrWhiteSpace(result)) { return new int[0]; }
            return JSON.Deserialize<int[]>(result);
        }

        public static List<Traffic> LoadTraffic(int trackerId, DateTime from, DateTime to) {
            var result = Monitoring.GetView<Traffic>("monitoring", "traffic", true)
                      .StartKey(new object[] { trackerId, new DateTime(from.Date.Ticks) })
                      .Stale(StaleMode.UpdateAfter)
                      .EndKey(new object[] { trackerId, new DateTime(to.Date.Ticks) }).ToList();
            return result;
        }

        public static List<Traffic> LoadTraffic(List<string> ids) {
            IDictionary<string, object> result = Monitoring.Get(ids);
            return result.Values.Select(o => JSON.Deserialize<Traffic>(o + "")).ToList();
        }

        public static GpsDay GetGpsDay(string id) {
            try {
                var result = Gps.Get<string>(id);
                if (string.IsNullOrWhiteSpace(result)) { return null; }
                return JSON.Deserialize<GpsDay>(result);
            } catch {
                return null;
            }
        }

        public static SensorsDay GetSensorsDay(string id) {
            var day = GetSingleValueFromGps<SensorsDay>(id);
            return day;
        }

        public static List<GpsDay> GetGpsDays(IEnumerable<string> ids) {
            IDictionary<string, object> result = Gps.Get(ids);
            var list = new List<GpsDay>();
            try {
                list.AddRange(result.Select(keyValuePair => JSON.Deserialize<GpsDay>(keyValuePair.Value.ToString())));
            } catch { return list; }
            return list;
        }

        public static T GetSingleValueFromMonitoring<T>(string id) where T : class {
            var value = Monitoring.Get<string>(id);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<T>(value);
            return lastState;
        }

        public static T GetSingleValueFromMain<T>(string id) where T : class {
            var value = Main.Get<string>(id);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<T>(value);
            return lastState;
        }

        public static T GetSingleValueFromOnline<T>(string id) where T : class {
            var value = Online.Get<string>(id);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<T>(value);
            return lastState;
        }

        public static T GetSingleValueFromGps<T>(string id) where T : class {
            var value = Gps.Get<string>(id);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<T>(value);
            return lastState;
        }

        public static Dictionary<string, T> GetMultipleValuesFromOnline<T>(IEnumerable<string> ids) where T : class {
            try {
                Dictionary<string, T> resultFromDb = Online.Get(ids).ToDictionary(o => o.Key, o => JSON.Deserialize<T>(o.Value + ""));
                return resultFromDb;
            } catch (Exception exception) {
                return new Dictionary<string, T>();
            }
        }

        public static T GetSingleValueFromAcounting<T>(string id) where T : class {
            var value = Accounting.Get<string>(id);
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            var lastState = JSON.Deserialize<T>(value);
            return lastState;
        }

        public static void SaveOdometer(string id, Odometer odometer) {
            Monitoring.StoreJson(StoreMode.Set, id, odometer);
        }

        public static SensorAlarm GetSensorAlarm(string id) {
            return GetSingleValueFromOnline<SensorAlarm>(id);
        }

        public static LastGpsDayInfo GetLastGpsDayInfo(string id) {
            return GetSingleValueFromOnline<LastGpsDayInfo>(id);
        }

        public static void SaveToOnlineBucket(StoreMode mode, string id, object value, TimeSpan span) {
            try {
                Online.Store(mode, id, value, span);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void SaveToOnlineBucket(StoreMode mode, string id, object value) {
            try {
                Online.Store(mode, id, value);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void SaveToGpsBucket(StoreMode mode, string id, object value, TimeSpan span) {
            try {
                Gps.Store(mode, id, value, span);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void SaveToGpsBucket(StoreMode mode, string id, object value) {
            try {
                Gps.Store(mode, id, value);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void SaveToAccountingBucket(StoreMode mode, string id, object value) {
            try {
                Accounting.Store(mode, id, value);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static bool AccountingDocumentExist(string id) {
            object result = Accounting.Get(id);
            return result != null;
        }

        public static List<PaymentConfirmation> GetPaymentTransactions(TransactionState state) {
            List<PaymentConfirmation> result = Accounting.GetView<PaymentConfirmation>("views", "payment_transactions", true)
                .StartKey(new object[] { state.ToString() }).EndKey(new object[] { state.ToString(), "\u0fff", "\u0fff" })
                .Stale(StaleMode.False).ToList();
            return result;
        }

        public static List<WriteOff> GetWriteOffTransactions(TransactionState state) {
            List<WriteOff> result = Accounting.GetView<WriteOff>("views", "writeoff_transactions", true)
                .StartKey(new object[] { state.ToString() }).EndKey(new object[] { state.ToString(), "\u0fff", "\u0fff" })
                .Stale(StaleMode.False).ToList();
            return result;
        }

        public static List<T> GetDoneTransation<T>(int userId, DateTime from, DateTime to) {
            List<T> result = Accounting.GetView<T>("views", "payment_transactions", true)
               .StartKey(new object[] { TransactionState.Done.ToString(), userId, from })
               .EndKey(new object[] { TransactionState.Done.ToString(), userId, to })
               .Stale(StaleMode.UpdateAfter).ToList();
            return result;
        }

        public static List<T> GetDoneWriteOffs<T>(int userId, DateTime from, DateTime to) {
            List<T> result = Accounting.GetView<T>("views", "writeoff_transactions", true)
               .StartKey(new object[] { TransactionState.Done.ToString(), userId, from })
               .EndKey(new object[] { TransactionState.Done.ToString(), userId, to })
               .Stale(StaleMode.UpdateAfter).ToList();
            return result;
        }

        public static List<DatabaseTurnRelay> GetRequiredRelayActions() {
            var requiredRelayActions = new List<DatabaseTurnRelay>();
            try {
                List<IViewRow> values = Monitoring.GetView("monitoring", "turn_relay").Stale(StaleMode.UpdateAfter).ToList();
                foreach (IViewRow viewRow in values) {
                    var relayAction = new DatabaseTurnRelay {
                        DocumentId = viewRow.ItemId,
                        TurnRelay = JSON.Deserialize<TurnRelay>(viewRow.GetItem() + "")
                    };
                    requiredRelayActions.Add(relayAction);
                }
                return requiredRelayActions;
            } catch {
                return requiredRelayActions;
            }
        }

        public static void AddRelayAction(TurnRelay turnRelay) {
            try {
                Monitoring.Store(StoreMode.Add, Guid.NewGuid().ToString(), JSON.Serialize(turnRelay), new TimeSpan(0, 2, 0, 0));
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void RemoveRelayAction(string id) {
            try {
                Monitoring.Remove(id);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static bool SaveAccount(Account account) {
            try {
                return Accounting.Store(StoreMode.Set, account.GetId(), JSON.Serialize(account));
            } catch (Exception exception) {
                Logger.Write(exception);
                return false;
            }
        }

        public static bool SaveWriteOff(WriteOff writeOff) {
            try {
                return Accounting.Store(StoreMode.Set, writeOff.GetId(), JSON.Serialize(writeOff));
            } catch (Exception exception) {
                Logger.Write(exception);
                return false;
            }
        }

        public static List<Account> GetAccounts() {
            try {
                return Accounting.GetView<Account>("views", "accounts", true).Stale(StaleMode.False).ToList();
            } catch (Exception exception) {
                Logger.Write(exception);
                return null;
            }
        }

        public static void SaveToMonitoringBucket(StoreMode mode, string id, object value) {
            try {
                Monitoring.Store(mode, id, JSON.Serialize(value));
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static List<WebLogging> GetWebLoggings(string userId, DateTime fromTime, DateTime toTime) {
            try {
                List<WebLogging> result = Monitoring.GetView<WebLogging>("monitoring", "weblogging", true)
                        .StartKey(new object[] { userId, fromTime })
                        .EndKey(new object[] { userId, toTime })
                        .Stale(StaleMode.UpdateAfter)
                        .ToList();
                return result;
            } catch (Exception) {
                return null;
            }
        }

        public static List<WebLogging> GetWebLoggings() {
            try {
                List<WebLogging> result = Monitoring.GetView<WebLogging>("monitoring", "weblogging", true)
                        .Stale(StaleMode.UpdateAfter)
                        .ToList();
                return result;
            } catch (Exception) {
                return null;
            }
        }
    }
}