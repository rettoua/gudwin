using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class ReportCommon : IReportItem {
        public ReportCommon() {
            Histories = new List<ReportHistory>();
        }

        [JsonProperty("a")]
        public int Hour { get; set; }
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
        [JsonProperty("g")]
        public List<ReportHistory> Histories { get; set; }
    }

    public class ReportCommonWithoutHistory {

        [JsonProperty("a")]
        public int Hour { get; set; }
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
    }
}