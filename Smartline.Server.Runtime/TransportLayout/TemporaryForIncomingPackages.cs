using System.Net.Sockets;

namespace Smartline.Server.Runtime.TransportLayout
{
    public class TemporaryForIncomingPackages {
        public byte[] Buffer { get; set; }
        public Socket Socket { get; set; }
        public int TrackerId { get; set; }
    }
}