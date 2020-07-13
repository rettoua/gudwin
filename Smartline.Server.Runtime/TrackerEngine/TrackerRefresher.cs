using System.Collections.Generic;
using System.Threading;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class TrackerRefresher : ConcurrentWorkerBase<int> {
        public static TrackerRefresher Instance = new TrackerRefresher();

        protected override void Working() {
            //while (ServerDomain.Working) {
            //    var trackers = GetTrackersForRefres();
            //    if (trackers != null && trackers.Count > 0) {
            //        trackers.ForEach(UpdateTrackerInTrackerWrapper);
            //    }
            //    Thread.Sleep(5000);
            //}
        }

        //private TrackerRefreshCollection GetTrackersForRefres() {
        //    var result = CouchbaseManager.Online.Get(TrackerRefreshCollection.Id);
        //    if (result != null) {
        //        CouchbaseManager.Online.Remove(TrackerRefreshCollection.Id);
        //        var collection = JSON.Deserialize<TrackerRefreshCollection>(result + "");
        //        return collection;
        //    }
        //    return null;
        //}

        private void UpdateTrackerInTrackerWrapper(int trackerId) {
            //TrackerWrapper trackerWrapper = GetTrackerWrapper(trackerId);
            //if (trackerWrapper == null) { return; }
            //Tracker tracker = GetTracker(trackerId);
            //if (tracker == null) { return; }
            //trackerWrapper.Tracker = tracker;
        }

        //private TrackerWrapper GetTrackerWrapper(int trackerId) {
        //    foreach (TrackerWrapper wrapper in _serverDomain.TrackerWrappers) {
        //        if (wrapper.Socket.IsStarted && wrapper.Tracker != null && wrapper.Tracker.TrackerId == trackerId) {
        //            return wrapper;
        //        }
        //    }
        //    return null;
        //}

        //private Tracker GetTracker(int trackerId) {
        //    var tracker = CouchbaseManager.GetTracker(trackerId);
        //    return tracker;
        //}
    }

    /// <summary>
    /// contains collection of tracker.trackerid for refresh data in actual connection
    /// </summary>
    public class TrackerRefreshCollection : List<int> {
        public const string Id = "trackers_for_refresh";
    }
}