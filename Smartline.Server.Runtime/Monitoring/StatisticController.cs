using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Monitoring {
    public sealed class StatisticController : SingleWorkerBase<int> {
        private readonly TrackersCollection _connectedTrackers = new TrackersCollection();
        private readonly ActualServerState _actualServerState = new ActualServerState();
        private int _packages;
        private readonly ConcurrentDictionary<int, Traffic> _traffics = new ConcurrentDictionary<int, Traffic>();

        private static readonly object LockConnectedTrackers = new object();

        public static StatisticController Instance = new StatisticController();

        public override int Timeout {
            get { return 1000; }
        }

        private StatisticController() {
            ServerStateSingleWorker.Instance.SetObject(_actualServerState);
        }

        private void NewPackage(int count) {
            if (_packages == int.MaxValue) {
                _packages = 0;
            }
            for (int i = 0; i < count; i++) {
                Interlocked.Increment(ref _packages);
            }
            UpdateActualServerState();
        }

        internal void AddOnlineTracker(Tracker tracker) {
            lock (LockConnectedTrackers) {
                if (_connectedTrackers.Contains(tracker)) { return; }
                _connectedTrackers.Add(tracker);
                UpdateActualServerState();
            }
        }

        internal void RemoveOnlineTracker(Tracker tracker) {
            lock (LockConnectedTrackers) {
                _connectedTrackers.Remove(tracker);
                UpdateActualServerState();
            }
        }

        private void UpdateActualServerState() {
            _actualServerState.ConnectedTrackersCount = _connectedTrackers.Count;
            _actualServerState.Packages = _packages;
        }

        private void AddIncomingTraffic(int trackerUid, int bytes) {
            Traffic traffic;
            _traffics.TryGetValue(trackerUid, out traffic);
            if (traffic != null && IsTheSameDay(trackerUid, traffic)) {
                traffic.In += bytes;
            } else {
                InitNewTraffic(new Traffic {
                    TrackerId = trackerUid,
                    In = bytes
                });
            }
        }

        private void AddOutgoingTraffic(int trackerUid, int bytes) {
            Traffic traffic;
            _traffics.TryGetValue(trackerUid, out traffic);
            if (traffic != null && IsTheSameDay(trackerUid, traffic)) {
                traffic.Out += bytes;
            } else {
                InitNewTraffic(new Traffic {
                    TrackerId = trackerUid,
                    Out = bytes
                });
            }
        }

        private void AddInitializedTracker(int trackerUid, Traffic traffic) {
            traffic.TrackerId = trackerUid;
            Traffic existTraffic;
            if (_traffics.TryGetValue(traffic.TrackerId, out existTraffic)) {
                if (existTraffic.Id == traffic.Id) {
                    _traffics[traffic.TrackerId] = existTraffic + traffic;
                } else {
                    InitNewTraffic(traffic);
                }
            } else {
                InitNewTraffic(traffic);
            }
        }

        private void AddNewPackage(int trackerUid, int packageType) {
            Traffic traffic;
            _traffics.TryGetValue(trackerUid, out traffic);
            if (traffic != null && IsTheSameDay(trackerUid, traffic)) {
                if (traffic.Packages.ContainsKey(packageType)) { traffic.Packages[packageType]++; } else {
                    traffic.Packages[packageType] = 1;
                }
            } else {
                var tf = new Traffic { TrackerId = trackerUid };
                tf.Packages[packageType] = 1;
                InitNewTraffic(tf);
            }
        }

        private void InitNewTraffic(Traffic traffic) {
            Traffic trafficFromDatabase = CouchbaseManager.GetTraffic(traffic.Id);
            if (trafficFromDatabase != null && trafficFromDatabase.Id == traffic.Id) {
                _traffics[traffic.TrackerId] = trafficFromDatabase + traffic;
            } else {
                _traffics[traffic.TrackerId] = traffic;
            }
        }

        private bool IsTheSameDay(int trackerId, Traffic existTraffic) {
            string newId = string.Format(Traffic.GetIdPattern(), trackerId, DateTime.Now);
            return newId != existTraffic.Id;
        }

        protected override bool Execute(int item) {
            var copy = new Dictionary<int, Traffic>(_traffics);
            try {
                foreach (KeyValuePair<int, Traffic> pair in copy) {
                    CouchbaseManager.SetTraffic(pair.Value);
                    Thread.Sleep(100);
                }
                SaveOnlineTrackers();
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return true;
        }

        private void SaveOnlineTrackers() {
            int[] connectedTrackers;
            lock (LockConnectedTrackers) {
                connectedTrackers = _connectedTrackers.Select(o => o.Id).ToArray();
            }
            CouchbaseManager.SaveOnlineTrackers(connectedTrackers);
        }
    }
}