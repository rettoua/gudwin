using System;
using Newtonsoft.Json;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class ReportAdapted {
        private readonly decimal _consumption;
        public ReportAdapted(ReportFull reportFull, decimal consumption) {
            _consumption = consumption;
            Set(reportFull);
        }

        public ReportAdapted() { }

        public DateTime Date { get; set; }
        [JsonProperty("b")]
        public decimal AvgSpeed { get; set; }
        [JsonProperty("c")]
        public decimal MaxSpeed { get; set; }
        public int? Distance { get; set; }
        public int Parking { get; set; }
        public int Moving { get; set; }

        [JsonProperty("e")]
        public string ParkingStr {
            get { return TimeSpan.FromSeconds(Parking).ToWordsWithoutDays(); }
        }
        public string ParkingStrCommon {
            get { return TimeSpan.FromSeconds(Parking).ToWordsWithoutDaysCommonStyle(); }
        }

        [JsonProperty("f")]
        public string MovingStr {
            get { return TimeSpan.FromSeconds(Moving).ToWordsWithoutDays(); }
        }
        public string MovingStrCommon {
            get { return TimeSpan.FromSeconds(Moving).ToWordsWithoutDaysCommonStyle(); }
        }
        [JsonProperty("d")]
        public string DistanceStr {
            get { return _consumption == 0 ? Distance.ToDistanceInKilometers() : Distance.ToDistanceInKilometersAndConsumption(_consumption); }
        }
        [JsonProperty("g")]
        public string DateStr {
            get { return Date.ToString("dd-MM-yyyy"); }
        }

        public void Set(ReportFull reportFull) {
            Date = reportFull.Date;
            AvgSpeed = reportFull.AvgSpeed;
            MaxSpeed = reportFull.MaxSpeed;
            Distance = reportFull.Distance;
            Parking = reportFull.Parking;
            Moving = reportFull.Moving;
        }
    }
}