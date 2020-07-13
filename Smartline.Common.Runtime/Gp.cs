using System;
using Newtonsoft.Json;

namespace Smartline.Common.Runtime {
    public class Gp {
        [JsonProperty("a")]//send_time
        public DateTime SendTime { get; set; }
        [JsonProperty("b")]//latitude
        public decimal Latitude { get; set; }
        [JsonProperty("c")]//longitude
        public decimal Longitude { get; set; }
        [JsonProperty("d")]//speed
        public decimal Speed { get; set; }
        [JsonProperty("e")]//tracker_id
        public int TrackerId { get; set; }
        [JsonProperty("f", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndTime { get; set; }
        [JsonProperty("g", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string GeoLocation { get; set; }
        [JsonProperty("h", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int? Distance { get; set; }
        [JsonProperty("i", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public GpsSignal GpsSignal { get; set; }
        [JsonProperty("k", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int? Battery { get; set; }

        [JsonProperty("s1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int? SOS1 { get; set; }
        [JsonProperty("s2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int? SOS2 { get; set; }

        [JsonProperty("s", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public Sensors Sensors { get; set; }

        public Gp Clone() {
            return (Gp)MemberwiseClone();
        }

        public DateTime GetActualTime() {
            return EndTime ?? SendTime;
        }

        public override int GetHashCode() {
            return SendTime.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode() ^ Speed.GetHashCode() ^ Distance.GetHashCode();
        }

        public override bool Equals(object obj) {
            var other = obj as Gp;
            if (other == null) { return false; }
            return SendTime == other.SendTime
                && Latitude == other.Latitude
                && Longitude == other.Longitude
                && Speed == other.Speed
                && Distance == other.Distance;
        }

        public void Assign(Gp source) {
            if (source == null) { throw new ArgumentNullException("source"); }
            SendTime = source.SendTime;
            EndTime = source.EndTime;
            Latitude = source.Latitude;
            Longitude = source.Longitude;
            Speed = source.Speed;
            Distance = source.Distance;
            GpsSignal = source.GpsSignal;
            Battery = source.Battery;
        }
    }
}