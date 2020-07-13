using System;
using System.Globalization;
using System.ServiceProcess;
using System.Windows.Forms;

namespace Smartline.Common.Runtime {
    public class EntryPoint<TServiceBase, TForm>
        where TServiceBase : ServiceBase
        where TForm : Form {

        public EntryPoint() {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        }

        public void Start(string[] args) {
            ArgumentsHelper.Instance.Set(args);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

            if (!ArgumentsHelper.Instance.RunAsApp) {
                var servicesToRun = new ServiceBase[] { Activator.CreateInstance<TServiceBase>() };
                ServiceBase.Run(servicesToRun);
            } else {
                Application.Run(Activator.CreateInstance<TForm>());
            }
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Logger.Write((Exception)e.ExceptionObject);
        }
    }
}
