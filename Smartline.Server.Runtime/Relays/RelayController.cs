using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smartline.Mapping;
using Smartline.Server.Runtime.SignalR;
using Smartline.Server.Runtime.TransportLayout;

namespace Smartline.Server.Runtime.Relays {
    public class RelayController {
        public static RelayController Instance = new RelayController();

        private RelayController() {
            WebServer.Instance.TurnOnRelayEvent -= TurnOnRelayEvent;
            WebServer.Instance.TurnOnRelayEvent += TurnOnRelayEvent;
            WebServer.Instance.TurnOffRelayEvent -= TurnOffRelayEvent;
            WebServer.Instance.TurnOffRelayEvent += TurnOffRelayEvent;
            WebServer.Instance.TurnOffAlarmingEvent -= TurnOffAlarmingEvent;
            WebServer.Instance.TurnOffAlarmingEvent += TurnOffAlarmingEvent;
        }

        internal void TurnOnRelayEvent(string userId, int trackerId, int relayId) {
            User user = CouchbaseManager.GetUser(userId);
            if (user == null) { return; }
            Tracker tracker = user.Trackers.FirstOrDefault(track => track.Id == trackerId);
            if (tracker == null) { return; }
            byte[] buffer = null;
            if (tracker.Relay != null && tracker.Relay.Id == relayId) {
                buffer = CreateTurnOnRelayCommand();
            } else if (tracker.Relay1 != null && tracker.Relay1.Id == relayId) {
                buffer = CreateTurnOnInputCommand(1);
            } else if (tracker.Relay2 != null && tracker.Relay2.Id == relayId) {
                buffer = CreateTurnOnInputCommand(2);
            }
            if (buffer == null) { return; }
            BottleneckMessageReceiver.Instance.SendData(trackerId, buffer);
        }

        internal void TurnOffRelayEvent(string userId, int trackerId, int relayId) {
            User user = CouchbaseManager.GetUser(userId);
            if (user == null) { return; }
            Tracker tracker = user.Trackers.FirstOrDefault(track => track.Id == trackerId);
            if (tracker == null) { return; }
            byte[] buffer = null;
            if (tracker.Relay != null && tracker.Relay.Id == relayId) {
                buffer = CreateTurnOffRelayCommand();
            } else if (tracker.Relay1 != null && tracker.Relay1.Id == relayId) {
                buffer = CreateTurnOffInputCommand(1);
            } else if (tracker.Relay2 != null && tracker.Relay2.Id == relayId) {
                buffer = CreateTurnOffInputCommand(2);
            }
            if (buffer == null) { return; }
            BottleneckMessageReceiver.Instance.SendData(trackerId, buffer);
        }

        private void TurnOffAlarmingEvent(string userId, int trackerId) {
            User user = CouchbaseManager.GetUser(userId);
            if (user == null) { return; }
            byte[] buffer = CreateTurnOffAlarmingCommand();
            BottleneckMessageReceiver.Instance.SendData(trackerId, buffer);
        }

        private byte[] CreateTurnOnInputCommand(int index) {
            string commandPrefix = string.Format("OUT_K{0}=1", index);
            List<byte> buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        private byte[] CreateTurnOffInputCommand(int index) {
            string commandPrefix = string.Format("OUT_K{0}=0", index);
            List<byte> buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        private byte[] CreateTurnOnRelayCommand() {
            const string commandPrefix = "RELE=1";
            var buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        private byte[] CreateTurnOnRelaySafelyCommand() {
            const string commandPrefix = "RELE=2";
            List<byte> buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        private byte[] CreateTurnOffRelayCommand() {
            const string commandPrefix = "RELE=0";
            List<byte> buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        private byte[] CreateTurnOffAlarmingCommand() {
            const string commandPrefix = "SOS=0";
            List<byte> buffer = Encoding.ASCII.GetBytes(commandPrefix).ToList();
            buffer.Add(13);//add [0D][0A] to end of the command
            buffer.Add(10);
            return buffer.ToArray();
        }

        public void SaveTurnOnAction(ulong userId, int trackerId, int relayId) {
            SaveRelayAction(userId, trackerId, relayId, RequiredActionEnum.On);
        }

        public void SaveTurnOffAction(ulong userId, int trackerId, int relayId) {
            SaveRelayAction(userId, trackerId, relayId, RequiredActionEnum.Off);
        }

        public void SaveTurnOffAlarmingAction(ulong userId, int trackerId) {
            SaveRelayAction(userId, trackerId, 0, RequiredActionEnum.Alarming);
        }

        private void SaveRelayAction(ulong userId, int trackerId, int relayId, RequiredActionEnum action) {
            var relayAction = new TurnRelay {
                RequiredAction = action,
                UserId = userId,
                TrackerId = trackerId,
                RelayId = relayId
            };
            CouchbaseManager.AddRelayAction(relayAction);
        }


    }
}