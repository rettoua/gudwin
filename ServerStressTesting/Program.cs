using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Ext.Net.Utilities;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace ServerStressTesting {
    class Program {
        static void Main(string[] args) {

            int count = GetTrackerSimulationCount();
            RunTrackers(count);

            Console.ReadLine();
        }

        private static int GetTrackerSimulationCount() {
            Console.WriteLine("Enter count of tracker simulation");

            string result = Console.ReadLine();
            int number;
            if (int.TryParse(result, out number)) {
                return number;
            }
            Console.WriteLine("Enter count of tracker simulation");
            return GetTrackerSimulationCount();
        }

        private static void RunTrackers(int trackers) {
            //var fs = new FileStream("D:\\out.txt", FileMode.Create);
            //Console.SetOut(new StreamWriter(fs));
            for (int i = 1; i <= trackers; i++) {
                ThreadPool.QueueUserWorkItem(SendDataWithNormalSp, (byte)i);
            }
        }

        private static void SendDataWithNormalSp(object objId) {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                socket.Connect("localhost", 9900);
            } catch (Exception) {
                return;
            }

            var date = DateTime.Now;
            var gp = new Gp { TrackerId = (byte)objId };
            var buffer = new byte[25];
            int iterator = 0;
            while (iterator++ < 10) {
                gp.SendTime = date;
                gp.Speed = gp.Speed == 0 ? 90 : 0;
                gp.Distance = gp.Speed == 0 ? 0 : 25;
                byte[] package = GpToBytes(gp);
                var pac = package.ToList();
                pac.AddRange(package);
                try {
                    socket.Send(package);

                    int receivedBytes = socket.Receive(buffer);
                    Console.WriteLine("[{2}] {0} - Receive bytes: {1}", (byte)objId, receivedBytes, DateTime.Now.ToString("hh:mm:ss"));
                } catch (Exception exception) {
                    Console.WriteLine("[{2}] {0} - ERROR: {1}", (byte)objId, exception.Message, DateTime.Now.ToString("hh:mm:ss"));
                }

                date = date.AddSeconds(1);
                Thread.Sleep(1000);
            }
            SendDataWithNormalSp(objId);
        }

        /// <summary>
        /// just send 3 packages and close connection, and repeats this steps forever
        /// </summary>        
        private static void SendData(object objId) {
            var trackerId = (byte)objId;
            var buffer = new byte[25];
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                socket.Connect("localhost", 9900);
                Console.WriteLine("[{1}] {0} - Connected", trackerId, DateTime.Now.ToString("hh:mm:ss"));
                for (int i = 0; i < 3; i++) {
                    //Console.WriteLine("[{1}] {0} - Sent package", trackerId, DateTime.Now.ToString("hh:mm:ss"));
                    socket.Send(new byte[] { 36, 36, 36, 51, 0, 0, 0, trackerId, 20, 31, 17, 36, 13, 174, 44, 64, 65, 33, 17, 28, 15, 0, 0, 16, 0, 0, 0, 3, 38, 38, 38 });
                    int receivedBytes = socket.Receive(buffer);
                    Console.WriteLine("[{2}] {0} - Receive bytes: {1}", trackerId, receivedBytes, DateTime.Now.ToString("hh:mm:ss"));
                    Thread.Sleep(1000);
                }
                socket.Close();
                //socket.Close();
            } catch (Exception exception) {
                Console.WriteLine("[{2}] {0} - ERROR: {1}", trackerId, exception.Message, DateTime.Now.ToString("hh:mm:ss"));
            }

            SendData(trackerId);
        }

        private static byte[] GpToBytes(Gp gp) {
            gp.SendTime = gp.SendTime.AddHours(-3);
            var buffer = new List<byte>();
            buffer.AddRange(new byte[] { 36, 36, 36 });
            buffer.Add(51);//command number
            buffer.AddRange(BitConverter.GetBytes(gp.TrackerId).Reverse());//tracker identity

            int monthYear = gp.SendTime.Month << 4 ^ (gp.SendTime.Year - 2010);
            buffer.Add((byte)monthYear);// year - month

            int day = gp.SendTime.Day;
            buffer.Add((byte)day);// day

            int hour = 0 ^ gp.SendTime.Hour;
            buffer.Add((byte)hour);//hour

            int minutes = 0 ^ gp.SendTime.Minute;
            buffer.Add((byte)minutes);//minutes

            int seconds = 0 ^ gp.SendTime.Second;
            buffer.Add((byte)seconds);//second


            //int z = (month)6 << 4 ^ (year)4;

            //int z1 = 64;//
            //int year = 64 & 0xf;
            //int month = 64 & 0xf0;

            buffer.AddRange(new byte[] { 174, 44, 64, 65, 33, 17, 28, 15, (byte)gp.Speed, 0, 16, (byte)gp.Distance.Value, 0, 0, 3 });
            buffer.AddRange(new byte[] { 38, 38, 38 });
            return buffer.ToArray();
        }
    }
}