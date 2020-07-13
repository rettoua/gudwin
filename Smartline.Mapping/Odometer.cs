using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class Odometer {
        /// <summary>
        /// described Tracker.Id property
        /// </summary>
        [JsonProperty("t")]
        public int TrackerUid { get; set; }
        [JsonProperty("id")]
        public int TrackerId { get; set; }
        [JsonProperty("o")]
        public int Meters { get; set; }
        [JsonProperty("i")]
        public DateTime InitialDate { get; set; }

        public void Update() {
            if (InitialDate == DateTime.MinValue) {
                InitialDate = DateTime.Now.AddDays(-31);
            }
            var ids = new List<string>();
            var tempDate = InitialDate;
            while (tempDate <= DateTime.Now) {
                ids.Add(GpsDay.GetId(tempDate, TrackerUid));
                tempDate = tempDate.AddDays(1);
            }
            List<GpsDay> dayPackages = CouchbaseManager.GetGpsDays(ids);
            if (dayPackages.Count > 0) {
                foreach (GpsDay gpsDay in dayPackages) {
                    foreach (Gp gp in gpsDay.Packages) {
                        if (gp.SendTime > InitialDate && gp.Distance.HasValue) {
                            Meters += gp.Distance.Value;
                        }
                    }
                }
                GpsDay lastGpsDay = dayPackages.Last();
                InitialDate = lastGpsDay.Packages.Any() ? lastGpsDay.Packages.Last().SendTime : lastGpsDay.Date;
            } else {
                InitialDate = DateTime.Now;
            }
            Save();
        }

        public void Save() {
            string documentId = Tracker.GetOdometerDocumentId(TrackerUid);
            CouchbaseManager.SaveOdometer(documentId, this);
        }
    }
}