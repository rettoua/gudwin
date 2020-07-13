using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UDP = System.Net.Sockets.UdpClient;

namespace UdpClient {
    public class UdpClientManager {
        //private Socket _UdpSocket;
        private UDP _newSock;

        public UdpClientManager() {
            Start();
        }

        private void Start() {
            var ipep = new IPEndPoint(IPAddress.Parse("213.111.88.49"), 9901);
            _newSock = new UDP(9050);
            _newSock.Connect(ipep);
            Console.WriteLine("Waiting...");
            Console.ReadKey();

            var sender = new IPEndPoint(IPAddress.Any, 0);
            while (true) {
                var bytesToSend = new byte[] { 36, 36, 36, 51, 0, 0, 23, 113, 68, 25, 19, 32, 2, 174, 45, 48, 68, 33, 23, 36, 77, 0, 0, 56, 0, 0, 0, 7, 38, 38, 38 };
                _newSock.Send(bytesToSend, bytesToSend.Length);
                var receivedData = _newSock.Receive(ref sender);
                ReceiveData(receivedData);
                Thread.Sleep(1000);
            }
        }

        private void ReceiveData(byte[] data) {
            string value = Encoding.UTF8.GetString(data);
            Console.WriteLine("Received: {0}", value);
        }
    }
}
