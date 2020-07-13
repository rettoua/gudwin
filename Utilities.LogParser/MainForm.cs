using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.Package;

namespace Utilities.LogParser {
    public partial class MainForm : Form {
        private List<Data> _datas = new List<Data>();
        private List<Totals> _totals = new List<Totals>();
        private StringBuilder _html = new StringBuilder();
        private List<Gp> _gps = new List<Gp>();
        private List<byte[]> _packages = new List<byte[]>();
        private int _trackerId = 623;
        public MainForm() {
            InitializeComponent();
        }

        private void Button1Click(object sender, EventArgs e) {
            _packages.Clear();
            _html.Clear();
            //_html.AppendLine("<table>");
            using (var openDirectory = new FolderBrowserDialog()) {
                if (openDirectory.ShowDialog(this) != DialogResult.OK) { return; }
                string[] files = Directory.GetFiles(openDirectory.SelectedPath, "*.log");
                ParseFiles(files);
            }
            //_html.Clear();
            //foreach (byte[] package in _packages) {
            //    _html.AppendLine(string.Join(" ", package));
            //}
            //_html.AppendLine("</table>");
            //foreach (Gp gp in _gps) {
            //    _html.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", gp.SendTime, gp.Latitude, gp.Longitude, gp.Distance, gp.Speed));
            //}

            File.WriteAllText(string.Format("F://log//totals_{0}.txt", _trackerId), _html.ToString());
            MessageBox.Show("OK");
        }

        private void ParseFiles(string[] files) {
            int index = 0;
            foreach (string file in files) {
                _datas.Clear();
                Text = (++index).ToString(CultureInfo.InvariantCulture);
                var fileInfo = new FileInfo(file);
                DateTime modifiedDate = fileInfo.LastWriteTime.Date;
                var stream = new StreamReader(File.OpenRead(file));
                ParseFile(stream, modifiedDate);
                //AddTotals();
            }
            //GenerateHtml();
        }
        string prevLine = null;
        private void ParseFile(StreamReader stream, DateTime dateTime) {
            while (stream.Peek() >= 0) {
                string line = stream.ReadLine();
                if (line.Contains("-----") || string.IsNullOrWhiteSpace(line)) {
                    prevLine = line;
                    continue;
                }
                line = line.Replace(Environment.NewLine, "");
                byte[] bytes = StringToByteArray(line);
                ParsePackage(bytes);
                //ParseBytes(bytes, dateTime);
            }
        }

        private void ParsePackage(byte[] bytes) {
            int trackerId = GetTrackerId(bytes);
            if (trackerId != _trackerId) { return; }
            Gp gp = ParseGpsPackage(bytes);
            if (gp == null) { return; }
            _html.AppendLine(string.Format("{0}: {1}", prevLine, gp.SendTime));
            _html.AppendLine(string.Join(" ", bytes));
            //_packages.Add(bytes);
            //if (bytes[0] != 51) { return; }
            //Gp gp = ParseGpsPackage(bytes);
            //if (gp == null || gp.TrackerId != _trackerId) { return; }
            _gps.Add(gp);
        }

        private Gp ParseGpsPackage(byte[] package) {
            try {
                int trackerid = GetTrackerId(package);
                var newGps = new Gp {
                    TrackerId = trackerid,
                    SendTime = package.ParseSendTime().AddHours(GetTimeZoneOffset()),
                    Latitude = package.ParseLatitude(),
                    Longitude = package.ParseLongitude(),
                    Speed = Math.Round(package.ParseSpeed(), 2),
                    Distance = package.ParseDistance(),
                    Battery = package.ParseBatteryState(),
                    SOS1 = package.ParseSos1(),
                    SOS2 = package.ParseSos2()
                };
                return newGps;
            } catch (Exception exception) {
                Logger.Write(exception, package);
                return null;
            }
        }

        private byte[] StringToByteArray(string line) {
            string[] strings = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return (from c in strings
                    select Convert.ToByte(c)).ToArray();
        }

        private void ParseBytes(byte[] buffer, DateTime dateTime) {
            var data = new Data {
                Date = dateTime,
                PackageType = buffer[0],
                TrackerId = GetTrackerId(buffer)
            };
            if (data.PackageType == 51) {
                data.IsMoving = IsMoving(buffer);
            }
            _datas.Add(data);
        }

        private bool IsMoving(byte[] package) {
            return Math.Round(package.ParseSpeed(), 2) > 0 &&
                   package.ParseDistance() > 0;
        }

        private int GetTrackerId(IEnumerable<byte> buffer) {
            return BitConverter.ToInt32(buffer.Skip(new ProtocolSpecification().GPS_TRACKERID.NoByteStart).Take(new ProtocolSpecification().GPS_TRACKERID.ByteCnt).Reverse().ToArray(), 0);
        }

        private int GetTimeZoneOffset() {
            try {
                return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
            } catch {
                return 2;
            }
        }

