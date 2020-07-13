using Newtonsoft.Json;

namespace Smartline.Common.Runtime {
    public class Sensors {
        [JsonProperty("r1", NullValueHandling = NullValueHandling.Ignore)]
        public bool Relay1 { get; set; }

        [JsonProperty("r2", NullValueHandling = NullValueHandling.Ignore)]
        public bool Relay2 { get; set; }

        [JsonProperty("s1", NullValueHandling = NullValueHandling.Ignore)]
        public bool Sensor1 { get; set; }

        [JsonProperty("s2", NullValueHandling = NullValueHandling.Ignore)]
        public bool Sensor2 { get; set; }

        [JsonProperty("r", NullValueHandling = NullValueHandling.Ignore)]
        public bool Relay { get; set; }
    }
}