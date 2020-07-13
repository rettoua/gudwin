using System;
using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class RelaySaver : ConcurrentWorkerBase<RelayAction> {
        public static RelaySaver Instance = new RelaySaver();

        protected override bool Execute(RelayAction relayAction) {
            try {
                CasResult<string> casResult = CouchbaseManager.Online.GetWithCas<string>(RelayCollection.ID);
                var list = new RelayCollection();
                if (!string.IsNullOrEmpty(casResult.Result)) {
                    list = JSON.Deserialize<RelayCollection>(casResult.Result);
                }
                foreach (RelayAction action in list) {
                    if (action.TrackerId == relayAction.TrackerId) {
                        list.Remove(action);
                        break;
                    }
                }
                list.Add(relayAction);
                return CouchbaseManager.Online.Cas(StoreMode.Set, RelayCollection.ID, JSON.Serialize(list), casResult.Cas).Result;
            } catch (Exception exception) {
                Logger.Write(exception);
                return false;
            }
        }

        public void Add(RelayAction relayAction) {
            _queue.Enqueue(relayAction);
        }
    }
}