using System;
using System.Globalization;

namespace Smartline.Common.Runtime {
    public static class Extensions {
        public static string ToWordsWithoutDays(this TimeSpan now) {
            return string.Format("{0:00} ч. {1:00} м. {2:00} с.", now.Hours, now.Minutes, now.Seconds);
        }
        public static string ToWordsWithoutDaysCommonStyle(this TimeSpan now) {
            return string.Format("{0:00}:{1:00}:{2:00}", now.Hours, now.Minutes, now.Seconds);
        }
        public static string ToWordsWithDays(this int ticks) {
            var dt = TimeSpan.FromSeconds(ticks);
            if (dt.TotalDays >= 1) {
                return string.Format("{3} д. {0:00} ч. {1:00} м. {2:00} с.", dt.Hours, dt.Minutes, dt.Seconds, (int)dt.TotalDays);
            }
            return string.Format("{0:00} ч. {1:00} м. {2:00} с.", dt.Hours, dt.Minutes, dt.Seconds);
        }
        public static string ToDistanceInKilometers(this int meters) {
            return Math.Round(meters / 1000m, 2).ToString(CultureInfo.InvariantCulture);
        }
        public static string ToDistanceInKilometers(this int? meters) {
            var m = meters ?? 0;
            return Math.Round(m / 1000m, 2).ToString(CultureInfo.InvariantCulture);
        }
        public static string ToDistanceInKilometersAndConsumption(this int? meters, decimal consumption) {
            int km = meters ?? 0;
            return ToDistanceInKilometersAndConsumption(km, consumption);
        }
        public static string ToDistanceInKilometersAndConsumption(this int meters, decimal consumption) {
            decimal km = meters / 1000m;
            string consumptionStr = Math.Round((consumption * km) / 100m, 1).ToString(CultureInfo.InvariantCulture);
            return string.Format("{0} ({1})", Math.Round(km, 2).ToString(CultureInfo.InvariantCulture), consumptionStr);
        }
        public static string ToFullDateTime(this DateTime now) {
            return now.ToString("dd-MM-yyyy HH:mm");
        }
        public static string ToTimeDateTime(this TimeSpan now) {
            return now.ToString("hh\\:mm\\:ss");
        }
        public static string ToTimeDateTime(this DateTime now) {
            return now.ToString("HH:mm:ss");
        }
        public static string ToTimeDateTime(this DateTime? now) {
            if (now == null) { return string.Empty; }
            return now.Value.ToTimeDateTime();
        }
        public static string ToFullDateTime(this DateTime? now) {
            if (now == null) { return string.Empty; }
            return now.Value.ToFullDateTime();
        }
    }
}