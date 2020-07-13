using System;
using System.Linq;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Reports {
    public sealed class ReportGenerate {
        private ReportFull _reportFull;
        private readonly int _trackerId;
        private Gp _lastGp;
        private readonly bool _withHistory;

        public ReportFull ReportFull { get { return _reportFull; } }

        private bool IsInitialized { get { return _reportFull != null; } }

        public ReportGenerate(int trackerId, bool withHistory) {
            _trackerId = trackerId;
            _withHistory = withHistory;
        }

        public void AddItem(Gp gp) {
            if (!IsInitialized) {
                Initialize(gp);
            }
            if (gp.EndTime.HasValue) {
                AddItemForStopPackage(gp);
            } else {
                AddItemCore(gp);
            }
        }

        private void AddItemCore(Gp gp) {
            Validate(gp);
            ApplyGp(gp);
            _lastGp = gp.Clone();
        }

        private void AddItemForStopPackage(Gp gp) {
            TimeSpan difference = gp.EndTime.Value - gp.SendTime;
            if (difference.TotalMinutes <= 5) {
                AddItemCore(gp);
                return;
            }
            DateTime cachedEndDate = gp.EndTime.Value;
            DateTime endDate = gp.SendTime.AddMinutes(5);
            while (endDate < cachedEndDate) {
                gp.EndTime = endDate;
                AddItemCore(gp);
                endDate = endDate.AddMinutes(5);
            }
        }

        private void Validate(Gp gp) {
            if (gp.SendTime.Date != _reportFull.Date.Date) {
                CreateNewReportFull(gp);
                return;
            }
            var last = _reportFull.Commons.LastOrDefault();
            var hour = gp.EndTime ?? gp.SendTime;
            if (last == null || last.Hour != hour.Hour) {
                SetActualHour(gp);
            }
        }

        private void Initialize(Gp gp) {
            if (_reportFull != null) {
                return;
            }
            CreateNewReportFull(gp);
        }

        private void CreateNewReportFull(Gp gp) {
            _reportFull = new ReportFull {
                TrackerId = _trackerId,
                Date = gp.SendTime.Date,
            };
            SetActualHour(gp);
        }

        private void ApplyGp(Gp gp) {
            _reportFull.LastDate = gp.GetActualTime();
            ApplyGpCommon(_reportFull, gp, _lastGp);
            ApplyGpCommon(_reportFull.Commons.Last(), gp, _lastGp);
            if (_withHistory) {
                ApplyReportHistory(_reportFull.Commons.Last(), gp);
            }
        }

        private void ApplyReportHistory(ReportCommon reportCommon, Gp gp) {
            if (reportCommon.Histories.Count == 0) {
                AddReportHistory(reportCommon, gp);
                return;
            }
            var currentHistory = reportCommon.Histories.Last();
            if (gp.Speed == 0) {
                if (currentHistory.IsMoving == true) {
                    currentHistory.End = gp.EndTime ?? gp.SendTime;
                    AddReportHistory(reportCommon, gp);
                } else {
                    currentHistory.End = gp.EndTime;
                }
            } else {
                if (currentHistory.IsMoving == true) {
                    currentHistory.End = gp.SendTime;
                    currentHistory.Distance += gp.Distance ?? 0;
                } else {
                    if (((currentHistory.End ?? currentHistory.Start) - currentHistory.Start).TotalMinutes < 2) {
                        if (reportCommon.Histories.Count > 1) {
                            if (reportCommon.Histories[reportCommon.Histories.Count - 2].IsMoving == true) {
                                reportCommon.Histories.Remove(currentHistory);
                                currentHistory = reportCommon.Histories.Last();
                                currentHistory.End = gp.SendTime;
                                currentHistory.Distance += gp.Distance ?? 0;
                                return;
                            }
                        }
                        currentHistory.IsMoving = true;
                        currentHistory.Distance = gp.Distance;
                        currentHistory.End = gp.SendTime;
                    } else {
                        AddReportHistory(reportCommon, gp);
                    }
                }
            }
        }

        private void AddReportHistory(ReportCommon reportCommon, Gp gp) {
            var history = new ReportHistory {
                Start = gp.EndTime ?? gp.SendTime,
                Distance = gp.Distance
            };
            reportCommon.Histories.Add(history);
            if (gp.Speed != 0) {
                reportCommon.Histories.Last().IsMoving = true;                
            } else {
                history.Latitude = gp.Latitude;
                history.Longitude = gp.Longitude;
                //UpdateGeoLocation(history);
            }
        }

        private void ApplyGpCommon(IReportItem item, Gp gp, Gp lastGp) {
            if (item.Distance == null) {
                item.Distance = 0;
            }
            item.Distance += gp.Distance;
            if (gp.Speed == 0 && lastGp != null && lastGp.Speed == 0) {
                item.Parking += (int)(gp.GetActualTime() - lastGp.GetActualTime()).TotalSeconds;
            }
            if (gp.Speed > 0 && lastGp != null) {
                item.Moving += (int)(gp.SendTime - lastGp.GetActualTime()).TotalSeconds;
                item.AvgSpeed = Math.Round((item.AvgSpeed + gp.Speed) / 2m, 2);
            }
            if (gp.Speed > item.MaxSpeed) {
                item.MaxSpeed = gp.Speed;
            }
        }

        private void SetActualHour(Gp gp) {
            if (gp.EndTime != null) {
                gp.SendTime = gp.EndTime.Value;
            }
            _reportFull.Commons.Add(new ReportCommon {
                Hour = gp.SendTime.Hour
            });
        }

        private void UpdateGeoLocation(ReportHistory history) {
            try {
                string result = RevertGeoCoding.Decode(history.Latitude, history.Longitude);
                history.GeoLocation = result;
            } catch (Exception) {

            }
        }
    }
}