using System;
using Ext.Net;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class LastGpsDayInfo {
        [JsonProperty("datetime")]
        public DateTime DateTime { get; set; }

        public static string GetId(int trackerId) {
            return string.Format("{0}_LastGpsDay", trackerId);
        }

        public static string Serialize(LastGpsDayInfo gpsDay) {
            string value = JSON.Serialize(gpsDay);
            return value;
        }
    }
}