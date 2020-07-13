using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Couchbase;
using Couchbase.Extensions;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.Package;
using Smartline.Server.Runtime.SignalR;
using Smartline.Server.Runtime.TrackerEngine;
using Smartline.Server.Runtime.TransportLayout;

namespace Smartline.Server {
    public partial class MainForm : Form {
        private ServerDomain _server;
        private AdminForm _adminForm;
        private bool _adminFormShow = false;

        public MainForm() {
            InitializeComponent();
            Shown += MainForm_Shown;

        }

        private static int GetTimeZoneOffset() {
            try {
                return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
            } catch {
                return 2;
            }
        }

        private Gp ParseGpsPackage(byte[] package) {
            try {
                var newGps = new Gp {
                    SendTime = package.ParseSendTime().AddHours(GetTimeZoneOffset()),
                    Latitude = package.ParseLatitude(),
                    Longitude = package.ParseLongitude(),
                    Speed = Math.Round(package.ParseSpeed(), 2),
                    Distance = package.ParseDistance(),
                    //Sensors = package.ParseSensor(tracker),
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

        void MainForm_Shown(object sender, EventArgs e) {


            //var package = new byte[] { 51, 0, 0, 23, 112, 180, 26, 4, 54, 2, 178, 25, 99, 83, 30, 23, 6, 14, 64, 93, 101, 0, 83, 77, 3 };
            //var gp = ParseGpsPackage(package);

            //var z = CouchbaseManager.LoadTrack<Gp>(3, DateTime.Now.Date, DateTime.Now);
            //var limit = 10;



            //List<WebLogging> result2 = CouchbaseManager.GetWebLoggings();

            //List<IViewRow> result = CouchbaseManager.Monitoring.GetView("monitoring", "weblogging").ToList();
            //var items = new List<WebLogging>();
            //foreach (IViewRow viewRow in result) {
            //    object item = viewRow.GetItem();
            //    items.Add(JSON.Deserialize<WebLogging>(item + ""));
            //}
            //var sb = new StringBuilder();
            //foreach (WebLogging webLogging in items) {
            //    sb.AppendLine(string.Format("{0}\t{1}\t{2}", webLogging.ActionTime, webLogging.UserName, webLogging.LoggingAction));
            //}

            //ThreadPool.QueueUserWorkItem(TestOnlineSelection);
        }

        private void TestOnlineSelection(object b) {
            int skip = 0;
            const int take = 100000;
            while (true) {
                Debug.WriteLine("SKIP {0}", skip);
                show(skip);
                List<IViewRow> items = CouchbaseManager.Gps.GetView("gps", "only_gps").Skip(0).Limit(take).Stale(StaleMode.UpdateAfter).ToList();
                //List<Gp> itemsParsed = CouchbaseManager.Gps.GetView<Gp>("gps", "only_gps", true).Skip(skip).Limit(take).ToList();

                if (items.Count == 0) { break; }
                foreach (IViewRow row in items) {
                    //object item = row.GetItem();
                    //if (item == null) { continue; }
                    //Gp gp = JSON.Deserialize<Gp>(item.ToString());
                    //if (gp.TrackerId == 0 || (max - gp.SendTime).TotalDays > 1) {
                    if (!CouchbaseManager.Gps.Remove(row.ItemId)) {

                    }
                    //Debug.WriteLine("DATE {0}", gp.SendTime);
                    //}
                }
                skip += take;
            }


        }


        private void show(int value) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<int>(show), value);
                return;
            }
            label_ReadMemcachedValue.Text = value + "";
        }

        public class Test {
            [JsonProperty("id")]
            public ulong Id1 { get; set; }

            [JsonProperty("aa")]
            public string Text { get; set; }

        }

        protected override void OnClosing(CancelEventArgs e) {
            if (_server != null)
                _server.Stop();
        }

        private void AdminToolStripMenuItemClick(object sender, EventArgs e) {
            if (!_adminFormShow) {
                _adminForm = new AdminForm();
                _adminForm.FormClosing += (x1, x2) => { _adminFormShow = false; this.Show(); };
                _adminFormShow = true;
                Hide();
                _adminForm.Show();
            } else {
                _adminForm.Show();
            }
        }

        private void Start() {
            button_start.Enabled = false;
            button_stop.Enabled = false;
            _server = new ServerDomain(9900);

            _server.Start();
        }

