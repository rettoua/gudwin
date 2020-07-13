using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Smartline.Common.Runtime;

namespace Smartline.Server.NMEA {

    public interface IGpsProtocolParser {
        void AddData(byte[] received);
        IEnumerable<Gp> Pop();
    }

    /// <summary>
    /// parser for TK-GPRS protocol based on NMEA 0183 ver 3.0 protocol
    /// </summary>   
    public class TkGprsProtocolParser : IGpsProtocolParser, IDisposable {
        private readonly StringBuilder Buffer = new StringBuilder();

        public void AddData(byte[] received) {
            Buffer.Append(GetStringFromBytes(received));
        }

        public IEnumerable<Gp> Pop() {
            string[] packages = GetPackages();
            if (packages.Length == 0) { yield return null; }
            foreach (string package in packages) {
                yield return ConvertStringToGp(package);
            }
        }

        public void Dispose() {
            Buffer.Clear();
        }

        private string GetStringFromBytes(byte[] received) {
            return Encoding.ASCII.GetString(received);
        }

        private string[] GetPackages() {
            string[] packages = Buffer.ToString().Split(new[] { "<CR><LF>" }, StringSplitOptions.RemoveEmptyEntries);
            return packages;
        }

        private Gp ConvertStringToGp(string source) {
            var gpContainer = new GpContainer(source);
            if (gpContainer.IsValid()) {

            }
            return null;
        }
    }

    internal class GpContainer {
        private readonly string _source;
        private string[] _sourceValues;

        internal Lazy<decimal> Latitude {
            get { return vLatitude; }
        }
        private readonly Lazy<decimal> vLatitude = new Lazy<decimal>(() => 5);

        internal Lazy<decimal> Longitude {
            get { return vLongitude; }
        }
        private readonly Lazy<decimal> vLongitude = new Lazy<decimal>(() => 15);

        internal GpContainer(string source) {
            _source = source;
            SplitSourceByValues();
        }

        private void SplitSourceByValues() {
            _sourceValues = _source.Split(',');
        }

        internal bool IsValid() {
            try {
                if (Latitude.Value <= 0 || Longitude.Value <= 0) { return false; }
            } catch {
                return false;
            }
            return true;
        }

        internal Gp ToGp() {
            if (!IsValid()) { throw new InvalidOperationException("source data is invalid."); }
            var gp = new Gp();
            gp.Latitude = Latitude.Value;
            gp.Longitude = Longitude.Value;
            return gp;
        }

    }

    internal enum TkGprsProtocolEnum {
        SerialNumber = 0,
        AuthorizedNumber = 1,
        UtcTime = 3,
        Status = 4,
        Latitude = 5,
        NSIndicator = 6,
        Longitude = 7,
        WEIndicator = 8,
        Speed = 9,
        Date = 11
    }
}
