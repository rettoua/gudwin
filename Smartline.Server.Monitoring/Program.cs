using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Smartline.Server.Monitoring {
    class Program {
        private static Socket _socket;
        static void Main(string[] args) {
            Console.WriteLine("Enter IP address to connect to server");
            string ipaddress = Console.ReadLine();
            if (Connect(ipaddress)) {
                SuccessConnect();
            } else {
                WrongConnect(ipaddress);
            }
        }

        static void SuccessConnect() {
            Console.WriteLine("Press any key to send package of 6000 tracker");
            Console.ReadLine();
            Send();
            Receive();
            SuccessConnect();
        }

        static void Send() {
            _socket.Send(new byte[] { 36, 36, 36, 51, 0, 0, 23, 112, 131, 17, 19, 13, 5, 178, 27, 44, 40, 30, 24, 51, 28, 0, 63, 95, 0, 14, 0, 7, 38, 38, 38 });
            Console.WriteLine("Package sent");
        }

        static void Receive() {
            var buffer = new byte[800];
            int data = _socket.Receive(buffer);
            IEnumerable<byte> receivedBuffer = buffer.Take(data);
            Console.WriteLine("Received bytes: {0}", string.Join(" ", receivedBuffer));
        }

        static void WrongConnect(string ip) {
            Console.WriteLine("Не удалось подключиться к серверу. Переподключиться? y/n");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.KeyChar == (char)Keys.Y) {
                if (Connect(ip)) {
                    SuccessConnect();
                } else {
                    WrongConnect(ip);
                }
            }
        }

        static bool Connect(string ip) {
            _socket = new Socket(new IPEndPoint(IPAddress.Any, 9900).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try {
                _socket.Connect(ip, 9900);
            } catch (Exception exception) {
                return false;
            }
            return true;
        }
    }
}