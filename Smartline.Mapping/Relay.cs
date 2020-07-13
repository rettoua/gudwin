using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class Relay : ISensor {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("a")]
        public bool Available { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }
    }
}
