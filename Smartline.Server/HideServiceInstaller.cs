using System.ComponentModel;
using System.ServiceProcess;

namespace Smartline.Server {
    [RunInstaller(true)]
    public class HideServiceInstaller : System.Configuration.Install.Installer {
        public HideServiceInstaller() {
            var process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            var serviceAdmin = new ServiceInstaller();
            serviceAdmin.StartType = ServiceStartMode.Manual;
            serviceAdmin.ServiceName = "GUDWINsvc";
            serviceAdmin.DisplayName = "GUDWIN Tracker Service";
            serviceAdmin.Description = "GUDWIN сервер подключения трекеров";
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
