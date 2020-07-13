using System;
using System.Collections.Generic;
using System.Linq;
using Ext.Net;
using Newtonsoft.Json;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class traffic : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                Download.AddJScriptDynamicaly("monitoring", this);
                Download.AddJScriptDynamicaly("common", this);
                LoadAllTrackers();
                SetDefaultValues();
            }
        }

        private void LoadAllTrackers() {
            List<BasicUser<ExtendedTracker>> users = CouchbaseManager.GetUsers<BasicUser<ExtendedTracker>>();
            var trackers = new List<ExtendedTracker>();
            foreach (BasicUser<ExtendedTracker> basicUser in users) {
                basicUser.Trackers.ForEach(o => o.User = basicUser.UserName);
                trackers.AddRange(basicUser.Trackers);
            }           
            Store store = gridAllTrackers.GetStore();            
            int[] onlineTrackers = CouchbaseManager.GetOnlineTrackers();
            trackers.Where(o => onlineTrackers.Contains(o.Id)).ToList().ForEach(o => o.IsOnline = true);
            store.DataSource = trackers;
            store.DataBind();
        }

        private void SetDefaultValues() {
            fldFromDate.Value = DateTime.Now;
            fldToDate.Value = DateTime.Now;
        }

        [DirectMethod]
        public static string LoadTraffic(int[] ids, DateTime from, DateTime to) {
            var traffics = new Dictionary<int, List<Traffic>>();
            foreach (int id in ids) {
                var databaseIds = new List<string>();
                DateTime d = from;
                while (d <= to) {
                    databaseIds.Add(Traffic.GetId(id, d));
                    d = d.AddDays(1);
                }
                traffics[id] = CouchbaseManager.LoadTraffic(databaseIds);
            }
            return JSON.Serialize(traffics);
        }

        private class ExtendedTracker : Tracker {

            public bool IsOnline { get; set; }
            public string User { get; set; }
        }
    }
}