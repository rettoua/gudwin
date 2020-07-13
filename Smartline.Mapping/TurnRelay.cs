using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class TurnRelay {
        [JsonProperty("r_a")]
        public RequiredActionEnum RequiredAction { get; set; }

        [JsonProperty("u_id")]
        public ulong UserId { get; set; }

        [JsonProperty("t_id")]
        public int TrackerId { get; set; }

        [JsonProperty("r_id")]
        public int RelayId { get; set; }
    }

    public class DatabaseTurnRelay {
        public string DocumentId { get; set; }

        public TurnRelay TurnRelay { get; set; }
    }

    public enum RequiredActionEnum {
        On,
        Off,
        Alarming
    }
}