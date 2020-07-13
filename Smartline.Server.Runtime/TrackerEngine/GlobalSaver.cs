using System;
using Enyim.Caching.Memcached;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class GlobalSaver : ConcurrentWorkerBase<GlobalSaver.ValueToSave> {
        public static GlobalSaver Instance = new GlobalSaver();

        public GlobalSaver() {
            SleepIntervalWhileWorking = 2;
        }

        public GlobalSaver(ServerDomain serverDomain)
            : base(serverDomain) {
        }

        protected override bool Execute(ValueToSave value) {
            try {
                switch (value.Bucket) {
                    case BucketEnum.Gps: {
                            CouchbaseManager.Gps.Store(StoreMode.Set, value.Key, value.Value, value.Expire);
                        }
                        break;                    
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

        public void Add(string key, object value, TimeSpan expire, BucketEnum bucket) {
            _queue.Enqueue(new ValueToSave { Key = key, Value = value, Expire = expire, Bucket = bucket });
        }

        public bool IsAny() {
            return _queue.Count > 0;
        }

        public class ValueToSave {
            internal string Key { get; set; }
            internal object Value { get; set; }
            internal TimeSpan Expire { get; set; }
            internal BucketEnum Bucket { get; set; }
        }

        public enum BucketEnum {
            Gps,            
            Online
        }
    }
}