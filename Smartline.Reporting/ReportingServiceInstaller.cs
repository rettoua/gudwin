using System.ComponentModel;
using System.ServiceProcess;

namespace Smartline.Reporting {
    [RunInstaller(true)]
    public partial class ReportingServiceInstaller : System.Configuration.Install.Installer {
        public ReportingServiceInstaller() {
            InitializeComponent();
            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            var serviceAdmin = new ServiceInstaller {
                StartType = ServiceStartMode.Manual,
                ServiceName = "GUDWIN_REPORTINGsvc",
                DisplayName = "GUDWIN Reporting Service",
                Description = "GUDWIN сервис отчетности"
            };
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
