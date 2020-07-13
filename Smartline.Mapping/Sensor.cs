using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class Sensor : ISensor {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("a")]
        public bool Available { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("sos")]
        public bool Sos { get; set; }
    }
}
