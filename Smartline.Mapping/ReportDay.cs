using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ReportDay {
        public string Id {
            get { return GetId(Date, TrackerId); }
        }

        [JsonProperty("e")]
        public int TrackerId { get; set; }
        [JsonProperty("a")]
        public DateTime Date { get; set; }
        [JsonProperty("b")]
        public decimal AvgSpeed { get; set; }
        [JsonProperty("c")]
        public decimal MaxSpeed { get; set; }
        [JsonProperty("d")]
        public int? Distance { get; set; }
        [JsonProperty("p")]
        public int Parking { get; set; }
        [JsonProperty("f")]
        public int Moving { get; set; }
        [JsonProperty("h", IsReference = true)]
        public List<ReportCommon> Commons { get; set; }
        [JsonProperty("l", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastDate { get; set; }

        public static string GetId(DateTime dateTime, int trackerUid) {
            return string.Format("{0}_{1}", dateTime.ToString("dd_MM_yyyy"), trackerUid);
        }
    }
}
