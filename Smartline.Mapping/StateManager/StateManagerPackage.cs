using System;
using Newtonsoft.Json;

namespace Smartline.Mapping.StateManager {
    public class StateManagerPackage {
        [JsonProperty("d")]
        public DateTime Date { get; set; }
        [JsonProperty("typeid")]
        public string TypeId { get; set; }
        [JsonProperty("t")]
        public int TrackerId { get; set; }
        [JsonProperty("u")]
        public string UserName { get; set; }
        [JsonProperty("i")]
        public string Ip { get; set; }
        [JsonProperty("r")]
        public string Reason { get; set; }
    }
}
