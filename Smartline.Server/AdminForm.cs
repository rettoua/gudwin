using System;
using System.Windows.Forms;
using Couchbase;
using Couchbase.Configuration;
using Smartline.Server.Runtime;

namespace Smartline.Server {
    public partial class AdminForm : Form {
        private readonly ServerDomain _serverDomain;
        public AdminForm() {
            InitializeComponent();

        }

        public AdminForm(ServerDomain serverDomain)
            : this() {
            //_serverDomain = serverDomain;
            //_serverDomain.ServerSocket.Started += ServerSocketStarted;
            //_serverDomain.ServerSocket.Stopped += ServerSocketStopped;
            //var thead = new Thread(CheckState) { IsBackground = true };
            //thead.Start();
        }


        private void ButtonStartClick(object sender, EventArgs e) {
            _serverDomain.BlockedTrackerId = -1;
        }

        private void button2_Click(object sender, EventArgs e) {
            _serverDomain.BlockedTrackerId = Convert.ToInt32(numericUpDown1.Value);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) {
           
        }

        private CouchbaseClient client;
        private void button3_Click(object sender, EventArgs e) {
            if (client == null) {
                client = new CouchbaseClient(CreateClientConfiguration("main", "main"));
            }

            client.Increment("I_Tracker", 1, 1);
            button3.Text = client.Get("I_Tracker") + "";
        }

        private static CouchbaseClientConfiguration CreateClientConfiguration(string bucket, string bucketPassword) {
            var couchbaseClientConfiguration = new CouchbaseClientConfiguration();
            //couchbaseClientConfiguration.Urls.Add(new Uri("http://localhost:8091/pools"));
            couchbaseClientConfiguration.Urls.Add(new Uri("http://gps-gudwin.com:8091/pools"));
            couchbaseClientConfiguration.Bucket = bucket;
            couchbaseClientConfiguration.BucketPassword = bucketPassword;
            return couchbaseClientConfiguration;
        }
    }
}