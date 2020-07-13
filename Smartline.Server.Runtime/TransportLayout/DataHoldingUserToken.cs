using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.Monitoring;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Server.Runtime.TransportLayout {
    public sealed class DataHoldingUserToken {
        private readonly Action<Tracker> _addTrackerAction;
        private readonly Action<Tracker> _removeTrackerAction;
        private Traffic _temporaryTraffic;
        private readonly StatisticController _statisticController;
        private readonly SocketListener _socketListener;
        private byte[] _okMessage = new byte[] { 79, 75, 13, 10 };

        internal int Incrementor = 0;

        internal Int32 SocketHandleNumber;

        internal byte[] LocalBuffer = null;

        internal readonly Int32 BufferOffsetReceive;
        internal readonly Int32 BufferOffsetSend;
        internal DateTime LastReceiveTime = DateTime.Now;

        private readonly Int32 _idOfThisObject; //for testing only        
        //The session ID correlates with all the data sent in a connected session.
        //It is different from the transmission ID in the DataHolder, which relates
        //to one TCP message. A connected session could have many messages, if you
        //set up your app to allow it.
        private Int32 _sessionId;
        //receiveMessageOffset is used to mark the byte position where the message
        //begins in the receive buffer. This value can sometimes be out of
        //bounds for the data stream just received. But, if it is out of bounds, the 
        //code will not access it.
        internal Byte[] ByteArrayForPrefix;

        //This variable will be needed to calculate the value of the
        //receiveMessageOffset variable in one situation. Notice that the
        //name is similar but the usage is different from the variable
        //receiveSendToken.receivePrefixBytesDone.
        internal Int32 SendBytesRemainingCount;
        internal readonly Int32 SendPrefixLength;
        internal Byte[] DataToSend;
        internal Int32 BytesSentAlreadyCount;
        internal Socket Socket;
        internal Int32 LengthOfCurrentIncomingMessage;

        public bool IsInitialized { get; set; }

        internal bool ShouldReleaseAfterUsing { get; private set; }

        internal Queue<byte[]> SendBuffer { get; set; }

        internal bool HasDataForSend { get { return SendBuffer.Count > 0; } }

        internal Tracker Tracker { get; set; }

        internal User User { get; set; }

        internal int TrackerUId { get { return Tracker == null ? 0 : Tracker.Id; } }

        internal int TrackerId { get { return Tracker == null ? 0 : Tracker.TrackerId; } }

        internal SocketAsyncEventArgs Saea { get; set; }

        //Let's use an ID for this object during testing, just so we can see what
        //is happening better if we want to.
        public Int32 TokenId { get { return _idOfThisObject; } }

        public Int32 SessionId { get { return _sessionId; } }

        private Traffic TemporaryTraffic {
            get { return _temporaryTraffic ?? (_temporaryTraffic = new Traffic { Date = DateTime.Now.Date }); }
        }

        internal bool IsClearing { get; set; }

        internal bool IsReleased { get; set; }

        public DataHoldingUserToken(Action<Tracker> addTrackerAction, Action<Tracker> removeTrackerAction, Int32 rOffset, Int32 sOffset, Int32 identifier, StatisticController controller, SocketListener socketListener) {
            _idOfThisObject = identifier;
            //_addTrackerAction = addTrackerAction;
            //_removeTrackerAction = removeTrackerAction;
            _socketListener = socketListener;

            //Create a Mediator that has a reference to the SAEA object.
            BufferOffsetReceive = rOffset;
            BufferOffsetSend = sOffset;
            _temporaryTraffic = new Traffic();
            _statisticController = controller;
            SendBuffer = new Queue<byte[]>();
        }

        //Used to create sessionId variable in DataHoldingUserToken.
        //Called in ProcessAccept().
        internal void CreateSessionId() {
            _sessionId = Interlocked.Increment(ref _socketListener.MainSessionId);
            IsClearing = false;
            IsReleased = false;
            LastReceiveTime = DateTime.Now;
        }

        private void SetAuth(Tracker tracker) {
            IsInitialized = true;
            Tracker = tracker;
            //User = user;
            //_socketListener.AddInitializedConnection(this);
            //_addTrackerAction(tracker);
            //_statisticController.AddInitializedTracker(tracker.Id, TemporaryTraffic);
            ResetTrafific();
        }

        private void ResetTrafific() {
            if (_temporaryTraffic != null) {
                _temporaryTraffic.Packages.Clear();
            }
            _temporaryTraffic = null;
        }

        internal DataHoldingUserToken Copy() {
            var copy = new DataHoldingUserToken(_addTrackerAction, _removeTrackerAction, BufferOffsetReceive,
                                                BufferOffsetSend, _idOfThisObject,
                                                _statisticController, _socketListener) {
                                                    Tracker = Tracker,
                                                    User = User,
                                                    ShouldReleaseAfterUsing = true,
                                                    Socket = Socket
                                                };
            return copy;
        }

        internal void Reset() {
            Socket = null;
            ShouldReleaseAfterUsing = false;
            SendBuffer.Clear();
            LocalBuffer = null;
            DataToSend = new byte[0];
            ResetTrafific();
            ResetUser();
            IsClearing = false;
            IsReleased = true;
        }

        internal void ResetUser() {
            if (Tracker != null) {
                //_removeTrackerAction(Tracker);
            }
            Tracker = null;
            User = null;
            IsInitialized = false;
            ResetTrafific();
        }

        private bool Initialize(IEnumerable<byte> package) {
            int trackerId = BottleneckMessageReceiver.Instance.GetTrackerId(package);
            var tracker = CouchbaseManager.GetTracker<Tracker>(trackerId);
            if (tracker == null) { return false; }
            //User user = CouchbaseManager.GetUserNameOfTracker(tracker.TrackerId);
            //if (user == null) { return false; }
            SetAuth(tracker);
            try {
                //CommunicationStateManager.Instance.AddTrackerConnected(trackerId, user.UserName, Socket.RemoteEndPoint.ToString());
            } catch (Exception) { }
            return true;
        }

        public bool ContaincePackage(Int32 offset) {
            try {
                int startIndex = LocalBuffer.Locate(_startSymbol, offset);
                int endIndex = LocalBuffer.Locate(_endSymbol, offset);
                if (startIndex == -1 || endIndex == -1) { return false; }
                if (endIndex < startIndex) {
                    offset = endIndex + 3;
                    return ContaincePackage(offset);
                }
                return startIndex != -1 && endIndex != -1;
            } catch (Exception exception) {
                Logger.Write(exception, LocalBuffer);
                return false;
            }
        }

        private readonly byte[] _startSymbol = { 36, 36, 36 };
        private readonly byte[] _endSymbol = { 38, 38, 38 };

        internal void PrepareDataForSend() {
            if (!HasDataForSend) { return; }
            DataToSend = SendBuffer.Dequeue();
            SendBytesRemainingCount = DataToSend.Length;
            BytesSentAlreadyCount = 0;
        }

        internal void AddMessagetoBuffer() {
            SendBuffer.Enqueue(_okMessage);
        }

        internal void InvokeMessageReceive(byte[] message, SocketAsyncEventArgs e) {
            //PackageCollection.Instance.Add(message, e.AcceptSocket, Tracker.Id);
            BottleneckMessageReceiver.Instance.Execute(message, e.AcceptSocket, Tracker.Id);
        }

        internal void AddIncomingTraffic(int bytes) {
            if (User == null) {
                Interlocked.Add(ref TemporaryTraffic.In, bytes);
            } else {
                //_statisticController.AddIncomingTraffic(Tracker.Id, bytes);
            }
        }

        internal void AddOutgoingTraffic(int bytes) {
            if (User == null) {
                Interlocked.Add(ref TemporaryTraffic.Out, bytes);
            } else {
                //_statisticController.AddOutgoingTraffic(Tracker.Id, bytes);
            }
        }

        internal void NewPackage(int packageType) {
            if (User == null) {
                TemporaryTraffic.IncrementPackageByType(packageType);
            } else {
                //_statisticController.AddNewPackage(Tracker.Id, packageType);
            }
        }

        public int Parse(SocketAsyncEventArgs e) {
            LastReceiveTime = DateTime.Now;

            byte[] newBuffer;
            if (LocalBuffer == null || LocalBuffer.Length == 0) {
                newBuffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, newBuffer, 0, e.BytesTransferred);
            } else {
                newBuffer = new byte[LocalBuffer.Length + e.BytesTransferred];
                Buffer.BlockCopy(LocalBuffer, 0, newBuffer, 0, LocalBuffer.Length);
                Buffer.BlockCopy(e.Buffer, e.Offset, newBuffer, LocalBuffer.Length, e.BytesTransferred);
            }
            LocalBuffer = newBuffer;
            ParseResult parseReslut = ParseCore();
            if (!IsInitialized) {
                bool initialized = parseReslut.Packages.Any(Initialize);
                if (!initialized) {
                    Logger.WriteIncorrectReceivedData(e.Buffer, string.Format("not initialized {0}", GetIpBySocket(e.AcceptSocket)));
                }

            }
            if (IsInitialized) {
                foreach (byte[] package in parseReslut.Packages) {
                    InvokeMessageReceive(package, e);
                }
            }
            LocalBuffer = parseReslut.Buffer;
            return parseReslut.OkMessageCount;
        }

        private string GetIpBySocket(Socket socket) {
            if (socket == null) {
                return string.Empty;
            }
            try {
                var remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                if (remoteIpEndPoint != null) {
                    return remoteIpEndPoint.Address.ToString();
                }
            } catch (Exception) {
                return string.Empty;
            }
            return string.Empty;
        }

        private ParseResult ParseCore() {
            var result = new ParseResult();
            int offset = 0;
            int startIndex = LocalBuffer.Locate(_startSymbol, 0);
            int endIndex = LocalBuffer.Locate(_endSymbol, 0);
            if (startIndex == -1 && endIndex == -1) {
                return ParseResult.Empty;
            }
            if (startIndex == -1 || startIndex > endIndex) {
                result.OkMessageCount++;
            }

            while (startIndex != -1 && endIndex != -1) {
                if (endIndex < startIndex) {
                    offset = startIndex;
                    startIndex = LocalBuffer.Locate(_startSymbol, offset);
                    endIndex = LocalBuffer.Locate(_endSymbol, offset);
                    continue;
                }
                var newPackage = new byte[endIndex - startIndex - 3];
                Buffer.BlockCopy(LocalBuffer, startIndex + 3, newPackage, 0, newPackage.Length);
                result.Packages.Add(newPackage);
                result.OkMessageCount++;
                offset = endIndex + 3;
                startIndex = LocalBuffer.Locate(_startSymbol, offset);
                endIndex = LocalBuffer.Locate(_endSymbol, offset);
            }
            if (offset <= LocalBuffer.Length - 1) {
                var temporaryBuffer = new byte[LocalBuffer.Length - offset];
                Buffer.BlockCopy(LocalBuffer, offset, temporaryBuffer, 0, temporaryBuffer.Length);
                result.Buffer = temporaryBuffer;
            } else {
                result.Buffer = null;
            }

            return result;
        }

        public class ParseResult {
            public int OkMessageCount { get; set; }
            public List<byte[]> Packages { get; set; }
            public byte[] Buffer { get; set; }

            public static ParseResult Empty {
                get { return new ParseResult { OkMessageCount = 1 }; }
            }

            public ParseResult() {
                Packages = new List<byte[]>();
            }

            public override bool Equals(object obj) {
                var other = obj as ParseResult;
                if (other == null) {
                    return false;
                }
                return OkMessageCount == other.OkMessageCount && Packages == other.Packages && Buffer == other.Buffer;
            }
        }
    }
}