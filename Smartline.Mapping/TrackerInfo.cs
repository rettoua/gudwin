using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class TrackerInfo {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("trackerid")]
        public int TrackerId { get; set; }
        [JsonProperty("addtime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? AddTime { get; set; }
        [JsonProperty("addedby")]
        public string AddedBy { get; set; }
        [JsonProperty("ip")]
        public string IP { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("userid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ulong UserId { get; set; }
        [JsonProperty("owner")]
        public string Owner { get; set; }
        [JsonProperty("applytime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ApplyTime { get; set; }
        [JsonProperty("oldtracker", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public bool? OldTracker { get; set; }
        [JsonProperty("jsontype")]
        public string JsonType {
            get { return "trackerinfo"; }
            internal set { }
        }
    }
}
