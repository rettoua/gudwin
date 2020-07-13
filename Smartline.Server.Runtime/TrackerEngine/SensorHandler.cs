using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public class SensorHandler : ISensorHandler {
        private SensorsDay _day;
        private DateTime _lastSaveTime = DateTime.Now;

        public void Update(Gp point) {
            if (!SensorDayHelper.IsOkSensorsDay(_day, point)) {
                if (_day != null) {
                    SaveDay();
                    _day.Clear();
                }
                _day = SensorDayHelper.GetOrCreate(_day, point);
            }
            AddStates(point.Sensors, point.GetActualTime());
            SaveDay();
        }

        private void AddStates(Sensors newState, DateTime date) {
            AddState(_day.Relays, newState.Relay, date);
            AddState(_day.Relays1, newState.Relay1, date);
            AddState(_day.Relays2, newState.Relay2, date);
            AddState(_day.Sensors1, newState.Sensor1, date);
            AddState(_day.Sensors2, newState.Sensor2, date);
        }

        private void AddState(List<SensorState> states, bool newState, DateTime date) {
            SensorState last = states.LastOrDefault();
            if (last != null && last.State == newState) {
                last.To = date;
            } else {
                states.Add(new SensorState { From = date, To = date, State = newState });
            }
        }

        private void SaveDay() {
            if ((DateTime.Now - _lastSaveTime).TotalSeconds < 120) { return; }
            string serializedObject = SensorDayHelper.Serialize(_day);
            GlobalSaverOnlineBucket.Instance.Add(_day.Id, serializedObject, new TimeSpan(180, 0, 0, 0), GlobalSaver.BucketEnum.Gps);
            _lastSaveTime = DateTime.Now;
        }
    }
}