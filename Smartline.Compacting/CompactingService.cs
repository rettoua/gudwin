using System.ServiceProcess;

namespace Smartline.Compacting {
    partial class CompactingService : ServiceBase {
        private readonly CompactingController _compactingController;
        public CompactingService() {
            InitializeComponent();
            ServiceName = "GUDWIN Compacting Service";
            _compactingController = new CompactingController(this);
        }

        protected override void OnStart(string[] args) {
            _compactingController.Start();
        }

        protected override void OnStop() {
            if (_compactingController == null) { return; }
            _compactingController.Stop(false);
        }
    }
}
