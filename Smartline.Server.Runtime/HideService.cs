namespace Smartline.Server.Runtime {
    public class HideService : System.ServiceProcess.ServiceBase {
        private ServerDomain _server;
        public HideService() {
            ServiceName = "GUDWIN Server Service multiports";
            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
        /// <SUMMARY>
        /// Set things in motion so your service can do its work.
        /// </SUMMARY>
        protected override void OnStart(string[] args) {
            _server = new ServerDomain(9900);
            _server.Start();
            base.OnStart(args);
        }

        /// <SUMMARY>
        /// Stop this service.
        /// </SUMMARY>
        protected override void OnStop() {
            _server.Stop();
            base.OnStop();
        }
    }
}
