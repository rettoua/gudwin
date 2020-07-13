using System;
using Newtonsoft.Json;

namespace Smartline.Web {
    public class LoadTrackObject {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("df")]
        public DateTime From { get; set; }
        [JsonProperty("dt")]
        public DateTime To { get; set; }
        [JsonProperty("tf")]
        public DateTime FromTimeSpan { get; set; }
        [JsonProperty("tt")]
        public DateTime ToTimeSpan { get; set; }

        public DateTime GetFromDate() {
            return From.AddHours(FromTimeSpan.Hour).AddMinutes(FromTimeSpan.Minute);
        }

        public DateTime GetToDate() {
            return To.AddHours(ToTimeSpan.Hour).AddMinutes(ToTimeSpan.Minute);
        }
    }
}