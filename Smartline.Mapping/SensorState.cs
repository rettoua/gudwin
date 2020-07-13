using Newtonsoft.Json;
using System;

namespace Smartline.Mapping {
    public class SensorState {
        [JsonProperty("f")]
        public DateTime From { get; set; }
        [JsonProperty("t")]
        public DateTime To { get; set; }
        [JsonProperty("s")]
        public bool State { get; set; }
    }
}
