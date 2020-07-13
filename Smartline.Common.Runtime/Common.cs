using System;
using System.Configuration;

namespace Smartline.Common.Runtime {
    public class Common {
        public const string CommunicationStateManagerCollectionId = "CommunicationStateManagerCollection";
        public static string GetConnectionString() {
            try {
                return ConfigurationManager.AppSettings["ConnectionString"];
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return string.Empty;
        }
    }
}
