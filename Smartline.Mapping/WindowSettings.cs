using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class WindowSettings {
        public WindowSettings() {
            Width = 340;
            Height = 250;
        }

        /// <summary>
        /// describes horizontal offset of left point in percentage
        /// </summary>
        [JsonProperty("x")]
        public double X { get; set; }
        /// <summary>
        /// describes vertical offset of top point in percentage
        /// </summary>
        [JsonProperty("y")]
        public double Y { get; set; }
        [JsonProperty("w")]
        public int Width { get; set; }
        [JsonProperty("h")]
        public int Height { get; set; }
        [JsonProperty("collapsed")]
        public bool Collapsed { get; set; }
    }
}