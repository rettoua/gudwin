using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using System.Xml.Serialization;
using Couchbase;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.Reports;
using XmlWriter = System.Xml.XmlWriter;

namespace GisServerService {
    public class GisServiceHelper {
        public static bool IsAuthenticated() {
            HttpContext context = HttpContext.Current;
            return context.User != null && context.User.Identity.IsAuthenticated;
        }

        public static string Auth(HttpSessionState session, string userName, string password) {
            HttpContext context = HttpContext.Current;
            if (!context.User.Identity.IsAuthenticated) {
                try {
                    List<User> users = GetUsers(userName, password);
                    if (users == null || users.Count == 0) {
                        return "Неверное имя пользователя или пароль";
                    }
                    var usersIds = string.Join(",", users.Select(user => user.Id));
                    FormsAuthentication.SetAuthCookie(usersIds, false);
                    if (session != null) {
                        session["user"] = users;
                    }
                } catch (Exception) {
                    return "Ошибка подключения. Обратитесь к администратору";
                }
            }

            return "ok";
        }

        internal static List<User> GetUsers(string userName, string password) {
            string computeSecret = User.ComputeSecretNew(password);
            var users = CouchbaseManager.AuthenticateEvosIntegrationUsers(userName, computeSecret);
            return users;
        }

        internal static User GetUser(string id) {
            User updateUser = CouchbaseManager.AuthenticateUser(Convert.ToUInt64(id));
            return updateUser;
        }

        internal static List<Tracker> GetTrackers(string integrationId) {
            var trackers = new List<Tracker>();
            var usersIds = integrationId.Split(',');
            var usersInArray = CouchbaseManager.Main.Get(usersIds);
            foreach (KeyValuePair<string, object> o in usersInArray) {
                var user = JSON.Deserialize<User>(o.Value.ToString());
                trackers.AddRange(user.Trackers.Where(tracker => !tracker.HideFromEvosIntegration));
            }
            return trackers;
        }

        internal static XmlNode LoadTrackerPoints(List<Tracker> trackers, bool onlyOnlineTrackers) {
            IDictionary<string, object> onlinePoints = CouchbaseManager.LoadOnlinePoints(trackers.Select(tracker => tracker.Id + "").ToArray());
            var trackerWithGp = new Dictionary<Tracker, Gp>();
            foreach (Tracker tracker in trackers) {
                if (onlinePoints.ContainsKey(tracker.Id + "")) {
                    var trackerGp = JSON.Deserialize<Gp>(onlinePoints[tracker.Id + ""].ToString());
                    trackerWithGp.Add(tracker, trackerGp);
                } else if (!onlyOnlineTrackers) {
                    trackerWithGp.Add(tracker, null);
                }
            }
            var listResult = (from c in trackerWithGp
                              select new ObjTree(c.Key, c.Value)
                             ).ToList();
            listResult.AddRange(from c in trackerWithGp
                                where c.Key.V_TrackerId != 0
                                select new ObjTree(c.Key, c.Value, true));
            return ObjectToXml(listResult);
        }

        internal static XmlNode LoadObjParameters(Tracker tracker) {
            var objParams = new List<ObjParameters>();
            var point = CouchbaseManager.LoadOnlinePoint(tracker.Id);
            if (tracker.Relay1 != null && tracker.Relay1.Available) {
                objParams.Add(CreateObjParameters(tracker.Relay1, point));
            }
            if (tracker.Relay2 != null && tracker.Relay2.Available) {
                objParams.Add(CreateObjParameters(tracker.Relay2, point));
            }
            if (tracker.Sensor1 != null && tracker.Sensor1.Available) {
                objParams.Add(CreateObjParameters(tracker.Sensor1, point));
            }
            if (tracker.Sensor2 != null && tracker.Sensor2.Available) {
                objParams.Add(CreateObjParameters(tracker.Sensor2, point));
            }
            return ObjectToXml(objParams.ToArray());
        }

