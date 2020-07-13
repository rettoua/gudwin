using System.Collections.Generic;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class GpStoreManager {
        public static GpStoreManager Instance = new GpStoreManager();
        private static readonly Dictionary<int, GpStore> _stores;

        static GpStoreManager() {
            _stores = new Dictionary<int, GpStore>();
        }

        public Dictionary<int, GpStore> Stores {
            get { return _stores; }
        }

        //public GpStore GetStore(Tracker tracker) {
        //    if (tracker == null) {
        //        return null;
        //    }
        //    //lock (_stores) {
        //    //    if (!_stores.ContainsKey(tracker.TrackerId)) {
        //    //        var store = new GpStore(tracker);
        //    //        _stores.Add(tracker.TrackerId, store);
        //    //        return store;
        //    //    }
        //    //    return _stores[tracker.TrackerId];
        //    //}
        //}
    }
}
