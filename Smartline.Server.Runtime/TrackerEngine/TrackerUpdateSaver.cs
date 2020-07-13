using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class TrackerUpdateSaver : ConcurrentWorkerBase<int> {
        public static TrackerUpdateSaver Instance = new TrackerUpdateSaver();

        protected override bool Execute(int trackerId) {
            try {
                CasResult<string> casResult = CouchbaseManager.Online.GetWithCas<string>(TrackerRefreshCollection.Id);
                var list = new TrackerRefreshCollection();
                if (!string.IsNullOrEmpty(casResult.Result)) {
                    list = JSON.Deserialize<TrackerRefreshCollection>(casResult.Result);
                }
                foreach (int id in list) {
                    if (id == trackerId) {
                        casResult.Result.Remove(id);
                        break;
                    }
                }
                list.Add(trackerId);
                return CouchbaseManager.Online.Cas(StoreMode.Set, TrackerRefreshCollection.Id, JSON.Serialize(list), casResult.Cas).Result;
            } catch (System.Exception exception) {
                Logger.Write(exception);
                return false;
            }
        }

        public void Add(int tracker) {
            _queue.Enqueue(tracker);
        }
    }
}