        private static ObjParameters CreateObjParameters(ISensor sensor, Gp point) {
            return new ObjParameters {
                IMG = point == null ? "no_signal" : "norm",
                SENID = sensor.Id,
                SEN = sensor.Name,
                VAL = point == null ? "Нед данных" : (point.Sensors.Relay1 == true ? "Включен" : "Выключен")
            };
        }

        internal static XmlNode LoadRoute(Tracker tracker, string sensorId, DateTime from, DateTime to) {
            List<Gp> result = GpsDayHelper.LoadTrack(tracker.Id, from, to);
            var listOfRoutePoint = new List<RoutePoint>();
            string sensor = GetLoadRouteSensorString(tracker, sensorId);
            foreach (Gp gp in result) {
                listOfRoutePoint.Add(new RoutePoint {
                    ID = 0,
                    Lat = gp.Latitude,
                    Lon = gp.Longitude,
                    TM = gp.SendTime.ToString("dd.MM.yyyy HH:mm:ss"),
                    SD = GetLoadRouteSensor(gp, sensor)
                });
            }
            return ObjectToXml(listOfRoutePoint.ToArray());
        }

        private static string GetLoadRouteSensor(Gp gp, string sensor) {
            if (string.IsNullOrWhiteSpace(sensor)) {
                return gp.Speed.ToString(CultureInfo.InvariantCulture);
            }
            bool? activeSensor = null;
            switch (sensor) {
                case "r1":
                    activeSensor = gp.Sensors.Relay1;
                    break;
                case "r2":
                    activeSensor = gp.Sensors.Relay2;
                    break;
                case "s1":
                    activeSensor = gp.Sensors.Sensor1;
                    break;
                case "s2":
                    activeSensor = gp.Sensors.Sensor2;
                    break;
            }
            return activeSensor == null ? "Нет данных" : (activeSensor == true ? "Вкл" : "Выкл");
        }

        private static string GetLoadRouteSensorString(Tracker tracker, string sensorId) {
            if (tracker.Relay1 != null && tracker.Relay1.Id + "" == sensorId) {
                return "r1";
            }
            if (tracker.Relay2 != null && tracker.Relay2.Id + "" == sensorId) {
                return "r2";
            }
            if (tracker.Sensor1 != null && tracker.Sensor1.Id + "" == sensorId) {
                return "s1";
            }
            if (tracker.Sensor2 != null && tracker.Sensor2.Id + "" == sensorId) {
                return "s2";
            }
            return string.Empty;
        }

        internal static XmlNode GetObjInfo(Tracker tracker) {
            var lastPoint = CouchbaseManager.GetLastGp(tracker.Id);
            if (lastPoint == null) {
                return null;
            }
            var objInfoArray = new ObjInfo[1];
            objInfoArray[0] = new ObjInfo {
                LT = lastPoint.SendTime.ToString("dd.MM.yyyy hh:mm")
            };
            return ObjectToXml(objInfoArray);
        }

        internal static XmlNode GetReport(Tracker tracker, string reportType, DateTime from, DateTime to) {
            if (reportType == "dist") {
                Dictionary<int, List<ReportAdapted>> result = ReportManager.GetSummaryReportData(from.Date, to.Date, from, to, new List<int> { tracker.Id });
                var reportSet = new ReportSet();
                return reportSet.GenerateDist(result[tracker.Id]);
            } else {
                Dictionary<int, List<DetailReportItemDay>> result = ReportManager.GetDetailedReportData(from.Date, to.Date, from, to, new List<int> { tracker.Id });
                var reportSet = new ReportSet();
                return reportSet.GenerateParking(result[tracker.Id]);
            }
        }

        internal static XmlNode ObjectToXml(object obj) {
            using (var stream = new MemoryStream()) {
                using (var writer = XmlWriter.Create(stream)) {
                    new XmlSerializer(obj.GetType()).Serialize(writer, obj);
                    var ms = new MemoryStream(stream.ToArray());
                    ms.Flush();
                    ms.Position = 0;
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(ms);
                    return xmlDoc;
                }
            }
        }
    }
}