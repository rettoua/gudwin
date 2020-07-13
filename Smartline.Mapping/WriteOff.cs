using System;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class WriteOff {
        public int T { get { return AccountingHelper.WriteOff; } }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("trackerUid")]
        public int TrackerUid { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("amountBefore")]
        public double AmountBefore { get; set; }

        [JsonProperty("amountAfter")]
        public double AmountAfter { get; set; }

        [JsonProperty("state")]
        public TransactionState State { get; set; }

        public string GetId() {
            return string.Format("wo_{0}_{1}_{2}",
                UserId,
                TrackerUid,
                Time.ToString("yyMMdd"));
        }
    }
}