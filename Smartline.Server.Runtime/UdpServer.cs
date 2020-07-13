using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Smartline.Mapping;
using Smartline.Server.Runtime.TransportLayout;

namespace Smartline.Server.Runtime {
    public class UdpServerManager {
        private byte[] _okMessagesBuffer = { 79, 75, 13, 10 };
        private UdpClient _newSock;

        public UdpServerManager() {
            ThreadPool.QueueUserWorkItem(Start);
        }

        private void Start(object p) {
            var ipep = new IPEndPoint(IPAddress.Any, 9902);
            _newSock = new UdpClient(ipep);
            var sender = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                byte[] data = _newSock.Receive(ref sender);
                ReceiveData(sender, data);
            }
        }

        private void ReceiveData(IPEndPoint sender, byte[] data) {
            List<byte[]> packages = GetPackages(data);
            if (packages.Count == 0) { return; }
            _newSock.Send(_okMessagesBuffer, _okMessagesBuffer.Length, sender);
            int trackerId = BottleneckMessageReceiver.Instance.GetTrackerId(packages[0]);

            var tracker = CouchbaseManager.GetTracker<Tracker>(trackerId);
            if (tracker == null) { return; }
            //packages.ForEach(message => BottleneckMessageReceiver.Instance.Add(message, null, tracker.Id));
        }

        private readonly byte[] _startSymbol = { 36, 36, 36 };
        private readonly byte[] _endSymbol = { 38, 38, 38 };

        private List<byte[]> GetPackages(byte[] buffer) {
            var packages = new List<byte[]>();
            int offset = 0;
            int startIndex = buffer.Locate(_startSymbol, offset);
            int endIndex = buffer.Locate(_endSymbol, offset);
            while (startIndex != -1 && endIndex != -1) {
                if (endIndex < startIndex) {
                    offset = startIndex;
                    startIndex = buffer.Locate(_startSymbol, offset);
                    endIndex = buffer.Locate(_endSymbol, offset);
                    continue;
                }
                var newPackage = new byte[endIndex - startIndex - 3];
                Buffer.BlockCopy(buffer, startIndex + 3, newPackage, 0, newPackage.Length);
                packages.Add(newPackage);
                offset = endIndex + 3;
                startIndex = buffer.Locate(_startSymbol, offset);
                endIndex = buffer.Locate(_endSymbol, offset);
            }
            if (offset < buffer.Length - 1) {
                var temporaryBuffer = new byte[buffer.Length - 1 - offset];
                Buffer.BlockCopy(buffer, offset, temporaryBuffer, 0, temporaryBuffer.Length);
            }
            return packages;
        }
    }
}