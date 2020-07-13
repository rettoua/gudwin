using System;
using System.Web.UI;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class cars : Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {
                Download.AddJScriptDynamicaly("car", this);
                if (Helper.IsDemoUser(Session)) {
                    //SetDemoMode();
                }
            }
        }

        private void SetDemoMode() {
            var selectionModel = gridPanelCars.SelectionModel[0] as RowSelectionModel;
            if (selectionModel == null) { return; }
            selectionModel.Listeners.Select.Handler = string.Empty;
            selectionModel.Listeners.Select.Fn = string.Empty;
            selectionModel.Listeners.Deselect.Handler = string.Empty;
            selectionModel.Listeners.Deselect.Fn = string.Empty;
            //toolbarTabsWrapper.Disable();
            //gridPanelCars.Plugins.Clear();
            ColumnEvos.Editable = false;
        }

        [DirectMethod]
        public static ulong GetNextTrackerId() {
            return Increments.GetTrackerId() + 1;
        }
    }
}