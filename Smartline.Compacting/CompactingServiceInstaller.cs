using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Smartline.Compacting {
    [RunInstaller(true)]
    public partial class CompactingServiceInstaller : Installer {
        public CompactingServiceInstaller() {
            InitializeComponent();
            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            var serviceAdmin = new ServiceInstaller {
                StartType = ServiceStartMode.Manual,
                ServiceName = "GUDWIN_COMPACTINGsvc",
                DisplayName = "GUDWIN Compacting Service",
                Description = "GUDWIN сервис обработки GPS пакетов"
            };
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}