using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ActualServerState {
        public static string Id { get { return "ActualServerState"; } }
        [JsonProperty("connected_trackers")]
        public int ConnectedTrackersCount { get; set; }
        [JsonProperty("update_time")]
        public DateTime UpdateOn { get; set; }
        [JsonProperty("packages")]
        public int Packages { get; set; }
    }
}