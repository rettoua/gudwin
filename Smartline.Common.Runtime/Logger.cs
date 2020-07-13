using System;
using System.Globalization;
using System.IO;

namespace Smartline.Common.Runtime {
    public class Logger {
        private static readonly string
            Path = string.Empty;
        private const string LogsFolderName = "Logs";

        static Logger() {
            Path = string.Format("{0}\\{1}",
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName),
                LogsFolderName);
        }

        private static readonly object LockObject = new object();

        /// <summary>
        /// записать ошибку в журналы ошибок
        /// </summary>
        /// <param name="exception">ошибка для записи</param>
        /// <param name="buffer"> </param>
        public static void Write(Exception exception, byte[] buffer = null) {
            string error;
            if (exception == null)
                error = "NULL EXCEPTION";
            else {
                Exception inner = exception.InnerException;
                error = string.Format(
                    @"DateTime: {0}
                                    Type: {1}
                                    Message: {2}
                                    Source: {3}
                                    Stack trace: {4}",
                    DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    exception.GetType(),
                    exception.Message,
                    exception.Source,
                    exception.StackTrace);
                if (inner != null)
                    error += "Inner exception: " + inner;

            }
            if (buffer != null) {
                error += Environment.NewLine;
                foreach (byte item in buffer) {
                    error += item.ToString(CultureInfo.InvariantCulture) + " ";
                }
                error += Environment.NewLine;
            }
            error += "\r\n=====================================================================\r\n\r\n\r\n";

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            lock (LockObject) {
                File.AppendAllText(
                           System.IO.Path.Combine(Path, String.Format("exception_{0}.log", DateTime.Now.ToLongDateString())), error);
            }
        }

        public static void Write(int l, string s, string s2, string s3, decimal result) {
            string save = string.Format("{0} - {1} - {2} - {3}: {4}", l, s, s2, s3, result);
            lock (LockObject) {
                File.AppendAllText(
                           System.IO.Path.Combine("C:\\", String.Format("latitude_{0}.log", DateTime.Now.ToLongDateString())), save);
            }
        }

        public static void WriteGpNotSaved(DateTime dateTime, int trackerId) {
            string message = string.Format("[{0}] Gp package failed saving. Tracker: {1}{2}", dateTime, trackerId, Environment.NewLine);
            lock (LockObject) {
                File.AppendAllText(
                           System.IO.Path.Combine("C:\\", String.Format("gp_package_{0}.log", DateTime.Now.ToLongDateString())), message);
            }
        }

        private static readonly object LockObjectByte = new object();
        public static void Write(byte[] array) {
            if (array == null || !ArgumentsHelper.Instance.WritePackageData) { return; }
            string result = string.Empty;
            foreach (byte item in array) {
                result += item.ToString(CultureInfo.InvariantCulture) + " ";
            }

            result += string.Format("\r\n{0}------------------------------------------------------------\r\n", DateTime.Now.ToLocalTime());
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            lock (LockObjectByte) {
                File.AppendAllText(
                    System.IO.Path.Combine(Path, String.Format("packages_{0}.log", DateTime.Now.ToLongDateString())), result);
            }
        }

        public static void WriteDemo(byte[] array) {
            if (array == null || !ArgumentsHelper.Instance.WritePackageData) { return; }
            string result = string.Empty;
            foreach (byte item in array) {
                result += item.ToString(CultureInfo.InvariantCulture) + " ";
            }

            result += string.Format("\r\n{0}------------------------------------------------------------\r\n", DateTime.Now.ToLocalTime());
            lock (LockObjectByte) {
                File.AppendAllText(
                    System.IO.Path.Combine(Path, String.Format("packages_6000_{0}.log", DateTime.Now.ToLongDateString())), result);
            }
        }

        public static void WriteIncorrectReceivedData(byte[] buffer, string message) {
            if (!ArgumentsHelper.Instance.WritePackageData) { return; }
            if (buffer == null) {
                buffer = new byte[0];
            }
            string result = string.Empty;
            foreach (byte item in buffer) {
                result += item.ToString(CultureInfo.InvariantCulture) + " ";
            }

            result += string.Format("\r\n{0} $ {1}------------------------------------------------------------\r\n", DateTime.Now.ToLocalTime(), message);
            lock (LockObjectByte) {
                File.AppendAllText(
                    System.IO.Path.Combine(Path, String.Format("incorrect_receive_{0}.log", DateTime.Now.ToLongDateString())), result);
            }
        }

        private static readonly object LockObjectIncorrectId = new object();
        public static void WriteTrackerWithIncorrectId(int trackerId) {
            string result = string.Format("[{0}] - {1}", DateTime.Now, trackerId);

            result += string.Format("\r\n{0}------------------------------------------------------------\r\n", DateTime.Now.ToLocalTime());
            lock (LockObjectIncorrectId) {
                File.AppendAllText(
                    System.IO.Path.Combine(Path, String.Format("incorrect_id_{0}.log", DateTime.Now.ToLongDateString())), result);
            }
        }
    }
}