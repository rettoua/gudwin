using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Smartline.Server.Runtime.Monitoring;
using Smartline.Server.Runtime.Relays;
using Smartline.Server.Runtime.SignalR;
using Smartline.Server.Runtime.TrackerEngine;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime.TransportLayout;

namespace Smartline.Server.Runtime {
    public sealed class ServerDomain {
        private readonly List<IWorker> _workers;
        private readonly int _port;
        private readonly int _portWithoutOk = 9901;
        private UdpServerManager _udpServer;
        internal static object LockTrackerWrappersObject = new object();

        public int BlockedTrackerId { get; set; }

        #region events

        public event Action ClientDisconnected;
        public event Action ClientConnected;
        #endregion

        public ServerDomain(int port) {
            _port = port;
            _workers = new List<IWorker>
                {
                    GlobalSaver.Instance,
                    GlobalSaverOnlineBucket.Instance,
                    RelaySaver.Instance,
                    CommunicationStateManager.Instance, 
                    ServerStateSingleWorker.Instance, 
                    StatisticController.Instance,                    
                };
        }

        public static bool Working { get; set; }

        public SocketListener SocketListenerWithOk { get; private set; }

        public SocketListener SocketListenerWithoutOk { get; private set; }

        private void StartWorkers() {
            _workers.ForEach(o => o.Start());
        }
        private void StopWorkers() {
            _workers.ForEach(o => o.Stop());
        }

        private void RunListeners() {
            var theSocketListenerSettingsOk = new SocketListenerSettings
                                            (SocketListener.MaxNumberOfConnections,
                                            SocketListener.ExcessSaeaObjectsInPool,
                                            SocketListener.Backlog,
                                            SocketListener.MaxSimultaneousAcceptOps,
                                            SocketListener.ReceivePrefixLength,
                                            SocketListener.TestBufferSize,
                                            SocketListener.SendPrefixLength,
                                            SocketListener.OpsToPreAlloc,
                                             new IPEndPoint(IPAddress.Any, _port));
            SocketListenerWithOk = new SocketListener(theSocketListenerSettingsOk);

            var theSocketListenerSettings = new SocketListenerSettings
                                            (SocketListener.MaxNumberOfConnections,
                                            SocketListener.ExcessSaeaObjectsInPool,
                                            SocketListener.Backlog,
                                            SocketListener.MaxSimultaneousAcceptOps,
                                            SocketListener.ReceivePrefixLength,
                                            SocketListener.TestBufferSize,
                                            SocketListener.SendPrefixLength,
                                            SocketListener.OpsToPreAlloc,
                                            new IPEndPoint(IPAddress.Any, _portWithoutOk));

            SocketListenerWithoutOk = new SocketListener(theSocketListenerSettings, false);

            _udpServer = new UdpServerManager();
        }

        public void Start() {
            Working = true;
            WebServer.Instance.Start();
            RelayController.Instance.ToString();
            StartWorkers();
            RelayExecuter.Instance.Start(this);
            //TrackerRefresher.Instance.Start(this);
            BottleneckMessageReceiver.Instance.Start(this);
            //PackageWorker.Instance.Start(this);
            RunListeners();
        }

        public void Stop() {
            RaiseOnStopingEvent();
            WebServer.Instance.Stop();
            while (GlobalSaver.Instance.IsAny()) {
                Thread.Sleep(100);
            }
            Working = false;
            try {
                StopWorkers();
            } catch (Exception exception) {
                Logger.Write(exception);
            }

        }

        private void RaiseOnStopingEvent() {
            Action handler = OnStopping;
            if (handler != null) {
                handler();
            }
        }

        public event Action OnStopping;
    }
}