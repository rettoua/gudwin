using System;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class GpHandler {
        private readonly IGpStorage _gpStorage;
        private Gp _lastGpPoint;
        private GpsSignal _gpsSignal;
        private bool _onStopping;
        private readonly int _trackerId;
        private readonly Tracker _tracker;
        private readonly ServerDomain _serverDomain;
        private readonly ISensorHandler _sensorHandler;
        private string _lastId;

        public int TrackerId { get { return _trackerId; } }

        public Tracker Tracker { get { return _tracker; } }

        internal User User { get; set; }

        public GpHandler(Tracker tracker, ServerDomain serverDomain, IGpStorage gpStorage, ISensorHandler sensorHandler) {
            _gpStorage = gpStorage;
            _tracker = tracker;
            _trackerId = tracker.Id;
            _serverDomain = serverDomain;
            _sensorHandler = sensorHandler;
            AttachEventHandlers();
        }

        private void AttachEventHandlers() {
            if (_serverDomain == null) { return; }
            _serverDomain.OnStopping -= OnStoppingServerDomain;
            _serverDomain.OnStopping += OnStoppingServerDomain;
        }

        private void OnStoppingServerDomain() {
            _onStopping = true;
        }

        public void SetNewPoint(Gp point) {
            if (_onStopping) { return; }
            try {
                if (!IsActualTime(point)) {
                    Logger.Write(new Exception("not actual time " + point.GetActualTime() + " tracker: " + point.TrackerId));
                    return;
                }

                LoadLastGp();
                VerifyPoint(point);
                ProcessPoint(point);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private void ProcessPoint(Gp point) {
            if (_lastGpPoint == null) {
                SetLastAndSave(point);
                return;
            }
            _sensorHandler.Update(point);
            _lastGpPoint.Sensors = point.Sensors;
            _lastGpPoint.Battery = point.Battery;
            if (IsParking(_lastGpPoint) && IsParking(point)) {
                _lastGpPoint.EndTime = point.SendTime;
                if (point.Latitude != 0) {
                    _lastGpPoint.Latitude = point.Latitude;
                }
                if (point.Longitude != 0) {
                    _lastGpPoint.Longitude = point.Longitude;
                }
                SaveLastPoint();
                return;
            }
            SetLastAndSave(point);
        }

        private void VerifyPoint(Gp point) {
            if (_lastGpPoint == null) { return; }
            if (_lastGpPoint.SendTime.Date != point.SendTime.Date) {
                _lastGpPoint = null;
            }
        }

        internal void SetServiceInfo(TrackerServiceInfo serviceInfo) {
            serviceInfo.TrackerId = _trackerId;
            //GlobalSaver.Instance.Add(Guid.NewGuid() + "", JSON.Serialize(serviceInfo), new TimeSpan(61, 0, 15, 0), GlobalSaver.BucketEnum.Gps);
        }

        public void SetGpsSignal(GpsSignal gpsSignal) {
            _gpsSignal = gpsSignal;
            SaveLastPoint(false);
        }

        private void SetLastAndSave(Gp point) {
            _lastGpPoint = point;
            _lastId = Guid.NewGuid().ToString();
            SaveLastPoint();
        }

        private bool IsParking(Gp gp) {
            bool isParking = gp.Speed == 0 || (gp.Distance == 0 && _tracker != null && _tracker.OldTracker != true) || gp.Distance == null;
            if (isParking) {
                gp.Distance = 0;
                gp.Speed = 0;
            }
            return isParking;
        }

        private bool IsActualTime(Gp point) {
            return ((DateTime.Now - point.SendTime).TotalMinutes) > -30;
        }

        private void UpdateGpsSignal() {
            _lastGpPoint.GpsSignal = _gpsSignal;
            //if (_gpsSignal != null && !_gpsSignal.Active) {
            //    if (_gpsSignal.Date > _lastGpPoint.GetActualTime()) {
            //        if (_lastGpPoint.EndTime.HasValue) {
            //            _lastGpPoint.EndTime = _gpsSignal.Date;
            //        } else {
            //            _lastGpPoint.SendTime = _gpsSignal.Date;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// use AllowState in order to make request asap.
        /// </summary>        
        private void LoadLastGp() {
            if (_lastGpPoint != null) { return; }
            Gp gp = _gpStorage.GetLastGp(_tracker.Id);
            _lastGpPoint = gp;
            _lastId = Guid.NewGuid().ToString();
        }

        private void SaveLastPoint(bool saveToDisk = true) {
            if (_lastGpPoint == null) { return; }
            UpdateGpsSignal();
            _gpStorage.Save(_lastGpPoint, _lastId, saveToDisk);
        }
    }
}