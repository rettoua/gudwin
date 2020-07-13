using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class Traffic {
        public Traffic() {
            Packages = new Dictionary<int, int>();
        }

        public string Id {
            get {
                return string.Format(GetIdPattern(), TrackerId, Date.ToString("dd_MM_yyyy"));
            }
        }
        [JsonProperty("i")]
        public int TrackerId { get; set; }
        [JsonProperty("d")]
        public DateTime Date { get; set; }
        [JsonProperty("in")]
        public int In;
        [JsonProperty("out")]
        public int Out;
        [JsonProperty("p")]
        public Dictionary<int, int> Packages { get; set; }

        public static Traffic operator +(Traffic one, Traffic two) {
            one.In += two.In;
            one.Out += two.Out;
            foreach (KeyValuePair<int, int> valuePair in two.Packages) {
                one.Packages[valuePair.Key] += valuePair.Value;
            }
            return one;
        }

        public static string GetIdPattern() {
            return "{0}_{1}";
        }

        public static string GetId(int trackerId, DateTime date) {
            return string.Format(GetIdPattern(), trackerId, date.ToString("dd_MM_yyyy"));
        }

        public void IncrementPackageByType(int type) {
            if (!Packages.ContainsKey(type)) {
                Packages[type] = 1;
            } else {
                Packages[type]++;
            }
        }
    }
}
