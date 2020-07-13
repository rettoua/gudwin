using System;
using System.Management;
using System.Text;

namespace Smartline.License.Common {
    public class ComputerInfo {
        private static long? _codePc;

        public static long GetCodePc() {
            if (_codePc == null) {
                return
                    (_codePc = Math.Abs(string.Format("{0}{1}", GetCodeMainboard(), GetCodeProcessor()).GetHashCode()))
                        .Value;
            }
            return _codePc.Value;
        }

        private static string GetCodeMainboard() {
            var mbs = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
            var sb = new StringBuilder();
            foreach (ManagementObject mo in mbs.Get()) {
                sb.Append(mo.GetPropertyValue("Product"));
            }
            return sb.ToString();
        }

        private static string GetCodeProcessor() {
            var mbs = new ManagementObjectSearcher("Select * From Win32_Processor");
            var sb = new StringBuilder();
            foreach (ManagementObject mo in mbs.Get()) {
                sb.Append(mo.GetPropertyValue("ProcessorId"));
                sb.Append(mo.GetPropertyValue("Name"));
            }
            return sb.ToString();
        }
    }
}