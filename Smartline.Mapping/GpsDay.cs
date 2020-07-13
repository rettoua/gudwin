using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class GpsDay {
        public string Id {
            get { return GetId(Date, TrackerId); }
        }

        [JsonProperty("a")]
        public DateTime Date {
            get { return _date.Date; }
            set { _date = value; }
        }
        private DateTime _date;

        [JsonProperty("e")]
        public int TrackerId { get; set; }

        [JsonProperty("packages")]
        public List<Gp> Packages { get; set; }

        public GpsDay() {
            Packages = new List<Gp>();
        }

        public static string GetId(DateTime dateTime, int trackerUid) {
            return string.Format("{0}_{1}", dateTime.ToString("dd_MM_yyyy"), trackerUid);
        }

        public void Add(Gp point) {
            lock (Packages) {
                Packages.Add(point);
            }
        }

        public DateTime GetLastPackageDate() {
            Gp gp = Packages.LastOrDefault();
            if (gp == null) {
                return Date;
            }
            return gp.GetActualTime();
        }

        public DateTime GetLastPackageSendDate() {
            Gp gp = Packages.LastOrDefault();
            if (gp == null) {
                return Date;
            }
            return gp.SendTime;
        }
    }
}