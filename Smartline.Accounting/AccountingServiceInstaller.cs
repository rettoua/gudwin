using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Smartline.Accounting {
    [RunInstaller(true)]
    public partial class AccountingServiceInstaller : Installer {
        public AccountingServiceInstaller() {
            InitializeComponent();
            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            var serviceAdmin = new ServiceInstaller {
                StartType = ServiceStartMode.Manual,
                ServiceName = "GUDWIN_ACCOUNTINGsvc",
                DisplayName = "GUDWIN Accounting Service",
                Description = "GUDWIN сервис обработки учетности"
            };
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
