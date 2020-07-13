using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public enum LoggingAction {
        Login = 1,
        Logout,
        LoadTracking,
        TurnOnRelay,
        TurnOffRelay,
        TurnOffSos,
        GenerateReport1,
        GenerateReport2,
        GenerateReport3,
        LoadTrackerInfo
    }

    public class WebLogging {
        public string T { get { return "weblogging"; } }

        [JsonProperty("la")]
        public LoggingAction LoggingAction { get; set; }

        [JsonProperty("at")]
        public DateTime ActionTime { get; set; }

        [JsonProperty("uid")]
        public ulong UserId { get; set; }

        [JsonProperty("un")]
        public string UserName { get; set; }
    }
}