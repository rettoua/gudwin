using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpClient {
    public class Program {
        static void Main(string[] args) {
            //byte[] buffer = BitConverter.GetBytes((Int32)6000);
            new UdpClientManager();
        }
    }
}
