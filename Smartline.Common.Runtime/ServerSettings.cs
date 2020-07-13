using System.Collections.Generic;
using Microsoft.Win32;

namespace Smartline.Common.Runtime {

    public class ServerSettings {
        private const string FieldPrefix = "gudwin_";
        private const string SubKeyName = "GUDWIN";
        private const string IP1Default = "91.145.200.44";
        //private const string IP2Default = "209.126.64.68";

        private static ServerSettings Default {
            get {
                return new ServerSettings {
                    DataBaseIps = new List<string> { IP1Default }
                };
            }
        }

        public List<string> DataBaseIps { get; private set; }

        public static ServerSettings Get() {
            RegistryKey subKey = null;
            try {
                subKey = Registry.CurrentUser.OpenSubKey(SubKeyName) ?? CreateDefault();
                if (subKey == null) {
                    CreateDefault();
                    return Default;
                }
                return CreateSettings(subKey);
            } finally {
                if (subKey != null) {
                    subKey.Close();
                }
            }
        }

        private static ServerSettings CreateSettings(RegistryKey registryKey) {
            var settings = new ServerSettings { DataBaseIps = new List<string>(GetIpFromRegistryKey(registryKey)) };
            return settings;
        }

        private static IEnumerable<string> GetIpFromRegistryKey(RegistryKey registryKey) {
            string[] names = registryKey.GetValueNames();
            foreach (string name in names) {
                if (name.StartsWith(FieldPrefix)) {
                    yield return registryKey.GetValue(name) + "";
                }
            }
        }

        public static RegistryKey CreateDefault() {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(SubKeyName);
            if (subKey == null) { return null; }
            subKey.SetValue(FieldPrefix + "ip1", IP1Default);
            //subKey.SetValue(FieldPrefix + "ip2", IP2Default);
            return subKey;
        }
    }
}