using System;
using Couchbase;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.SignalR;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class GpStorage : IGpStorage {
        private DateTime _lastSentTimeViaSignalR = DateTime.Now;
        private readonly int _userId;
        private readonly int _trackerUid;

        private bool IsUserConnectedViaSignalr { get { return WebServer.Instance.IsUserConnected(_userId + ""); } }

        public GpStorage(int userId, int trackerUid) {
            _userId = userId;
            _trackerUid = trackerUid;
        }

        public Gp GetLastGp(int trackerUid) {
            return CouchbaseManager.GetLastGp(trackerUid, StaleMode.UpdateAfter);
        }

        public void Save(Gp gp, string id, bool saveToDataBase) {
            if (gp == null) { return; }
            try {
                string serializableObject = JSON.Serialize(gp);
                SendDataThroughtSignalR(serializableObject);
                SaveToOnlineBucket(serializableObject);
                if (saveToDataBase) {
                    SaveToGpsBucket(id, serializableObject);
                }
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        /// <summary>
        /// if user(-s)  connected throught signalr  in web at this moment data should be sent to user browser initially.
        /// </summary>        
        private void SendDataThroughtSignalR(string serializableObject) {
            if (IsUserConnectedViaSignalr && (DateTime.Now - _lastSentTimeViaSignalR).TotalSeconds > 1) {
                WebServer.Instance.AddGps(_userId + "", serializableObject);
                _lastSentTimeViaSignalR = DateTime.Now;
            }
        }

        /// <summary>
        /// save data into online(memcached) bucket in order to show data asap on web, in cases if SignalR does not works
        /// </summary>
        private void SaveToOnlineBucket(string serializableObject) {
            //CouchbaseManager.SaveToOnlineBucket(StoreMode.Set, _tracker.Id + "", serializableObject, new TimeSpan(0, 0, 15, 0));
            GlobalSaverOnlineBucket.Instance.Add(_trackerUid + "", serializableObject, new TimeSpan(31, 0, 0, 0), GlobalSaver.BucketEnum.Online);
        }

        private void SaveToGpsBucket(string id, string serializableObject) {
            //CouchbaseManager.SaveToGpsBucket(StoreMode.Add, Guid.NewGuid().ToString(), serializableObject, new TimeSpan(2, 0, 0, 0));
            GlobalSaver.Instance.Add(id, serializableObject, new TimeSpan(2, 0, 0, 0), GlobalSaver.BucketEnum.Gps);
        }
    }
}