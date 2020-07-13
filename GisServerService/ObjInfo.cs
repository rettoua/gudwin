namespace GisServerService {
    public class ObjInfo {
        public string LT { get; set; }
        public string GSM { get; set; }
        public string GPS { get; set; }
        public string BAT { get; set; }
        public string POW { get; set; }

        public ObjInfo() {
            GSM = GPS = BAT = POW = "0";
        }
    }
}