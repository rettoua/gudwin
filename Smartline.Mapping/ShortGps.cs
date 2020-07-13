using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ShortGps {
        [JsonProperty("a")]//send_time
        public DateTime SendTime { get; set; }
        [JsonProperty("b")]//latitude
        public decimal Latitude { get; set; }
        [JsonProperty("c")]//longitude
        public decimal Longitude { get; set; }
        [JsonProperty("d")]//speed
        public decimal Speed { get; set; }
        [JsonProperty("f", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndTime { get; set; }
        [JsonProperty("g", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string GeoLocation { get; set; }
        [JsonProperty("h", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int? Distance { get; set; }
    }
}