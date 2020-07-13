using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class RelayAction {
        [JsonProperty("id")]
        public int TrackerId { get; set; }
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date { get; set; }
        [JsonProperty("rindex")]
        public int RelayIndex { get; set; }
        [JsonProperty("executing")]
        public bool Executing { get; set; }
        [JsonProperty("ison")]
        public bool IsOn { get; set; }
        [JsonProperty("executed")]
        public bool Executed { get; set; }
        [JsonProperty("error")]
        public string FailureMessage { get; set; }
        [JsonProperty("action")]
        public bool Action { get; set; }
    }
}
