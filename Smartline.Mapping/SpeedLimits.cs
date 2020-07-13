using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class SpeedLimits {
        [JsonProperty("l1")]
        public int Limit1 { get; set; }
        [JsonProperty("l1c")]
        public string Limit1Color { get; set; }

        [JsonProperty("l2")]
        public int Limit2 { get; set; }
        [JsonProperty("l2c")]
        public string Limit2Color { get; set; }

        [JsonProperty("l3")]
        public int Limit3 { get; set; }
        [JsonProperty("l3c")]
        public string Limit3Color { get; set; }
    }
}