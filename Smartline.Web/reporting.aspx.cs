using System;
using System.IO;
using System.Web.UI;
using Ext.Net;
using Smartline.Mapping;
using Smartline.Server.Runtime.Reports;

namespace Smartline.Web {
    [DirectMethodProxyID(IDMode = DirectMethodProxyIDMode.None)]
    public partial class reporting : Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {                
                LoadCars();
                InitFilterField();
            }
        }
        
        private void LoadCars() {
            Store store = gridPanelCars.GetStore();
            store.DataSource = CouchbaseManager.CarsRepository.GetCarListForUser(Session["user"] as User);
            store.DataBind();
        }

        private void InitFilterField() {
            dfFrom.SelectedDate = DateTime.Now.Date;
            dfTo.SelectedDate = DateTime.Now.Date;
        }

        [DirectMethod]
        public static JsonObject GenerateReport(int reportId, DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan, int[] trackers) {
            switch (reportId) {
                case 1: {
                    return ReportManager.GenerateSummaryReport(from, to, fromTimeSpan, toTimeSpan, trackers);
                }
                case 2: {
                    return ReportManager.GenerateDetailedReport(from, to, fromTimeSpan, toTimeSpan, trackers, ReportManager.GetReportNameByType(ReportTypes.Detail));
                }
                case 3: {
                    return ReportManager.GenerateDetailedReport(from, to, fromTimeSpan, toTimeSpan, trackers, ReportManager.GetReportNameByType(ReportTypes.Parking));
                }
            }
            return null;
        }

        protected void btnExportClick(object sender, DirectEventArgs e) {
            string values = e.ExtraParams["values"];
            var obj = JSON.Deserialize<Values>(values);
            var reportType = (ReportTypes)obj.rt;
            string reportName = ReportManager.GetReportNameByType(reportType);
            MemoryStream stream = null;
            switch (reportType) {
                case ReportTypes.Summary: {
                    stream = ReportManager.ExportSummaryReport(obj.df, obj.dt, obj.tf, obj.tt, obj.trackers, obj.names);
                    break;
                }
                case ReportTypes.Detail: {
                    stream = ReportManager.ExportDetailReport(obj.df, obj.dt, obj.tf, obj.tt, obj.trackers, obj.names);
                    break;
                }
                case ReportTypes.Parking: {
                    stream = ReportManager.ExportParkingReport(obj.df, obj.dt, obj.tf, obj.tt, obj.trackers, obj.names);
                    break;
                }
            }
            if (stream == null) {
                return;
            }
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}.xlsx", reportName));
            stream.WriteTo(Response.OutputStream);
            Response.End();
        }
    }

    internal class Values {
        //df: df, dt: dt, tf: tf, tt: tt, rt: rt, trackers: trackers 
        public DateTime df { get; set; }
        public DateTime dt { get; set; }
        public DateTime tf { get; set; }
        public DateTime tt { get; set; }
        public int[] trackers { get; set; }
        public int rt { get; set; }
        public string[] names { get; set; }
    }
}