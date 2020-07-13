using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime.Package;

namespace Smartline.Web.Log {
    public class FileLogParser {
        private readonly string LogsDirectory = @"C:\gudwin\server_new\Logs";
        private readonly List<byte[]> _packages = new List<byte[]>();
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly int _trackerId = 6002;
        private DateTime _date;

        public FileLogParser(int trackerId, DateTime date) {
            _trackerId = trackerId;
            _date = date;
        }

        public FileLogParser(int trackerId, DateTime date, string logsDirectory) {
            _trackerId = trackerId;
            _date = date;
            LogsDirectory = logsDirectory;
        }

        public string Parse() {
            string filePath = GetFilePath();
            if (filePath == "") {
                return string.Empty;
            }
            string[] result = File.ReadAllLines(filePath);

            foreach (string l in result) {
                if (l.Contains("-----") || String.IsNullOrWhiteSpace(l)) { continue; }
                var line = l.Replace(Environment.NewLine, "");
                byte[] bytes = StringToByteArray(line);
                ParsePackage(bytes);
            }
            foreach (byte[] package in _packages) {
                _stringBuilder.AppendLine(String.Join(" ", package));
            }
            return _stringBuilder.ToString();
        }

        public List<Gp> ParseToGp() {
            var list = new List<Gp>();
            string filePath = GetFilePath();
            if (filePath == "") {
                return list;
            }
            string[] result = File.ReadAllLines(filePath);

            foreach (string l in result) {
                if (l.Contains("-----") || String.IsNullOrWhiteSpace(l)) { continue; }
                var line = l.Replace(Environment.NewLine, "");
                byte[] bytes = StringToByteArray(line);
                ParsePackage(bytes);
            }
            foreach (byte[] package in _packages) {
                list.Add(ProtocolEngine.ParseGpsPackage(package, null));
            }
            return list;
        }

        public void Clear() {
            _stringBuilder.Clear();
            _packages.Clear();
        }

        private string GetFilePath() {
            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            string path = Path.Combine(LogsDirectory, String.Format("packages_{0}.log", _date.ToLongDateString()));

            Thread.CurrentThread.CurrentCulture = culture;
            if (File.Exists(path)) {
                return path;
            }
            return string.Empty;
        }

        private void ParsePackage(byte[] bytes) {
            int trackerId = GetTrackerId(bytes);
            if (trackerId != _trackerId) { return; }
            _packages.Add(bytes);
        }

        private byte[] StringToByteArray(string line) {
            string[] strings = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return (from c in strings
                    select Convert.ToByte(c)).ToArray();
        }

        private int GetTrackerId(IEnumerable<byte> buffer) {
            return BitConverter.ToInt32(buffer.Skip(new ProtocolSpecification().GPS_TRACKERID.NoByteStart).Take(new ProtocolSpecification().GPS_TRACKERID.ByteCnt).Reverse().ToArray(), 0);
        }
    }
}