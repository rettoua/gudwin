using System;
using System.Collections.Generic;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Relays {
    public class RelayExecuter : ConcurrentWorkerBase<DatabaseTurnRelay> {
        public static RelayExecuter Instance = new RelayExecuter();

        protected override void Working() {
            while (ServerDomain.Working) {
                try {
                    List<DatabaseTurnRelay> result = GetValues();
                    if (result != null) {
                        result.ForEach(ExecuteInternal);
                    }
                } catch (Exception exception) {
                    Logger.Write(exception);
                }
                Thread.Sleep(500);
            }
        }

        private List<DatabaseTurnRelay> GetValues() {
            try {
                List<DatabaseTurnRelay> actions = CouchbaseManager.GetRequiredRelayActions();
                if (actions.Count == 0) { return null; }
                return actions;
            } catch (Exception) {
                return null;
            }
        }

        private void ExecuteInternal(DatabaseTurnRelay relayAction) {
            Execute(relayAction);
        }

        protected override bool Execute(DatabaseTurnRelay action) {
            try {
                switch (action.TurnRelay.RequiredAction) {
                    case RequiredActionEnum.On: { RelayController.Instance.TurnOnRelayEvent(action.TurnRelay.UserId + "", action.TurnRelay.TrackerId, action.TurnRelay.RelayId); }
                    break;
                    case RequiredActionEnum.Off: { RelayController.Instance.TurnOffRelayEvent(action.TurnRelay.UserId + "", action.TurnRelay.TrackerId, action.TurnRelay.RelayId); }
                    break;
                    case RequiredActionEnum.Alarming: { RelayController.Instance.SaveTurnOffAlarmingAction(action.TurnRelay.UserId, action.TurnRelay.TrackerId); }
                    break;
                    default:
                    throw new ArgumentOutOfRangeException();
                }
            } finally {
                CouchbaseManager.RemoveRelayAction(action.DocumentId);
            }
            return true;
        }
    }
}