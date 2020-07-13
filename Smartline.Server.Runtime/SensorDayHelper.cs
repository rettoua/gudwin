using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime {
    public class SensorDayHelper {
        public static SensorsDay GetOrCreate(SensorsDay sensorsDay, Gp gp) {
            if (sensorsDay == null) {
                SensorsDay newSensorsDay = Create(gp);
                SensorsDay sensorsDayFromDb = CouchbaseManager.GetSensorsDay(newSensorsDay.Id);
                return sensorsDayFromDb ?? newSensorsDay;
            }

            return Create(gp);
        }

        public static bool IsOkSensorsDay(SensorsDay sensorsDay, Gp gp) {
            if (sensorsDay == null) { return false; }
            return sensorsDay.Date.Day == gp.GetActualTime().Day;
        }

        public static string Serialize(SensorsDay sensorsDay) {
            string value;
            lock (sensorsDay) {
                value = JSON.Serialize(sensorsDay);
            }
            return value;
        }

        public static SensorsDay Create(Gp gp) {
            return new SensorsDay {
                Date = gp.GetActualTime(),
                TrackerId = gp.TrackerId
            };
        }
    }
}
