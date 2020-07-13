using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class EventCollection : List<Event> {
        public static string GetId(int userUid) {
            return string.Format("event_collection_{0}", userUid);
        }
    }

    public class Event {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("useruid")]
        public int UserId { get; set; }
        [JsonProperty("trackeruid")]
        public int TrackerUid { get; set; }
        [JsonProperty("completed")]
        public bool Completed { get; set; }
        [JsonProperty("dependon")]
        public int DependOn { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}