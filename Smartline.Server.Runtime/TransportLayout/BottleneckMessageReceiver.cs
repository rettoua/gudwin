using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.Package;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Server.Runtime.TransportLayout {
    public class BottleneckMessageReceiver : ConcurrentWorkerBase<TemporaryForIncomingPackages> {
        private readonly ConcurrentDictionary<int, GpHandler> _localStores;
        private readonly ProtocolSpecification _protocolSpecification = new ProtocolSpecification();
        private readonly ProtocolEngine _protocolEngine;

        public static BottleneckMessageReceiver Instance = new BottleneckMessageReceiver();

        public BottleneckMessageReceiver() {
            _protocolEngine = new ProtocolEngine();
            SleepIntervalWhileWorking = 1;
            SleepIntervalIfFailureExecute = 15;
            _localStores = new ConcurrentDictionary<int, GpHandler>();
        }

        //public void Add(byte[] buffer, Socket socket, int trackerId) {
        //    //all packages in protocol have length equal 25 all other packages are damaged and should be ignored
        //    if (buffer.Length != 25) {
        //        return;
        //    }
        //    Debug.WriteLine("Tracker {0}-{1}", trackerId, Thread.CurrentThread.ManagedThreadId);
        //    _queue.Enqueue(new TemporaryForIncomingPackages { Buffer = buffer, Socket = socket, TrackerId = trackerId });
        //}

        public void Execute(byte[] buffer, Socket socket, int trackerId) {
            Execute(new TemporaryForIncomingPackages { Buffer = buffer, Socket = socket, TrackerId = trackerId });
        }

        public void SendData(int trackerId, byte[] buffer) {
            if (_serverDomain.SocketListenerWithOk.SendData(trackerId, buffer)) { return; }
            if (_serverDomain.SocketListenerWithoutOk.SendData(trackerId, buffer)) { return; }
        }

        protected override bool Execute(TemporaryForIncomingPackages item) {
            int trackerId = GetTrackerId(item.Buffer);
            if (!_localStores.ContainsKey(trackerId) && !TryAddGpStore(trackerId)) {
                return true;
            }
            GpHandler store;
            if (_localStores.TryGetValue(trackerId, out store)) {
                try {
                    _protocolEngine.Parse(item, store);
                } catch (Exception exception) {
                    Logger.Write(exception, item.Buffer);
                }
            } else {
                Logger.Write(new Exception("No tracker with id " + trackerId));
            }
            return true;
        }

        internal int GetTrackerId(IEnumerable<byte> buffer) {
            return
                BitConverter.ToInt32(
                    buffer.Skip(_protocolSpecification.GPS_TRACKERID.NoByteStart).Take(
                        _protocolSpecification.GPS_TRACKERID.ByteCnt).Reverse().ToArray(), 0);
        }

        private bool TryAddGpStore(int trackerId) {
            IUserTracker userTracker = CouchbaseManager.GetUserTracker(trackerId);
            if (userTracker == null || userTracker.User == null || userTracker.Tracker == null) {
                Logger.Write(new Exception(string.Format("tracker {0} not found", trackerId)));
                return false;
            }
            try {
                var gpHandler = new GpHandler(userTracker.Tracker, _serverDomain, new GpStorage((int)userTracker.User.Id, userTracker.Tracker.Id), new SensorHandler());
                while (!_localStores.TryAdd(trackerId, gpHandler)) {
                    Thread.Sleep(10);
                }
                return true;
            } catch (Exception exception) {
                Logger.Write(exception);
                return false;
            }
        }
    }
}