using System;

namespace Smartline.Reporting {
    public class ReportingHelper {
        public static DateTime SetDateToMidnight(DateTime dateTime) {
            dateTime = dateTime.AddHours(23 - dateTime.Hour);
            dateTime = dateTime.AddMinutes(59 - dateTime.Minute);
            dateTime = dateTime.AddSeconds(59 - dateTime.Second);
            return dateTime;
        }
    }
}
