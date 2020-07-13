using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {

    public class SensorAlarm {
        [JsonProperty("s1")]
        public DateTime? Sensor1Alarm { get; set; }
        [JsonProperty("s2")]
        public DateTime? Sensor2Alarm { get; set; }
        [JsonProperty("sos1")]
        public bool Sensor1Sos { get; set; }
        [JsonProperty("sos2")]
        public bool Sensor2Sos { get; set; }
    }
}