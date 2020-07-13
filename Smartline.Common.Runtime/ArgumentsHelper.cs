using System.ComponentModel;
using System.Linq;

namespace Smartline.Common.Runtime {
    public class ArgumentsHelper {
        public static ArgumentsHelper Instance = new ArgumentsHelper();
        public bool RunAsApp { get; set; }
        [DefaultValue(true)]
        public bool WritePackageData { get; set; }

        public string[] Args { get; set; }

        public void Set(string[] args) {
            Args = args;
            RunAsApp = ParameterExist("-app");
            WritePackageData = !ParameterExist("-package");
        }

        private bool ParameterExist(string paramter) {
            return Args.Any(o => o == paramter);
        }
    }
}