        private void AddTotals() {
            var dates = _datas.Select(o => o.Date).Distinct().OrderBy(o => o).ToList();
            var trackerIds = _datas.Select(o => o.TrackerId).Distinct().OrderBy(o => o).ToList();
            foreach (DateTime dateTime in dates) {
                var totals = new Totals { Date = dateTime };
                foreach (int trackerId in trackerIds) {
                    List<Data> packages = _datas.Where(o => o.TrackerId == trackerId && o.Date == dateTime).ToList();
                    var totalItems = new TotalItems {
                        TrackerId = trackerId,
                        Package48Count = packages.Count(o => o.PackageType == 48),
                        Package51MovingCount = packages.Count(o => o.PackageType == 51 && o.IsMoving),
                        Package51ParkingCount = packages.Count(o => o.PackageType == 51 && !o.IsMoving),
                        Package53Count = packages.Count(o => o.PackageType == 53),
                        TotalCount = packages.Count()
                    };
                    totals.Items.Add(totalItems);
                }
                _totals.Add(totals);
            }
            //GenerateHtml(totals);
        }

        private void GenerateHtml() {
            IOrderedEnumerable<DateTime> dates = _totals.Select(o => o.Date).Distinct().OrderBy(o => o);
            var trackers = new List<int>();
            foreach (Totals totalse in _totals) {
                foreach (TotalItems items in totalse.Items) {
                    if (!trackers.Contains(items.TrackerId)) {
                        trackers.Add(items.TrackerId);
                    }
                }
            }
            trackers.Sort();
            _html.AppendLine("<tr><td>Трекер</td>");
            foreach (DateTime dateTime in dates) {
                _html.AppendLine(string.Format("<td>{0}</td>", dateTime.ToString("dd-MM-yyyy")));
            }
            _html.AppendLine("</tr>");
            foreach (int tracker in trackers) {
                _html.AppendLine(string.Format("<tr><td>{0}</td>", tracker));
                foreach (DateTime dateTime in dates) {
                    Totals total = _totals.FirstOrDefault(o => o.Date == dateTime);
                    TotalItems tr = total.Items.FirstOrDefault(o => o.TrackerId == tracker);
                    if (tr == null) {
                        _html.AppendLine("<td></td>");
                    } else {
                        _html.AppendLine(string.Format("<td>{0}</td>", tr.TotalCount));
                    }
                }
                _html.AppendLine("</tr>");
            }
            //foreach (DateTime dateTime in dates) {
            //    Totals total = _totals.FirstOrDefault(o => o.Date == dateTime);
            //    foreach (int tracker in trackers)
            //    {
            //        var tr = total.Items.Any(o => o.TrackerId == tracker);
            //        if () {

            //        }
            //    }
            //}
            //foreach (TotalItems totalse in totals.Items) {
            //    _html.AppendLine("<tr>");
            //    _html.AppendLine(
            //        string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td>",
            //                      totals.Date.ToShortDateString(), totalse.TrackerId, totalse.TotalCount, totalse.Package48Count, totalse.Package51MovingCount, totalse.Package51ParkingCount, totalse.Package53Count));
            //    _html.AppendLine("</tr>");
            //}
        }

        class Data {
            public int TrackerId { get; set; }
            public DateTime Date { get; set; }
            public int PackageType { get; set; }
            public bool IsMoving { get; set; }
        }

        class Totals {
            public Totals() {
                Items = new List<TotalItems>();
            }
            public DateTime Date { get; set; }
            public List<TotalItems> Items { get; set; }
        }

        class TotalItems {
            public int TrackerId { get; set; }
            public int Package48Count { get; set; }
            public int Package51MovingCount { get; set; }
            public int Package51ParkingCount { get; set; }
            public int Package53Count { get; set; }
            public int TotalCount { get; set; }
        }

        private void button2_Click(object sender, EventArgs e) {
            var gpsday = CouchbaseManager.GetGpsDay("06_11_2014_549");
            //var s = string.Format("{0:00}", 5);

            //var pe = new ProtocolEngine();
            //var stream = new StreamReader(File.OpenRead(@"F:\exception_2 июня 2014 г..log\packages_3 июля 2014 г..log"));
            //while (stream.Peek() >= 0) {
            //    string line = stream.ReadLine();
            //    if (line.Contains("------") || string.IsNullOrWhiteSpace(line)) { continue; }
            //    line = line.Replace(Environment.NewLine, "");
            //    byte[] bytes = StringToByteArray(line);
            //    int trackerId = GetTrackerId(bytes);
            //    if (trackerId == 106) {
            //        var gp = pe.ParseGpsPackage(bytes);
            //        System.Diagnostics.Debug.WriteLine(gp.SendTime + "  " + line);
            //    }
            //    //pe.Parse(new TemporaryForIncomingPackages() { Buffer = bytes });
            //}
        }
    }

}