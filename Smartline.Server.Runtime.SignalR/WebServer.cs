using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Owin.Hosting;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime.SignalR.Hubs;

namespace Smartline.Server.Runtime.SignalR {
    public class WebServer : IWorker {
        private IDisposable _webServer;
        private readonly string _url;
        private readonly Dictionary<string, string> _mapHubConnections = new Dictionary<string, string>();

        private static readonly object MapHubLockObject = new object();

        public static WebServer Instance = new WebServer();

        private WebServer() {
            _url = string.Format("http://{0}:8081/", LocalIpAddress());
        }

        public static string LocalIpAddress() {
            string localIp = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }

        public void Start() {
            if (_webServer == null) {
                _webServer = WebApp.Start<SignalRCommon>(_url);
            }
        }

        public void Stop() {
            try {
                _webServer.Dispose();
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public void AddGps(string userId, string value) {
            MapHub.AddGps(userId, value);
        }

        public bool IsUserConnected(string userId) {
            return _mapHubConnections.ContainsValue(userId);
        }

        internal void AddMapHubConnection(string connectionId, string userId) {
            lock (MapHubLockObject) {
                _mapHubConnections[connectionId] = userId;
            }
            RaiseUserConnected(userId);
        }

        internal void RemoveMapHubConnection(string connectionId, string userId) {
            lock (MapHubLockObject) {
                if (_mapHubConnections.ContainsKey(connectionId)) {
                    _mapHubConnections.Remove(connectionId);
                }
            }
            RaiseUserDisonnected(userId);
        }

        internal string GetUserIdByConnectionId(string connectionId) {
            string value;
            if (_mapHubConnections.TryGetValue(connectionId, out value)) {
                return value;
            }
            return string.Empty;
        }

        internal void TurnOnRelay(string connectionId, int trackerId, int relayId) {
            string userId = GetUserIdByConnectionId(connectionId);
            RaiseTurnOnRelayEvent(userId, trackerId, relayId);
        }

        internal void TurnOffRelay(string connectionId, int trackerId, int relayId) {
            string userId = GetUserIdByConnectionId(connectionId);
            RaiseTurnOffRelayEvent(userId, trackerId, relayId);
        }

        internal void TurnOffAlarming(string connectionId, int trackerId) {
            string userId = GetUserIdByConnectionId(connectionId);
            RaiseTurnOffAlarmingEvent(userId, trackerId);
        }

        private void RaiseUserConnected(string userId) {
            Action<string> handler = UserConnected;
            if (handler != null) {
                handler(userId);
            }
        }

        private void RaiseUserDisonnected(string userId) {
            Action<string> handler = UserDisconnected;
            if (handler != null) {
                handler(userId);
            }
        }

        private void RaiseTurnOnRelayEvent(string userId, int trackerId, int relayId) {
            Action<string, int, int> handler = TurnOnRelayEvent;
            if (handler != null) {
                handler(userId, trackerId, relayId);
            }
        }

        private void RaiseTurnOffRelayEvent(string userId, int trackerId, int relayId) {
            Action<string, int, int> handler = TurnOffRelayEvent;
            if (handler != null) {
                handler(userId, trackerId, relayId);
            }
        }

        private void RaiseTurnOffAlarmingEvent(string userId, int trackerId) {
            Action<string, int> handler = TurnOffAlarmingEvent;
            if (handler != null) {
                handler(userId, trackerId);
            }
        }

        public event Action<string> UserConnected;
        public event Action<string> UserDisconnected;
        public event Action<string, int, int> TurnOnRelayEvent;
        public event Action<string, int, int> TurnOffRelayEvent;
        public event Action<string, int> TurnOffAlarmingEvent;
    }
}