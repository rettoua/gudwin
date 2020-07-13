using System;

namespace Smartline.Mapping {
    public class Increments {
        public static ulong GenerateUserId() {
            return CouchbaseManager.Main.Increment("I_User", 1, 1);
        }

        public static ulong GetUserId() {
            return Convert.ToUInt64(CouchbaseManager.Main.Get("I_User"));
        }

        public static ulong GenerateTrackerId() {
            return CouchbaseManager.Main.Increment("I_Tracker", 1, 1);
        }

        public static ulong GetTrackerId() {
            return Convert.ToUInt64(CouchbaseManager.Main.Get("I_Tracker"));
        }

        public static ulong GenerateSensorId() {
            return CouchbaseManager.Main.Increment("I_Sensor", 1, 1);
        }

        public static ulong GetSensorId() {
            return Convert.ToUInt64(CouchbaseManager.Main.Get("I_Sensor"));
        }

        public static ulong GeneratePaymentId() {
            return CouchbaseManager.Main.Increment("I_Payment", 1, 1);
        }

        public static ulong GetPaymentId() {
            return Convert.ToUInt64(CouchbaseManager.Main.Get("I_Payment"));
        }


    }
}