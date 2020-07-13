using System;
using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Mapping.StateManager;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class CommunicationStateManager : ConcurrentWorkerBase<StateManagerPackage> {
        public static CommunicationStateManager Instance = new CommunicationStateManager();

        public void AddNewConnectionInitialized(string ip) {
            var newConnection = new StateManagerPackage { Date = DateTime.Now, Ip = ip, TypeId = "nc" };
            AddToQueue(newConnection);
        }

        public void AddTrackerConnected(int trackerId, string userName, string ip) {
            var trackerConnected = new StateManagerPackage {
                Date = DateTime.Now,
                TrackerId = trackerId,
                UserName = userName,
                Ip = ip,
                TypeId = "tc"
            };
            AddToQueue(trackerConnected);
        }

        public void AddTrackerDisconnected(Tracker tracker, User user, string reason, string ip) {
            var trackerDisconnected = new StateManagerPackage { Date = DateTime.Now, Reason = reason, Ip = ip, TypeId = "td" };
            if (tracker != null) {
                trackerDisconnected.TrackerId = tracker.TrackerId;
            }
            if (user != null) {
                trackerDisconnected.UserName = user.UserName;
            }
            AddToQueue(trackerDisconnected);
        }

        public void AddIncorrectNumberTracker(int trackerNumber) {
            var incorrectNumber = new StateManagerPackage { Date = DateTime.Now, TrackerId = trackerNumber, TypeId = "in" };
            AddToQueue(incorrectNumber);
        }

        private void AddToQueue(StateManagerPackage package) {
            _queue.Enqueue(package);
        }

        protected override bool Execute(StateManagerPackage item) {
            try {
                var obj = JSON.Serialize(item);
                CouchbaseManager.Monitoring.Store(StoreMode.Set, Guid.NewGuid().ToString(), obj, new TimeSpan(2, 0, 0, 0));
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return true;
        }


    }
}