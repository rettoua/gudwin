using System;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web.manage {
    public partial class accounting : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                LoadGlobalAccountingSettings();
            }
        }

        private void LoadGlobalAccountingSettings() {
            var settings = GlobalAccountingSettings.Get();
            fldMonthWriteOff.Value = settings.WriteOffPerMonthAmount;
            fldFinancialLock.Value = settings.OffAmount;
        }

        protected void SaveSettingsClick(object sender, DirectEventArgs e) {
            var writeOff = (double)fldMonthWriteOff.Value;
            var financialLock = (double)fldFinancialLock.Value;
            var settings = GlobalAccountingSettings.Get();
            settings.WriteOffPerMonthAmount = writeOff;
            settings.OffAmount = financialLock;
            settings.Save();
            X.Msg.Alert("Сохранение", "Данные успешно сохранены").Show();
        }
    }
}