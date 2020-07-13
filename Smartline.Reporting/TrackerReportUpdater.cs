using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Mapping;
using Smartline.Server.Runtime;

namespace Smartline.Reporting {
    internal class TrackerReportUpdater {
        private readonly Tracker _tracker;
        public TrackerReportUpdater(Tracker tracker) {
            _tracker = tracker;
        }

        internal void UpdateReport() {
            ReportFullObject lastReportObject = GetLastReportFull();
            if (lastReportObject == null) {
                LoadDataToDate(null);
            } else {
                DateTime date = lastReportObject.ReportFull.Date;
                if (lastReportObject.ReportFull.LastDate.HasValue) {
                    date = lastReportObject.ReportFull.LastDate.Value.AddSeconds(1);
                } else if (lastReportObject.ReportFull.Commons.Count > 0) {
                    date = date.AddHours(lastReportObject.ReportFull.Commons.Max(o => o.Hour));
                }
                LoadDataToDate(date);
            }
        }

        private ReportFullObject GetLastReportFull() {
            return CouchbaseManager.GetLastFullReport(_tracker.Id);
        }

        private DateTime? GetFirstGpsPackageDate() {
            var gp = CouchbaseManager.GetFirstGp(_tracker.Id);
            if (gp == null) {
                return null;
            }
            return gp.SendTime;
        }

        private DateTime? GetLastGpsPackageDate() {
            Gp gp = CouchbaseManager.GetLastGp(_tracker.Id);
            if (gp == null) {
                return null;
            }
            return gp.GetActualTime();
        }

        private void LoadDataToDate(DateTime? startFromDate) {
            DateTime? lastGpPackage = GetLastGpsPackageDate();
            if (!lastGpPackage.HasValue) { return; }
            if (startFromDate.HasValue) {
                LoadData(startFromDate.Value, lastGpPackage.Value);
            } else {
                DateTime? firstGpPackageDate = GetFirstGpsPackageDate();
                if (!firstGpPackageDate.HasValue) { return; }
                LoadData(firstGpPackageDate.Value, lastGpPackage.Value);
            }
        }

        private void LoadData(DateTime from, DateTime to) {
            List<Gp> gps = GpsDayHelper.LoadTrack(_tracker.Id, from, to);
            if (gps.Count > 0) {
                var reportAdapter = new ReportAdapter(_tracker.Id);
                gps.ForEach(reportAdapter.AddItem);
                //reportAdapter.Save();
            }
        }
    }
}