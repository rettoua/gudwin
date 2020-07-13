using System;
using Newtonsoft.Json;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class Repository {
        public Repository() {
        }

        public Repository(int id) {
            Id = id;
        }

        public static Repository Transform(Tracker tracker, bool init = false) {
            return new Repository {
                Id = tracker.Id,
                TrackerId = tracker.TrackerId,
                Name = tracker.Name,
                Color = tracker.HtmlColor,
                Relay = tracker.Relay,
                Relay1 = tracker.Relay1,
                Relay2 = tracker.Relay2,
                Sensor1 = tracker.Sensor1,
                Sensor2 = tracker.Sensor2,
                Image = tracker.CarImage ?? CarImage.Default,
            };
        }

        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("TrackerId")]
        public int TrackerId { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Speed")]
        public decimal Speed { get; set; }
        [JsonProperty("LastSendTime")]
        public DateTime? LastSendTime { get; set; }
        [JsonProperty("EndSendTime")]
        public DateTime? EndSendTime { get; set; }
        [JsonProperty("Latitude")]
        public decimal Latitude { get; set; }
        [JsonProperty("Longidute")]
        public decimal Longitude { get; set; }
        [JsonProperty("IsTracked")]
        public bool IsTracked { get { return false; } }
        [JsonProperty("Color")]
        public string Color { get; set; }
        [JsonProperty("r1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay1 { get; set; }
        [JsonProperty("r", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay { get; set; }
        [JsonProperty("r2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Relay Relay2 { get; set; }
        [JsonProperty("s1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Sensor Sensor1 { get; set; }
        [JsonProperty("s2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Sensor Sensor2 { get; set; }
        [JsonProperty("s")]
        public Sensors s { get; set; }
        [JsonProperty("image")]
        public CarImage Image { get; set; }
        [JsonProperty("k")]
        public int? Battery { get; set; }

        public void Refresh() {
            Gp lastState = CouchbaseManager.LoadOnlinePoint(Id);
            Update(lastState);
        }

        internal void Update(Gp lastGp) {
            if (lastGp == null) {
                Speed = -1;
            } else {
                Speed = lastGp.Speed;
                LastSendTime = lastGp.SendTime;
                if (lastGp.EndTime != null) {
                    EndSendTime = lastGp.EndTime.Value;
                }
                Latitude = lastGp.Latitude;
                Longitude = lastGp.Longitude;
                s = lastGp.Sensors;
                Battery = lastGp.Battery;
            }
        }
    }
}