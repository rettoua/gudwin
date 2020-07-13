using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Smartline.Common.Runtime;

namespace Smartline.Server.Runtime.SignalR.Hubs {
    public sealed class MapHub : Hub {
        private static IHubContext _mapHubContext;
        /// <summary>
        /// Context instance to access client connections to broadcast to
        /// </summary>
        public static IHubContext MapHubContext {
            get {
                return _mapHubContext ?? (_mapHubContext = GlobalHost.ConnectionManager.GetHubContext<MapHub>());
            }
        }

        public override Task OnDisconnected() {
            LeaveUserConnection();
            return base.OnDisconnected();
        }

        public void ItsMe() {
            string userId = Clients.Caller.userId + "";
            JoinUserConnection(userId);
            WebServer.Instance.AddMapHubConnection(Context.ConnectionId, userId);
        }

        public static void AddGps(string userId, string value) {
            try {
                MapHubContext.Clients.Group(userId).addGps(value);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void RelayTurnedOnExecuted(string userId, int trackerId, int relayId) {
            try {
                MapHubContext.Clients.Group(userId).relayTurnedOnExecuted(trackerId, relayId);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public static void RelayTurnedOffExecuted(string userId, int trackerId, int relayId) {
            try {
                MapHubContext.Clients.Group(userId).relayTurnedOffExecuted(trackerId, relayId);
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        public void TurnOnRelay(int trackerId, int relayId) {
            WebServer.Instance.TurnOnRelay(Context.ConnectionId, trackerId, relayId);
        }

        public void TurnOffRelay(int trackerId, int relayId) {
            WebServer.Instance.TurnOffRelay(Context.ConnectionId, trackerId, relayId);
        }

        public void TurnOffAlarming(int trackerId) {
            WebServer.Instance.TurnOffAlarming(Context.ConnectionId, trackerId);
        }

        public Task JoinUserConnection(string userId) {
            return Groups.Add(Context.ConnectionId, userId);
        }

        public Task LeaveUserConnection() {
            string userId = WebServer.Instance.GetUserIdByConnectionId(Context.ConnectionId);
            if (string.IsNullOrEmpty(userId)) {
                return null;
            }
            WebServer.Instance.RemoveMapHubConnection(Context.ConnectionId, userId);
            return Groups.Remove(Context.ConnectionId, userId);
        }
    }
}