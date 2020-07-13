using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Ext.Net;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.Package;
using Utilities.localhost;

namespace Utilities {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            //label1.Text = Increments.GetTrackerId() + "";
        }

        private void ButtonRecreateUsersClick(object sender, EventArgs e) {
            var existUsers = CouchbaseManager.GetUsersOld();
            foreach (User existUser in existUsers) {
                if (existUser.Id != 0) {
                    CouchbaseManager.Main.Remove(existUser.UserName);
                    continue;
                }
                var id = Increments.GenerateUserId();
                existUser.Id = id;
                if (CouchbaseManager.SetUser(existUser)) {
                    CouchbaseManager.Main.Remove(existUser.UserName);
                }
            }
        }

        private void ButtonCreateIncrementsClick(object sender, EventArgs e) {
            if (Increments.GetUserId() == 0) {
                Increments.GenerateUserId();
            }
            if (Increments.GetTrackerId() == 0) {
                int trackersCount = Tracker.GetMaxId();
                CouchbaseManager.Main.Increment("I_Tracker", (ulong)trackersCount, 1);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            Increments.GenerateTrackerId();
            label1.Text = Increments.GetTrackerId() + "";
        }

        private GUDWINGisServerService _service;
        private void button2_Click(object sender, EventArgs e) {
            //var user = CouchbaseManager.GetUser(111);
            //user.UserName = "vid813";
            //user.Secret = "FB05ED0BF69B6DCE7373F921E4551D496D4E465E";
            //user.Name = "vid813";
            //CouchbaseManager.SetUser(user);
            //var bytes = new byte[] { 53, 0, 0, 0, 5, 249, 0, 0, 4, 59, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //bytes.ParseSendTime();
            //var result = CouchbaseManager.Gps.GetView<JsonObject>("gps", "gps", true).Descending(true).Limit(50).ToList();
            //var f = new DateTime(2013, 08, 19, 16, 0, 0);
            //var resut = CouchbaseManager.GetLastGp(27);
            //if (_service == null) {
            //    _service = new GUDWINGisServerService();
            //    _service.CookieContainer = new CookieContainer();
            //}
            //_service.ConnectToServer("", "", "");
        }

        private void button3_Click(object sender, EventArgs e) {
            var z27 = CouchbaseManager.Online.Get("27");
            var z506 = CouchbaseManager.Online.Get("506");
        }

        private void button4_Click(object sender, EventArgs e) {
            var from = new DateTime(2014, 11, 6, 14, 0, 0);
            var to = new DateTime(2014, 11, 6, 15, 0, 0);
            List<Gp> gps = CouchbaseManager.LoadTrack<Gp>(300, from, to);
            //CouchbaseManager.Online.Remove("545");
            //ThreadPool.QueueUserWorkItem(LoadOnline);
        }

        private void LoadOnline(object sender) {
            while (true) {
                try {
                    Gp gp = CouchbaseManager.GetLastGp(300);
                    if (gp != null) {
                        ShowTime(gp.GetActualTime());
                    }
                } catch (Exception) {


                }
                Thread.Sleep(1000);
            }
        }

        private void ShowTime(DateTime now) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<DateTime>(ShowTime), now);
                return;
            }
            this.button4.Text = now.ToString(CultureInfo.InvariantCulture);
        }
    }
}