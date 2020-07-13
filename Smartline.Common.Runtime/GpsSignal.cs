using System;
using Newtonsoft.Json;

namespace Smartline.Common.Runtime {
    public class GpsSignal {
        [JsonProperty("a")]
        public bool Active { get; set; }
        [JsonProperty("d")]
        public DateTime Date { get; set; }
    }
}