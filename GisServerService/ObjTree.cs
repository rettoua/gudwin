using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace GisServerService {
    public class ObjTree {
        public int ID { get; set; }
        public decimal Lon { get; set; }
        public decimal Lat { get; set; }
        public decimal Direction { get; set; }
        public string name { get; set; }
        public string group { get; set; }
        public string image { get; set; }

        public ObjTree() {
        }

        public ObjTree(Tracker tracker, Gp gp) {
            ID = tracker.TrackerId;
            if (gp != null) {
                Lon = gp.Longitude;
                Lat = gp.Latitude;
                image = gp.Speed == 0 ? "blue" : "norm";
            } else {
                image = "no_signal";
            }
            name = tracker.Name;
        }

        public ObjTree(Tracker tracker, Gp gp, bool sss) {
            ID = tracker.V_TrackerId;
            if (gp != null) {
                Lon = gp.Longitude;
                Lat = gp.Latitude;
                image = gp.Speed == 0 ? "blue" : "norm";
            } else {
                image = "no_signal";
            }
            name = tracker.V_Name;
        }
    }
}