using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ReportHistory {
        [JsonProperty("s")]
        public DateTime Start { get; set; }
        [JsonProperty("e", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? End { get; set; }
        [JsonProperty("d")]
        public int? Distance { get; set; }
        [JsonProperty("g", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string GeoLocation { get; set; }
        [JsonProperty("i", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMoving { get; set; }
        [JsonProperty("b", DefaultValueHandling = DefaultValueHandling.Ignore)]//latitude
        public decimal Latitude { get; set; }
        [JsonProperty("c", DefaultValueHandling = DefaultValueHandling.Ignore)]//longitude
        public decimal Longitude { get; set; }
    }
}
