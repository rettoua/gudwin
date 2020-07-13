using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class UserMeta {
        [JsonProperty("parent")]
        public string Parent { get; set; }
        [JsonProperty("created")]
        public DateTime? Created { get; set; }
    }
}
