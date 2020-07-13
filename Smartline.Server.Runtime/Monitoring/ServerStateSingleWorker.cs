using System;
using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Monitoring {
    public class ServerStateSingleWorker : SingleWorkerBase<ActualServerState> {
        public static ServerStateSingleWorker Instance = new ServerStateSingleWorker();

        public override int Timeout {
            get { return 1000; }
        }

        protected override bool Execute(ActualServerState item) {
            try {
                item.UpdateOn = DateTime.Now;
                string serializedObject = JSON.Serialize(item);
                CouchbaseManager.Online.Store(StoreMode.Set, ActualServerState.Id, serializedObject, new TimeSpan(0, 0, 15, 0));
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return true;
        }
    }
}