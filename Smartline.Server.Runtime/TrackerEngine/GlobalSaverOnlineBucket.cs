using System;
using Enyim.Caching.Memcached;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class GlobalSaverOnlineBucket : GlobalSaver {
        public new static GlobalSaverOnlineBucket Instance = new GlobalSaverOnlineBucket();

        protected override bool Execute(ValueToSave value) {
            try {
                switch (value.Bucket) {
                    case BucketEnum.Online: {
                            CouchbaseManager.Online.Store(StoreMode.Set, value.Key, value.Value, value.Expire);
                        }
                        break;
                }
            } catch (Exception exception) {
                Logger.Write(exception);
                return false;
            }
            return true;
        }
    }
}