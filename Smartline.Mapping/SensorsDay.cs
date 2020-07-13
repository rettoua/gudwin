using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    /// <summary>
    /// history of sensors and relays changes
    /// </summary>
    public class SensorsDay {

        public string Id {
            get { return GetId(Date, TrackerId); }
        }

        [JsonProperty("a")]
        public DateTime Date {
            get { return _date.Date; }
            set { _date = value; }
        }
        private DateTime _date;

        [JsonProperty("e")]
        public int TrackerId { get; set; }

        [JsonProperty("s1")]
        public List<SensorState> Sensors1 { get; set; }

        [JsonProperty("s2")]
        public List<SensorState> Sensors2 { get; set; }

        [JsonProperty("r")]
        public List<SensorState> Relays { get; set; }

        [JsonProperty("r1")]
        public List<SensorState> Relays1 { get; set; }

        [JsonProperty("r2")]
        public List<SensorState> Relays2 { get; set; }

        public SensorsDay() {
            Sensors1 = new List<SensorState>();
            Sensors2 = new List<SensorState>();
            Relays = new List<SensorState>();
            Relays1 = new List<SensorState>();
            Relays2 = new List<SensorState>();
        }

        public static string GetId(DateTime dateTime, int trackerUid) {
            return string.Format("{0}_{1}_sen", dateTime.ToString("dd_MM_yyyy"), trackerUid);
        }

        public void Clear() {
            Sensors1.Clear();
            Sensors2.Clear();
            Relays.Clear();
            Relays1.Clear();
            Relays2.Clear();
        }
    }
}