using System;
using System.Collections.Generic;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    public partial class statemonitor : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                Download.AddJScriptDynamicaly("monitoring", this);
                Download.AddJScriptDynamicaly("common", this);
            }
        }

        [DirectMethod]
        public static JsonObject LoadOnlineActivity(DateTime dateTime) {
            List<JsonObject> events = CouchbaseManager.LoadMonitoring(dateTime);
            var activity = CouchbaseManager.LoadOnlineActivity();
            var jo = new JsonObject { { "events", events }, { "activity", activity } };
            return jo;
        }
    }
}