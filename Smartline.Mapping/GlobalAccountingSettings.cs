using Enyim.Caching.Memcached;
using Ext.Net;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class GlobalAccountingSettings {
        public const string Id = "GlobalAccountingSettings";

        [JsonProperty("writeOffPerDayAmount")]
        public double WriteOffPerDayAmount { get; set; }

        [JsonProperty("writeOffPerMonthAmount")]
        public double WriteOffPerMonthAmount { get; set; }

        [JsonProperty("offAmount")]
        public double OffAmount { get; set; }

        public void Save() {
            CouchbaseManager.SaveToAccountingBucket(StoreMode.Set, Id, JSON.Serialize(this));
        }

        public static GlobalAccountingSettings Get() {
            return CouchbaseManager.GetSingleValueFromAcounting<GlobalAccountingSettings>(Id) ?? new GlobalAccountingSettings();
        }
    }
}