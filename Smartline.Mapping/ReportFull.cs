using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ReportFull : IReportItem {
        public ReportFull() {
            Commons = new List<ReportCommon>();
        }

        [JsonProperty("a")]
        public int TrackerId { get; set; }
        [JsonProperty("g")]
        public DateTime Date { get; set; }
        [JsonProperty("b")]
        public decimal AvgSpeed { get; set; }
        [JsonProperty("c")]
        public decimal MaxSpeed { get; set; }
        [JsonProperty("d")]
        public int? Distance { get; set; }
        [JsonProperty("e")]
        public int Parking { get; set; }
        [JsonProperty("f")]
        public int Moving { get; set; }
        [JsonProperty("h", IsReference = true)]
        public List<ReportCommon> Commons { get; set; }
        [JsonProperty("l", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastDate { get; set; }
        [JsonProperty("lat")]
        public decimal Latitude { get; set; }
        [JsonProperty("long")]
        public decimal Longitude { get; set; }

        public ReportFull Clone() {
            return (ReportFull)MemberwiseClone();
        }
    }

    public class ReportWithoutHistory {
        public ReportWithoutHistory() {
            Commons = new List<ReportCommonWithoutHistory>();
        }

        [JsonProperty("a")]
        public int TrackerId { get; set; }
        [JsonProperty("g")]
        public DateTime Date { get; set; }
        [JsonProperty("b")]
        public decimal AvgSpeed { get; set; }
        [JsonProperty("c")]
        public decimal MaxSpeed { get; set; }
        [JsonProperty("d")]
        public int? Distance { get; set; }
        [JsonProperty("e")]
        public int Parking { get; set; }
        [JsonProperty("f")]
        public int Moving { get; set; }
        [JsonProperty("h", IsReference = true)]
        public List<ReportCommonWithoutHistory> Commons { get; set; }

    }
}