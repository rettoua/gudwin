using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;

namespace Smartline.Compacting {
    public class GpsDayProvider : IGpsDayProvider {
        private Tracker _tracker;

        public void AssignTracker(Tracker tracker) {
            _tracker = tracker;
        }

        public GpsDay GetLastDay() {
            DateTime? gpsDay = GetLastGpsDayDate();
            GpsDay day = gpsDay.HasValue ? GetGpsDay(gpsDay.Value) : FindLastGpsDay();
            return day;
        }

        public GpsDay CreateDay() {
            Gp gp = CouchbaseManager.GetFirstGp(_tracker.Id);
            if (gp == null) { return null; }
            GpsDay day = GpsDayHelper.Create(gp);
            day.Packages.Add(gp);
            return day;
        }

        private DateTime? GetLastGpsDayDate() {
            return GpsDayHelper.GetLastGpsDayDate(_tracker.Id);
        }

        private GpsDay GetGpsDay(DateTime date) {
            return CouchbaseManager.GetGpsDay(GpsDay.GetId(date, _tracker.Id));
        }

        private GpsDay FindLastGpsDay() {
            DateTime? lastDate = GetLastGpsDayDate();
            if (!lastDate.HasValue) {
                lastDate = FindLastGpsDayDate();
            }
            if (lastDate.HasValue) {
                return CouchbaseManager.GetGpsDay(GpsDay.GetId(lastDate.Value, _tracker.Id));
            }
            return null;
        }

        private DateTime? FindLastGpsDayDate() {
            List<string> ids = GpsDayHelper.GenerateIds(DateTime.Now.AddMonths(-1), DateTime.Now, _tracker.Id);
            for (int i = ids.Count - 1; i >= 0; i--) {
                var dayShort = CouchbaseManager.GetSingleValueFromGps<GpsDayShort>(ids[i]);
                if (dayShort != null) {
                    return dayShort.Date;
                }
            }
            return null;
        }


        /// <summary>
        /// got only a Date field from GpsDay in order to decrease heavy of json converting
        /// </summary>
        private class GpsDayShort {
            [JsonProperty("a")]
            public DateTime Date { get; set; }
        }
    }
}