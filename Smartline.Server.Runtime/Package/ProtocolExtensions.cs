using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Package {
    /// <summary>
    /// all the bitwise logic should be take into account that parse is like little-endian (from right to left)
    /// </summary>
    public static class ProtocolExtensions {
        private const int YEAR_SHIFT = 5;
        private const int MONTH_SHIFT = 5;
        private const int DAY_SHIFT = 6;
        private const int HOURS_SHIFT = 7;
        private const int MINUTES_SHIFT = 8;
        private const int SECONDS_SHIFT = 9;
        private const int LATITUDE1_SHIFT = 10;
        private const int LATITUDE2_SHIFT = 11;
        private const int LATITUDE3_SHIFT = 12;
        private const int LATITUDE4_SHIFT = 13;
        private const int LONGITUDE1_SHIFT = 14;
        private const int LONGITUDE2_SHIFT = 15;
        private const int LONGITUDE3_SHIFT = 16;
        private const int LONGITUDE4_SHIFT = 17;
        private const int SPEED1_SHIFT = 18;
        private const int SPEED2_SHIFT = 19;
        private const int DISTANCE1_SHIFT = 21;

        private static readonly int MASK_5_8 = Convert.ToInt32("00001111", 2);//    nulled first 4 bits
        private static readonly int MASK_4_8 = Convert.ToInt32("00011111", 2);//    nulled first 3 bits            
        private static readonly int MASK_3_8 = Convert.ToInt32("00111111", 2);//    nulled first 2 bits    
        private static readonly int MASK_2_8 = Convert.ToInt32("01111111", 2);//    nulled first 1 bits    

        public static DateTime ParseSendTime(this byte[] package) {
            int year = 2020 + (package[YEAR_SHIFT] & MASK_5_8);
            int month = package[MONTH_SHIFT] >> 4;
            int day = package[DAY_SHIFT] & MASK_4_8;
            int hours = package[HOURS_SHIFT] & MASK_4_8;
            int minutes = package[MINUTES_SHIFT] & MASK_3_8;
            int seconds = package[SECONDS_SHIFT] & MASK_3_8;
            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        public static decimal ParseLatitude(this byte[] package) {
            int beforeComma = package[LATITUDE1_SHIFT] & MASK_2_8;
            int afterComma1 = package[LATITUDE2_SHIFT] & MASK_2_8;
            int afterComma2 = package[LATITUDE3_SHIFT] & MASK_2_8;
            int afterComma3 = package[LATITUDE4_SHIFT] & MASK_2_8;

            decimal afterComma = Convert.ToDecimal(string.Format("{0:00}{3}{1:00}{2:00}", afterComma1, afterComma2, afterComma3, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            decimal latitude = Math.Round(beforeComma + afterComma / 60m, 5);//as far as I remember this logic needs in order to presend coordinates in google way. TODO: check this logic            
            return latitude;
        }

        public static decimal ParseLongitude(this byte[] package) {
            int beforeComma = package[LONGITUDE1_SHIFT];
            int afterComma1 = package[LONGITUDE2_SHIFT] & MASK_2_8;
            int afterComma2 = package[LONGITUDE3_SHIFT] & MASK_2_8;
            int afterComma3 = package[LONGITUDE4_SHIFT] & MASK_2_8;

            decimal secondArg = Convert.ToDecimal(string.Format("{0:00}{3}{1:00}{2:00}", afterComma1, afterComma2, afterComma3, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            decimal longitude = Math.Round(beforeComma + secondArg / 60m, 5);
            return longitude;
        }

        public static decimal ParseSpeed(this byte[] package) {
            int beforeComma = package[SPEED1_SHIFT];
            int afterComma = package[SPEED2_SHIFT] & MASK_2_8;
            decimal speed = Convert.ToDecimal(string.Format("{0}{2}{1}", beforeComma, afterComma, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            return speed;
        }

        public static int ParseSos1(this byte[] package) {
            return ((package[LONGITUDE2_SHIFT] >> 7) & 1);
        }

        public static int ParseSos2(this byte[] package) {
            return ((package[LONGITUDE3_SHIFT] >> 7) & 1);
        }

        public static UInt16 ParseDistance(this byte[] package) {
            try {
                return BitConverter.ToUInt16(package.Skip(DISTANCE1_SHIFT).Take(2).Reverse().ToArray(), 0);
            } catch (Exception exception) {
                Logger.Write(exception);
                return 0;
            }
        }

        public static Sensors ParseSensor(this byte[] package, Tracker tracker) {
            if (tracker == null) { return new Sensors(); }
            var sensors = new Sensors();
            byte sensorByte = package[24];//offset of 24 bytes - it byte for sensors' values
            var bitArray = new BitArray(new[] { sensorByte });
            sensors.Sensor1 = bitArray.Get(0);
            sensors.Sensor2 = bitArray.Get(1);
            sensors.Relay1 = bitArray.Get(2);
            sensors.Relay2 = bitArray.Get(3);
            sensors.Relay = bitArray.Get(4);
            return sensors;
        }


        public static int? ParseBatteryState(this byte[] package) {
            byte analogueByte = package[24];//offset of 24 bytes - it byte for sensors' values
            bool powerFromBattery = ((analogueByte >> 7) & 1) == 1;
            if (powerFromBattery) {
                string powerCode = ((analogueByte >> 6) & 1) + "" + ((analogueByte >> 5) & 1);
                return GetPercentageOfBatteryPower(powerCode);
            }
            return null;
        }

        private static int GetPercentageOfBatteryPower(string code) {
            switch (code) {
                case "11":
                return 100;
                case "10":
                return 75;
                case "01":
                return 50;
                case "00":
                return 25;
                default:
                return -1;
            }
        }
    }
}
