using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpServer {
    public class UdpServerManager {
        //private Socket _UdpSocket;
        private UdpClient _newSock;


        public UdpServerManager() {
            Start();
        }

        private void Start() {
            var ipep = new IPEndPoint(IPAddress.Any, 9050);
            _newSock = new UdpClient(ipep);
            Console.WriteLine("Waiting for a client...");

            var sender = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                byte[] data = _newSock.Receive(ref sender);
                ReceiveData(sender, data);
            }
        }

        private void ReceiveData(IPEndPoint sender, byte[] data) {
            string value = Encoding.UTF8.GetString(data);
            Console.WriteLine("Received: {0}", value);
            string valueToSend = string.Format("send to client: {0}", value);
            byte[] dataToSend = Encoding.UTF8.GetBytes(valueToSend);
            _newSock.Send(dataToSend, dataToSend.Length, sender);
        }

        ///// <summary>
        ///// Initialize socket 
        ///// </summary>
        //private void InitializeUdpSocket() {
        //    _UdpSocket = new Socket(AddressFamily.InterNetwork,
        //    SocketType.Dgram, ProtocolType.Udp);
        //}

        //public void StartListener() {
        //    //Assign the any IP of the machine and listen on port number 52200
        //    //_ReceiveByteData = new byte[1072];
        //    _UdpSocket.Bind(new IPEndPoint(IPAddress.Any, 99999));
        //    EndPoint newClientEp = new IPEndPoint(IPAddress.Any, 0);
        //    //Start receiving data
        //    _UdpSocket.BeginReceiveFrom(_ReceiveByteData, 0, _ReceiveByteData.Length,
        //    SocketFlags.None, ref newClientEp, DoReceiveFrom, _UdpSocket);
        //    //if (UpdateUI != null) {
        //    //    UpdateUI(CommonFunctions.GetDateTime() + " Server Stated....");
        //    //}
        //}

        //private void DoReceiveFrom(IAsyncResult ar) {
        //    try {
        //        var recvSock = (Socket)ar.AsyncState;
        //        EndPoint clientEp = new IPEndPoint(IPAddress.Any, 0);
        //        int msgLen = recvSock.EndReceiveFrom(ar, ref clientEp);
        //        byte[] objbuffer = new byte[msgLen];
        //        Array.Copy(_ReceiveByteData, objbuffer, msgLen);
        //        _ReceiveByteData = null;
        //        //notification of the data on UI
        //        //if (UpdateUI != null)
        //        //{
        //        //    UpdateUI(objbuffer);
        //        //}
        //        _SendByteData = ProcessAlarmData(objbuffer, clientEp);
        //        SendData(clientEp, _SendByteData);
        //    }
        //}
    }
}