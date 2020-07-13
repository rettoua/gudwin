using System;
using System.Linq;
using Couchbase;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    /// <summary>
    /// for trackers uses increment key with name 'I_Tracker'
    /// </summary>
    public class Tracker {
        private string _color;
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("v_name")]
        public string V_Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("trackerid")]
        public int TrackerId { get; set; }
        [JsonProperty("v_trackerid")]
        public int V_TrackerId { get; set; }
        [JsonProperty("userid")]
        public int UserId { get; set; }
        [JsonProperty("oldtracker", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public bool? OldTracker { get; set; }
        [JsonProperty("r1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay1 { get; set; }
        [JsonProperty("r", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay { get; set; }
        [JsonProperty("r2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay2 { get; set; }
        [JsonProperty("s1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Sensor Sensor1 { get; set; }
        [JsonProperty("s2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Sensor Sensor2 { get; set; }
        [JsonProperty("hevos")]
        public bool HideFromEvosIntegration { get; set; }
        [JsonProperty("consumption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Consumption { get; set; }
        [JsonProperty("color")]
        public string Color {
            get { return _color ?? "0026FF"; }
            set {
                var v = value;
                if (v.Contains("34AADA")) {
                    v = v.Replace("34AADA", "0026FF");
                }
                if (!string.IsNullOrEmpty(v)) {
                    v = v.Replace("#", "");
                }
                _color = v;
            }
        }

        public string HtmlColor {
            get {
                return Color.StartsWith("#") ? Color : "#" + Color;
            }
        }

        [JsonProperty("jsontype")]
        public string JsonType {
            get { return "tracker"; }
            internal set { }
        }

        [JsonProperty("image")]
        public CarImage CarImage { get; set; }

        protected string GetOdometerDocumentId() {
            return GetOdometerDocumentId(Id);
        }

        public static string GetOdometerDocumentId(int trackerUid) {
            return string.Format("od_{0}", trackerUid);
        }

        #region change data in database

        public void Update(Tracker newTracker) {
            Name = newTracker.Name;
            Description = newTracker.Description;
            TrackerId = newTracker.TrackerId;
            HideFromEvosIntegration = newTracker.HideFromEvosIntegration;
            Color = newTracker.Color;
            V_Name = newTracker.V_Name;
            V_TrackerId = newTracker.V_TrackerId;
            Consumption = newTracker.Consumption;
            //newTracker.Relay1.Id = Relay1 != null ? Relay1.Id : 0;
            //newTracker.Relay2.Id = Relay1 != null ? Relay2.Id : 0;
            //newTracker.Sensor1.Id = Relay1 != null ? Sensor1.Id : 0;
            //newTracker.Sensor2.Id = Relay1 != null ? Sensor2.Id : 0;
            Relay1 = newTracker.Relay1;
            Relay2 = newTracker.Relay2;
            Sensor1 = newTracker.Sensor1;
            Sensor2 = newTracker.Sensor2;
            Relay = newTracker.Relay;
            CarImage = newTracker.CarImage;
            TryUpdateIds();
        }

        private void TryUpdateIds() {
            TryUpdateId(Relay1);
            TryUpdateId(Relay2);
            TryUpdateId(Sensor1);
            TryUpdateId(Sensor2);
            TryUpdateId(Relay);
        }

        private void TryUpdateId(ISensor sensor) {
            if (sensor != null && sensor.Id == 0) {
                sensor.Id = (int)Increments.GenerateSensorId();
            }
        }

        public bool SensorsChanged(Tracker newTracker) {
            return SensorChanged(Relay1, newTracker.Relay1) ||
                   SensorChanged(Relay2, newTracker.Relay2) ||
                   SensorChanged(Sensor1, newTracker.Sensor1) ||
                   SensorChanged(Sensor2, newTracker.Sensor2) ||
                   SensorChanged(Relay, newTracker.Relay);
        }

        private bool SensorChanged(ISensor sensor1, ISensor sensor2) {
            //if (sensor1 == null && sensor2 == null) {
            //    return false;
            //}

            var r1 = sensor1 != null && sensor1.Available;
            var r2 = sensor2 != null && sensor2.Available;
            return r1 == r2;
        }

        public void InitializeRelays() {
            if (Relay == null) {
                Relay = new Relay {
                    Available = true,
                    Name = "Реле"
                };
            }
            if (Relay1 == null) {
                Relay1 = new Relay {
                    Available = true,
                    Name = "Выход 1"
                };
            }
            if (Relay2 == null) {
                Relay2 = new Relay {
                    Available = true,
                    Name = "Выход 2"
                };
            }
            if (Sensor1 == null) {
                Sensor1 = new Sensor {
                    Name = "Вход 1"
                };
            }
            if (Sensor2 == null) {
                Sensor2 = new Sensor {
                    Name = "Вход 2"
                };
            }
            TryUpdateIds();
        }

        public static int GetMaxId() {
            var value =
                CouchbaseManager.Main.GetView("tracker_by_trackerid", "trackers_info").Descending(true).Stale(
                    StaleMode.False).Limit(1).FirstOrDefault();
            if (value == null) {
                return 1;
            }
            return Convert.ToInt32(value.ViewKey[0]);
        }

        #endregion

        public bool IsActive(ISensor sensor) {
            return sensor != null && sensor.Available;
        }
    }
}