        private void Stop() {
            if (_server != null) {
                _server.Stop();
            }
        }

        private void SetStateAdminButton(bool isRunning) {
            if (IsDisposed) {
                return;
            }
            if (InvokeRequired) {
                Invoke(new Action<bool>(SetStateAdminButton), isRunning);
                return;
            }
            button_start.Enabled = !isRunning;
            button_stop.Enabled = isRunning;
        }

        private void ButtonStartClick(object sender, EventArgs e) {
            Start();
        }

        private void ButtonStopClick(object sender, EventArgs e) {
            Stop();
        }

        private void Button1Click(object sender, EventArgs e) {

            ServerDomain.Working = true;
            GlobalSaver.Instance.Start();
            WebServer.Instance.Start();
            //CommunicationStateManager.Instance.AddIncorrectNumberTracker(25);
            //var packages = new List<IStateManagerPackage>();
            //var package = new StateManagerNewConnection { Date = DateTime.Now, Ip = "10.10.10.25" };
            //var package1 = new StateManagerIncorrectTracker { Date = DateTime.Now, TrackerId = 25 };
            //var package2 = new StateManagerPackage { Date = DateTime.Now, TrackerId = 25, UserName = "test user" };
            //var package3 = new StateManagerTrackerDisconnected { Date = DateTime.Now, TrackerId = 25, UserName = "test user", Reason = "no reason" };
            //packages.Add(package);
            //packages.Add(package1);
            //packages.Add(package2);
            //packages.Add(package3);

            //CouchbaseManager.Online.Store(StoreMode.Set, Common.Runtime.Common.CommunicationStateManagerCollectionId, JSON.Serialize(packages));
            
            //SaveTrackerData(503);
            //new Thread(SaveManyGps).Start();
        }

        private static int? GetBatteryState(int? currentState) {
            switch (currentState) {
                case null:
                return 100;
                case 100:
                return 75;
                case 75:
                return 50;
                case 50:
                return 25;
                case 25:
                return null;
                default:
                return null;
            }
        }

        private void ButtonCreateAdminUserClick(object sender, EventArgs e) {
            var adminUser = new User { UserName = "Administrator" };
            adminUser.Secret = User.ComputeSecretNew(textBox_password.Text);
            CouchbaseManager.SetUser(adminUser);
        }

        private void тестоваяФормочкаToolStripMenuItem_Click(object sender, EventArgs e) {
            //ThreadPool.QueueUserWorkItem(AddMemcachedValues);
            //ThreadPool.QueueUserWorkItem(ReadMemcachedValues);
            new AdminForm(_server).Show(this);
        }

        private void AddMemcachedValues(object value) {
            for (int i = 0; i < 10000; i++) {
                CouchbaseManager.Online.Store(StoreMode.Set, i + "", i);
            }
        }

        private void ReadMemcachedValues(object p) {
            SetValue(0);
            for (int i = 0; i < 10000; i++) {
                var value = CouchbaseManager.Online.Get<int>(i + "");

            }
            SetValue(1);
        }

        private void SetValue(int value) {
            if (InvokeRequired) {
                Invoke(new Action<int>(SetValue), value);
                return;
            }
            label_ReadMemcachedValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void Button2Click(object sender, EventArgs e) {
            BottleneckMessageReceiver.Instance.SendData(6000, new byte[] { 1, 2, 3, 4, 56 });
            //6000
            //_server.SocketListener.SendData();
            //var relay = new RelayAction {
            //    TrackerId = 4,
            //    RelayIndex = 1,
            //    Action = true
            //};
            //RelaySaver.Instance.Add(relay);
        }

        private void Button3Click(object sender, EventArgs e) {
            var relay = new RelayAction {
                TrackerId = 4,
                RelayIndex = 1,
                Action = false
            };
            RelaySaver.Instance.Add(relay);
        }

        private void Button5Click(object sender, EventArgs e) {
            var relay = new RelayAction {
                TrackerId = 4,
                RelayIndex = 2,
                Action = true
            };
            RelaySaver.Instance.Add(relay);
        }

        private void Button4Click(object sender, EventArgs e) {
            var relay = new RelayAction {
                TrackerId = 4,
                RelayIndex = 2,
                Action = false
            };
            RelaySaver.Instance.Add(relay);
        }
    }
}