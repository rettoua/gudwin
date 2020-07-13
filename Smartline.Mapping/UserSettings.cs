using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class UserSettings {
        public UserSettings() {
            PointsInPath = 25;
            Pan = true;
            Weight = 3;
            WindowSettings = new WindowSettings();
        }

        [JsonProperty("i")]
        public int UserId { get; set; }
        [JsonProperty("s")]
        public bool ShowSensors { get; set; }
        [JsonProperty("t")]
        public bool ShowTracking { get; set; }
        [JsonProperty("lat")]
        public decimal Latitude { get; set; }
        [JsonProperty("long")]
        public decimal Longitude { get; set; }
        [JsonProperty("p")]
        public int PointsInPath { get; set; }
        [JsonProperty("pan")]
        public bool Pan { get; set; }
        [JsonProperty("weight")]
        public int Weight { get; set; }
        [JsonProperty("wndcars")]
        public WindowSettings WindowSettings { get; set; }
        [JsonProperty("sl")]
        public SpeedLimits SpeedLimits { get; set; }

        public static string GetUserSettingsId(int userId) {
            return string.Format("user_settings_{0}", userId);
        }

        public static string GetUserSettingsId(User user) {
            return GetUserSettingsId((int)user.Id);
        }
    }
}