using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class InternalUser : IUser {
        public InternalUser() {
            TrackerUids = new List<int>();
        }

        [JsonProperty("Id")]
        public ulong Id { get; set; }
        [JsonProperty("UserName")]
        public string UserName { get; set; }
        [JsonProperty("Secret")]
        public string Secret { get; set; }
        [JsonProperty("NormalSecret")]
        public string NormalSecret { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("ParentUserId")]
        public ulong ParentUserId { get; set; }
        [JsonProperty("IsBlocked")]
        public bool IsBlocked { get; set; }
        [JsonProperty("Trackers")]
        public List<int> TrackerUids { get; set; }
    }
}