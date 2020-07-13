using System;

namespace GisServerService {
    public class RoutePoint {
        public int ID { get; set; }
        public decimal Lon { get; set; }
        public decimal Lat { get; set; }
        public string SD { get; set; }
        public string TM { get; set; }
    }
}