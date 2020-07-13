using System.ServiceProcess;

namespace Smartline.Reporting {
    partial class ReportingService : ServiceBase {
        private readonly ReportingController _reportingController;
        public ReportingService() {
            InitializeComponent();
            ServiceName = "GUDWIN Reporting Service";
            _reportingController = new ReportingController(this);
        }

        protected override void OnStart(string[] args) {
            _reportingController.Start();
        }

        protected override void OnStop() {
            if (_reportingController == null) { return; }
            _reportingController.Stop(false);
        }
    }
}