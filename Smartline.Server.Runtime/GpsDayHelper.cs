using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime {
    public class GpsDayHelper {
        public static GpsDay GetOrCreate(GpsDay gpsDay, Gp gp) {
            if (gpsDay == null) {
                GpsDay newGpsDay = Create(gp);
                GpsDay gpsDayFromDb = CouchbaseManager.GetGpsDay(newGpsDay.Id);
                return gpsDayFromDb ?? newGpsDay;
            }
            gpsDay.Packages.Clear();
            return Create(gp);
        }

        public static bool IsOkGpsDay(GpsDay gpsDay, Gp gp) {
            if (gpsDay == null) { return false; }
            return gpsDay.Date.Day == gp.GetActualTime().Day;
        }

        public static string Serialize(GpsDay gpsDay) {
            string value;
            lock (gpsDay.Packages) {
                value = JSON.Serialize(gpsDay);
            }
            return value;
        }

        public static GpsDay Create(Gp gp) {
            return new GpsDay {
                Date = gp.GetActualTime(),
                TrackerId = gp.TrackerId
            };
        }

        public static List<Gp> LoadTrack(int tracker, DateTime from, DateTime to) {
            var packages = new List<Gp>();
            var initialFrom = new DateTime(from.Ticks);
            if (from.Date == to.Date) {
                List<Gp> packagesByPeriod = GetTrackByPeriod(from, to, tracker);
                if (packagesByPeriod != null && packagesByPeriod.Count > 0) {
                    packages.AddRange(packagesByPeriod);
                }
            } else {
                var nextFrom = new DateTime(initialFrom.Year, initialFrom.Month, initialFrom.Day, 0, 0, 0).AddDays(1);
                while (initialFrom <= to) {
                    List<Gp> packagesByPeriod = GetTrackByPeriod(initialFrom, nextFrom, tracker);
                    if (packagesByPeriod != null && packagesByPeriod.Count > 0) {
                        packages.AddRange(packagesByPeriod);
                    }
                    initialFrom = nextFrom;
                    nextFrom = nextFrom.AddDays(1);
                }
            }
            var r = packages.OrderBy(o => o.SendTime).Where(o => o.GetActualTime() >= from && o.SendTime <= to).ToList();
            return r;
        }

        private static List<Gp> GetTrackByPeriod(DateTime from, DateTime to, int trackerUid) {
            var result = new List<Gp>();
            GpsDay gpsDay = CouchbaseManager.GetGpsDay(GpsDay.GetId(from, trackerUid));
            if (gpsDay != null) {
                result.AddRange(gpsDay.Packages);
                from = gpsDay.GetLastPackageDate().AddSeconds(1);
            }
            try {
                List<Gp> gps = CouchbaseManager.LoadTrack<Gp>(trackerUid, from, to);
                if (gps != null) {
                    result.AddRange(gps);
                }
            } catch (Exception exception) {
            }
            return result;
        }

        internal static List<GpsDay> GetDaysByPeriod(DateTime from, DateTime to, int trackerUid) {
            List<string> ids = GenerateIds(from, to, trackerUid);
            List<GpsDay> days = CouchbaseManager.GetGpsDays(ids).OrderBy(o => o.Date).ToList();
            return days;
        }

        public static Gp GetLastGp(int trackerId) {
            var gpsDate = new GpsDay {
                Date = DateTime.Now,
                TrackerId = trackerId
            };
            GpsDay gpsDay = CouchbaseManager.GetGpsDay(gpsDate.Id);
            if (gpsDay != null && gpsDay.Packages.Count > 0) {
                return gpsDay.Packages.OrderByDescending(o => o.GetActualTime()).FirstOrDefault();
            }
            Gp lastGp = CouchbaseManager.GetLastGp(trackerId, StaleMode.UpdateAfter);
            if (lastGp != null && lastGp.Latitude == 0 && lastGp.Longitude == 0) {
                gpsDay = new GpsDay {
                    TrackerId = trackerId,
                    Date = lastGp.SendTime
                };
                gpsDay = CouchbaseManager.GetGpsDay(gpsDay.Id);
                if (gpsDay != null && gpsDay.Packages.Count > 0) {

                    return gpsDay.Packages.Where(o => o.Latitude != 0).OrderByDescending(o => o.GetActualTime()).FirstOrDefault();
                }
            }
            return lastGp;
        }

        public static DateTime? GetLastGpsDayDate(int trackerId) {
            LastGpsDayInfo dayInfo = CouchbaseManager.GetLastGpsDayInfo(LastGpsDayInfo.GetId(trackerId));
            if (dayInfo != null) {
                return dayInfo.DateTime;
            }
            return null;
        }


        public static List<string> GenerateIds(DateTime from, DateTime to, int trackerUid) {
            var values = new List<string>();
            for (DateTime d = from; d <= to; d = d.AddDays(1)) {
                values.Add(GpsDay.GetId(d, trackerUid));
            }
            return values;
        }
    }
}