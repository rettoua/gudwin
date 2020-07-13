using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Compacting {
    public class GpsCompactor {
        private const int CompactingLimitInMinutes = 60;
        private readonly Tracker _tracker;
        private readonly IGpsDayProvider _gpsDayProvider;
        private GpsDay _day;

        public GpsCompactor(Tracker tracker, IGpsDayProvider gpsDayProvider) {
            _tracker = tracker;
            _gpsDayProvider = gpsDayProvider;
            _gpsDayProvider.AssignTracker(tracker);
        }

        internal void Compact() {
            _day = _gpsDayProvider.GetLastDay() ?? _gpsDayProvider.CreateDay();
            if (_day == null) { return; }
            if (!IsCompactingAllowed(_day)) { return; }
            try {
                List<Gp> gps = CouchbaseManager.LoadTrack<Gp>(_tracker.Id, _day.GetLastPackageSendDate(), DateTime.Now);
                gps.ForEach(GenerateGpsDay);
                Save();
                SaveGpsDayInfo(_tracker.Id, _day.Date);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private bool IsCompactingAllowed(GpsDay day) {
            if (day == null) { throw new ArgumentNullException("day"); }
            TimeSpan timeDifference = DateTime.Now - day.GetLastPackageDate();
            return timeDifference.TotalMinutes >= CompactingLimitInMinutes;
        }

        private void GenerateGpsDay(Gp point) {
            if (point == null) { return; }
            if (!GpsDayHelper.IsOkGpsDay(_day, point)) {
                ProcessNewDay(point);
            } else {
                Gp lastGp = _day.Packages.LastOrDefault();
                if (lastGp != null && lastGp.Equals(point)) {
                    lastGp.Assign(point);
                    return;
                }
            }
            _day.Packages.Add(point);
        }

        private void ProcessNewDay(Gp point) {
            RoundEndOfTheDay(_day);
            Save();
            Gp lastPoint = _day.Packages.LastOrDefault();
            _day = GpsDayHelper.GetOrCreate(_day, point);
            RoundStartOfTheDay(_day, lastPoint, point);
        }

        private void Save() {
            string serializableObject = GpsDayHelper.Serialize(_day);
            GlobalSaver.Instance.Add(_day.Id, serializableObject, new TimeSpan(31, 0, 0, 0), GlobalSaver.BucketEnum.Gps);
        }

        private void SaveGpsDayInfo(int trackerUid, DateTime date) {
            var gpsInfo = new LastGpsDayInfo { DateTime = date };
            string serializableObject = LastGpsDayInfo.Serialize(gpsInfo);
            GlobalSaver.Instance.Add(LastGpsDayInfo.GetId(trackerUid), serializableObject, new TimeSpan(31, 0, 0, 0), GlobalSaver.BucketEnum.Online);
        }

        private static void RoundEndOfTheDay(GpsDay day) {
            Gp lastGp = day.Packages.LastOrDefault();
            if (lastGp == null) { return; }
            if (IsParking(lastGp)) {
                lastGp.EndTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 23, 59, 59);
            }
        }

        //todo: only one question: WTF??? make the method more readable and understandable
        private static void RoundStartOfTheDay(GpsDay day, Gp packageFromPreviosDay, Gp newPoint) {
            if (packageFromPreviosDay == null) {
                day.Packages.Add(newPoint);
                return;
            }
            if (IsParking(packageFromPreviosDay)) {
                if (IsParking(newPoint)) {
                    newPoint.SendTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 0, 0, 0);
                } else {
                    Gp newPackage = newPoint.Clone();
                    newPackage.SendTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 0, 0, 0);
                    newPackage.EndTime = newPoint.SendTime.AddSeconds(-1);
                    day.Packages.Add(newPackage);
                }
            } else {
                if (IsParking(newPoint)) {
                    newPoint.SendTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 0, 0, 0);
                }
            }
            day.Packages.Add(newPoint);
        }

        private static bool IsParking(Gp gp) {
            return gp.Speed == 0 || gp.Distance == 0 || gp.Distance == null;
        }
    }
}