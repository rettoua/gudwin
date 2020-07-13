using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class EvosIntegration {
        [JsonProperty("a")]
        public bool Available { get; set; }
        [JsonProperty("l")]
        public string Login { get; set; }
        [JsonProperty("p")]
        public string Password { get; set; }
    }
}
