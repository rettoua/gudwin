using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Xml;
using Smartline.Mapping;

namespace GisServerService {
    [WebService(Namespace = "http://localhost:19236/WebServer/Service1.asmx/", Name = "GUDWIN GisServerService")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : WebService {

        [WebMethod(EnableSession = true)]
        public string ConnectToServer(string login, string pass, string tz) {
            return GisServiceHelper.Auth(Session, login, pass);
        }

        [WebMethod]
        public XmlNode ClientPing(string a) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            return null;
        }

        [WebMethod(EnableSession = true)]
        public string Disconnect(string a) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            FormsAuthentication.SignOut();
            if (Session != null) {
                Session.Abandon();
            }
            return "ok";
        }

        [WebMethod(Description = "Метод не реализован, даный функционал отсутствует")]
        public XmlNode GetAlarmTable(string a) {
            return null;
        }

        [WebMethod(Description = "Метод не реализован, даный функционал отсутствует")]
        public XmlNode GetRequestedAlarmTable(string from_date, string to_date) {
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Запрос всех трекеров пользователя")]
        public XmlNode GetAllObjects(string a) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            List<Tracker> trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            return GisServiceHelper.LoadTrackerPoints(trackers, false);
        }

        [WebMethod(EnableSession = true, Description = "Обновить информацию по состоянию объектов")]
        public XmlNode UpdateObjectsInfo(string a) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            List<Tracker> trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            return GisServiceHelper.LoadTrackerPoints(trackers, true);
        }

        [WebMethod(Description = "Метод не реализован, даный функционал отсутствует")]
        public XmlNode UpdateAlarms(string a) {
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Запрос маршрута передвижения. При пустом параметре sensor_id в ответе в SD передается скорость")]
        public XmlNode GetRoute(string id, string sensor_id, string from_date, string to_date) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            var trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            Tracker tracker = trackers.FirstOrDefault(o => o.TrackerId + "" == id || o.V_TrackerId + "" == id);
            if (tracker == null) {
                throw new Exception(string.Format("Трекера с ID {0} у текущего пользователя нет. Проверте правильность идентификатора трекера", id));
            }
            if (string.IsNullOrWhiteSpace(from_date) || string.IsNullOrWhiteSpace(to_date)) {
                return null;
            }
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            DateTime from;
            DateTime to;
            if (!DateTime.TryParse(from_date, culture, System.Globalization.DateTimeStyles.AssumeLocal, out from)) {
                return null;
            }
            if (!DateTime.TryParse(to_date, culture, System.Globalization.DateTimeStyles.AssumeLocal, out to)) {
                return null;
            }

            return GisServiceHelper.LoadRoute(tracker, sensor_id, from, to);
        }

        [WebMethod(EnableSession = true, Description = "Запрос состояния трекера (без показаний GSM, GPS, BAT и POW)")]
        public XmlNode GetObjInfo(string id) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            var trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            Tracker tracker = trackers.FirstOrDefault(o => o.TrackerId + "" == id || o.V_TrackerId + "" == id);
            if (tracker == null) {
                throw new Exception(
                    string.Format(
                        "Трекера с ID {0} у текущего пользователя нет. Проверте правильность идентификатора трекера",
                        id));
            }
            return GisServiceHelper.GetObjInfo(tracker);
        }

        [WebMethod]
        System.Xml.XmlNode GetAllObjInfo(string id) {
            throw new NotImplementedException("GetAllObjInfo");
        }

        [WebMethod(EnableSession = true, Description = "Запрос списка датчиков")]
        public XmlNode GetObjParams(string id) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            var trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            Tracker tracker = trackers.FirstOrDefault(o => o.TrackerId + "" == id || o.V_TrackerId + "" == id);
            if (tracker == null) {
                throw new Exception(string.Format("Трекера с ID {0} у текущего пользователя нет. Проверте правильность идентификатора трекера", id));
            }
            return GisServiceHelper.LoadObjParameters(tracker);
        }

        [WebMethod(EnableSession = true, Description = "Отчеты. Реализованы dist и parking отчеты")]
        public XmlNode GetReport(string id, string report_type, string from_date, string to_date) {
            if (!GisServiceHelper.IsAuthenticated()) {
                throw new Exception("Пользователь не авторизован");
            }
            var trackers = GisServiceHelper.GetTrackers(HttpContext.Current.User.Identity.Name);
            Tracker tracker = trackers.FirstOrDefault(o => o.TrackerId + "" == id || o.V_TrackerId + "" == id);
            if (tracker == null) {
                throw new Exception(string.Format("Трекера с ID {0} у текущего пользователя нет. Проверте правильность идентификатора трекера", id));
            }
            if (string.IsNullOrWhiteSpace(from_date) || string.IsNullOrWhiteSpace(to_date) || (report_type != "dist" && report_type != "parking")) {
                return null;
            }
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            DateTime from;
            DateTime to;
            if (!DateTime.TryParse(from_date, culture, System.Globalization.DateTimeStyles.AssumeLocal, out from)) {
                return null;
            }
            if (!DateTime.TryParse(to_date, culture, System.Globalization.DateTimeStyles.AssumeLocal, out to)) {
                return null;
            }

            return GisServiceHelper.GetReport(tracker, report_type, from, to);
        }

        [WebMethod]
        string GetLicenseTime() {
            throw new NotImplementedException("GetLicenseTime");
        }

        [WebMethod]
        string SetLicenseTime(string hexCode) {
            throw new NotImplementedException("SetLicenseTime");
        }
    }
}