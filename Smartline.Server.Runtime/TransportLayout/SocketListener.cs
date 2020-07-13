using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime.Monitoring;

namespace Smartline.Server.Runtime.TransportLayout {
    public class SocketListener {
        private readonly StatisticController _statisticController;
        private readonly ConcurrentDictionary<int, DataHoldingUserToken> _connections;
        private readonly bool _sendConfirmation;

        //This variable determines the number of 
        //SocketAsyncEventArg objects put in the pool of objects for receive/send.
        //The value of this variable also affects the Semaphore.
        //This app uses a Semaphore to ensure that the max # of connections
        //value does not get exceeded.
        //Max # of connections to a socket can be limited by the Windows Operating System
        //also.
        public const Int32 MaxNumberOfConnections = 3000;

        //You would want a buffer size larger than 25 probably, unless you know the
        //data will almost always be less than 25. It is just 25 in our test app.
        public const Int32 TestBufferSize = 31;

        //This is the maximum number of asynchronous accept operations that can be 
        //posted simultaneously. This determines the size of the pool of 
        //SocketAsyncEventArgs objects that do accept operations. Note that this
        //is NOT the same as the maximum # of connections.
        public const Int32 MaxSimultaneousAcceptOps = 200;

        //The size of the queue of incoming connections for the listen socket.
        public const Int32 Backlog = 1000;

        //For the BufferManager
        public const Int32 OpsToPreAlloc = 2; // 1 for receive, 1 for send

        //allows excess SAEA objects in pool.
        public const Int32 ExcessSaeaObjectsInPool = 1;

        //This number must be the same as the value on the client.
        //Tells what size the message prefix will be. Don't change this unless
        //you change the code, because 4 is the length of 32 bit integer, which
        //is what we are using as prefix.
        public const Int32 ReceivePrefixLength = 4;
        public const Int32 SendPrefixLength = 4;

        public Int32 MainTransMissionId = 10000;
        public Int32 StartingTid; //
        public Int32 MainSessionId = 1000000000;

        //If you make this a positive value, it will simulate some delay on the
        //receive/send SAEA object after doing a receive operation.
        //That would be where you would do some work on the received data, 
        //before responding to the client.
        //This is in milliseconds. So a value of 1000 = 1 second delay.
        public readonly Int32 MsDelayAfterGettingMessage = -1;

        // To keep a record of maximum number of simultaneous connections
        // that occur while the server is running. This can be limited by operating
        // system and hardware. It will not be higher than the value that you set
        // for maxNumberOfConnections.
        public Int32 MaxSimultaneousClientsThatWereConnected = 0;

        //These strings are just for console interaction.
        public const string CheckString = "C";
        public const string CloseString = "Z";
        public const string Wpf = "T";
        public const string WpfNo = "F";
        public string WpfTrueString = "";
        public string WpfFalseString = "";

        internal Int32 NumberOfAcceptedSockets;
        internal Int32 NumberOfReceivedPackages;
        private Socket _listenSocket;

        private readonly BufferManager _bufferManager;

        //A Semaphore has two parameters, the initial number of available slots
        // and the maximum number of slots. We'll make them the same. 
        //This Semaphore is used to keep from going over max connection #. (It is not about 
        //controlling threading really here.)   
        readonly Semaphore _theMaxConnectionsEnforcer;

        private readonly SocketListenerSettings _socketListenerSettings;

        // pool of reusable SocketAsyncEventArgs objects for accept operations
        private readonly SocketAsyncEventArgsPoolAccept _poolOfAcceptEventArgs;
        // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations
        private readonly SocketAsyncEventArgsPoolSend _poolOfRecSendEventArgs;

        private readonly List<SocketAsyncEventArgs> _cacheOfActiveAsyncArgs = new List<SocketAsyncEventArgs>();
        private static object LockCacheObject = new object();
        private Thread _checkCacheThread;

        public SocketListener(SocketListenerSettings theSocketListenerSettings, bool sendConfirmation = true) {
            _sendConfirmation = sendConfirmation;
            _connections = new ConcurrentDictionary<int, DataHoldingUserToken>();
            _statisticController = StatisticController.Instance;
            _socketListenerSettings = theSocketListenerSettings;
            _bufferManager =
                new BufferManager(
                    _socketListenerSettings.BufferSize * _socketListenerSettings.NumberOfSaeaForRecSend *
                    _socketListenerSettings.OpsToPreAllocate,
                    _socketListenerSettings.BufferSize * _socketListenerSettings.OpsToPreAllocate);

            _poolOfRecSendEventArgs = new SocketAsyncEventArgsPoolSend(_socketListenerSettings, _statisticController, _bufferManager, IoCompleted, this);
            _poolOfAcceptEventArgs = new SocketAsyncEventArgsPoolAccept(AcceptEventArgCompleted);

            _theMaxConnectionsEnforcer = new Semaphore(_socketListenerSettings.MaxConnections, _socketListenerSettings.MaxConnections);
            Init();
            StartListen();
            StartCheckCache();
        }

        internal void Init() {
            _bufferManager.InitBuffer();
            _poolOfAcceptEventArgs.CreateSaeaObjects();
            _poolOfRecSendEventArgs.CreateSaeaObjects();
        }

        internal void StartListen() {
            _listenSocket = new Socket(_socketListenerSettings.LocalEndPoint.AddressFamily, SocketType.Stream,
                                       ProtocolType.Tcp);
            _listenSocket.Bind(_socketListenerSettings.LocalEndPoint);
            _listenSocket.Listen(_socketListenerSettings.Backlog);
            StartAccept();
        }

        internal void StartAccept() {
            SocketAsyncEventArgs acceptEventArg = _poolOfAcceptEventArgs.Pop();
            _theMaxConnectionsEnforcer.WaitOne();
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent) {
                ProcessAccept(acceptEventArg);
            }
        }

        private void AcceptEventArgCompleted(object sender, SocketAsyncEventArgs e) {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs) {
            if (acceptEventArgs.SocketError != SocketError.Success) {
                LoopToStartAccept();

                HandleBadAccept(acceptEventArgs);
                return;
            }

            LoopToStartAccept();

            SocketAsyncEventArgs receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
            var dataHolding = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            dataHolding.CreateSessionId();

            //CommunicationStateManager.Instance.AddNewConnectionInitialized(acceptEventArgs.AcceptSocket.RemoteEndPoint.ToString());


            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
            dataHolding.Socket = receiveSendEventArgs.AcceptSocket;

            acceptEventArgs.AcceptSocket = null;
            _poolOfAcceptEventArgs.Push(acceptEventArgs);

            AddToCache(receiveSendEventArgs);

            StartReceive(receiveSendEventArgs);
        }

        public bool SendData(int trackerUid, byte[] buffer) {
            if (_cacheOfActiveAsyncArgs.Count == 0) { return false; }
            List<SocketAsyncEventArgs> v = _cacheOfActiveAsyncArgs;
            List<SocketAsyncEventArgs> values = v.Where(saea => saea.UserToken is DataHoldingUserToken && ((DataHoldingUserToken)saea.UserToken).TrackerUId == trackerUid).ToList();
            if (values.Count == 0) { return false; }

            foreach (SocketAsyncEventArgs socketAsyncEventArgse in values) {
                var store = (DataHoldingUserToken)socketAsyncEventArgse.UserToken;
                SocketAsyncEventArgs receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
                DataHoldingUserToken userTokenCopy = store.Copy();
                receiveSendEventArgs.UserToken = userTokenCopy;
                receiveSendEventArgs.AcceptSocket = store.Socket;
                userTokenCopy.SendBuffer.Enqueue(buffer);
                userTokenCopy.PrepareDataForSend();
                StartSend(receiveSendEventArgs);
            }

            return true;
        }

        private void LoopToStartAccept() {
            StartAccept();
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs, bool setDefaultBuffer = true) {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;

            try {
                if (setDefaultBuffer) {
                    receiveSendEventArgs.SetBuffer(receiveSendToken.BufferOffsetReceive,
                        _socketListenerSettings.BufferSize);
                }
                bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);
                if (willRaiseEvent) {
                    return;
                }
            } catch (Exception exception) {
                Logger.Write(exception);
                Logger.Write(
                    new Exception(
                        string.Format("Custom exception: _poolOfRecSendEventArgs-{0};_poolOfAcceptEventArgs-{1}",
                                      _poolOfRecSendEventArgs.Count, _poolOfAcceptEventArgs.Count)));
                CloseClientSocket(receiveSendEventArgs);
                return;
            }
            ProcessReceive(receiveSendEventArgs);
        }

        private void IoCompleted(object sender, SocketAsyncEventArgs e) {
            switch (e.LastOperation) {
                case SocketAsyncOperation.Receive: {
                    ProcessReceive(e);
                }
                break;
                case SocketAsyncOperation.Send: {
                    ProcessSend(e);
                }
                break;
                default:
                throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e) {
            try {
                var receiveSendToken = (DataHoldingUserToken)e.UserToken;
                if (receiveSendToken.IsClearing) { return; }


                receiveSendToken.AddIncomingTraffic(e.BytesTransferred);
                if (e.SocketError != SocketError.Success ||
                    e.BytesTransferred == 0) {
                    CloseClientSocket(e);
                    return;
                }

                int packagesCount = receiveSendToken.Parse(e);
                if (packagesCount == -1) {
                    StartReceive(e);
                } else {
                    //_statisticController.NewPackage(packagesCount);
                    if (packagesCount > 0 && _sendConfirmation) {
                        receiveSendToken.AddMessagetoBuffer();
                        receiveSendToken.PrepareDataForSend();
                        StartSend(e);
                    } else {
                        StartReceive(e);
                    }
                }
            } catch (Exception exception) {
                Logger.Write(exception);
                CloseClientSocket(e);
            }
        }

        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs) {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;

            if (receiveSendToken.SendBytesRemainingCount <= _socketListenerSettings.BufferSize) {
                receiveSendEventArgs.SetBuffer(receiveSendToken.BufferOffsetSend,
                                               receiveSendToken.SendBytesRemainingCount);
                Buffer.BlockCopy(receiveSendToken.DataToSend, receiveSendToken.BytesSentAlreadyCount,
                                 receiveSendEventArgs.Buffer, receiveSendToken.BufferOffsetSend,
                                 receiveSendToken.SendBytesRemainingCount);
            } else {
                receiveSendEventArgs.SetBuffer(receiveSendToken.BufferOffsetSend, _socketListenerSettings.BufferSize);
                Buffer.BlockCopy(receiveSendToken.DataToSend, receiveSendToken.BytesSentAlreadyCount,
                                 receiveSendEventArgs.Buffer, receiveSendToken.BufferOffsetSend,
                                 _socketListenerSettings.BufferSize);
            }

            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.SendAsync(receiveSendEventArgs);

            if (!willRaiseEvent) {
                ProcessSend(receiveSendEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs receiveSendEventArgs) {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            if (receiveSendToken.IsClearing) { return; }

            receiveSendToken.AddOutgoingTraffic(receiveSendEventArgs.BytesTransferred);

            if (receiveSendEventArgs.SocketError == SocketError.Success && receiveSendEventArgs.BytesTransferred > 0) {
                receiveSendToken.SendBytesRemainingCount = receiveSendToken.SendBytesRemainingCount -
                                                           receiveSendEventArgs.BytesTransferred;
                if (receiveSendToken.SendBytesRemainingCount == 0) {
                    if (receiveSendToken.HasDataForSend) {
                        receiveSendToken.PrepareDataForSend();
                        StartSend(receiveSendEventArgs);
                    } else {
                        if (receiveSendToken.ShouldReleaseAfterUsing) {
                            //in case when was made extra send RSEA object should be released without closing socket
                            //ReleaseSaea(receiveSendEventArgs);
                            receiveSendEventArgs.AcceptSocket=null;
                            _poolOfRecSendEventArgs.Push(receiveSendEventArgs);
                        } else {
                            StartReceive(receiveSendEventArgs);
                        }
                    }
                } else {
                    receiveSendToken.BytesSentAlreadyCount += receiveSendEventArgs.BytesTransferred;
                    StartSend(receiveSendEventArgs);
                }
            } else {
                CloseClientSocket(receiveSendEventArgs);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e) {
            var receiveSendToken = (DataHoldingUserToken)e.UserToken;
            if (receiveSendToken.IsClearing || receiveSendToken.IsReleased) { return; }

            //CommunicationStateManager.Instance.AddTrackerDisconnected(receiveSendToken.Tracker, receiveSendToken.User,
            //                                                          "Disconnected",
            //                                                          e.AcceptSocket == null ||
            //                                                          !e.AcceptSocket.Connected
            //                                                              ? ""
            //                                                              : e.AcceptSocket.RemoteEndPoint.ToString());            
            try {
                receiveSendToken.IsClearing = true;
                if (e.AcceptSocket != null) {
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                    e.AcceptSocket.Close();
                }
            } catch {
            } finally {
                e.AcceptSocket = null;
            }
            ReleaseSaea(e);
        }

        private void ReleaseSaea(SocketAsyncEventArgs e) {
            if (e == null) { return; }

            var receiveSendToken = (DataHoldingUserToken)e.UserToken;
            //Debug.WriteLine("--- RELEASED TRACKER: {0}", receiveSendToken.TrackerId);
            receiveSendToken.Reset();
            _poolOfRecSendEventArgs.Push(e);
            RemoveFromCache(e);
            //System.Diagnostics.Debug.WriteLine("--- RELEASED: {0}");
            _theMaxConnectionsEnforcer.Release();
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs) {
            acceptEventArgs.AcceptSocket.Close();
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
        }

        internal void AddInitializedConnection(DataHoldingUserToken userToken) {
            //lock (_connections) {
            DataHoldingUserToken storedUserToken;
            if (!_connections.TryGetValue(userToken.TrackerUId, out storedUserToken)) {
                while (!_connections.TryAdd(userToken.Tracker.Id, userToken)) {
                    Thread.Sleep(50);
                }
            } else {
                if (userToken == storedUserToken) { return; }
                CloseClientSocket(storedUserToken.Saea);
                _connections[userToken.Tracker.Id] = userToken;
            }
            //}
        }

        internal void CleanUpOnExit() {
            DisposeAllSaeaObjects();
        }

        private void DisposeAllSaeaObjects() {
            SocketAsyncEventArgs eventArgs;
            while (_poolOfAcceptEventArgs.Count > 0) {
                eventArgs = _poolOfAcceptEventArgs.Pop();
                eventArgs.Dispose();
            }
            while (_poolOfRecSendEventArgs.Count > 0) {
                eventArgs = _poolOfRecSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }

        private void AddToCache(SocketAsyncEventArgs e) {
            lock (LockCacheObject) {
                _cacheOfActiveAsyncArgs.Add(e);
            }
        }

        private void RemoveFromCache(SocketAsyncEventArgs e) {
            lock (LockCacheObject) {
                _cacheOfActiveAsyncArgs.Remove(e);
            }
        }

        private void StartCheckCache() {
            _checkCacheThread = new Thread(DoStartCheckCache) {
                IsBackground = true
            };
            _checkCacheThread.Start();
        }

        private void DoStartCheckCache() {
            while (ServerDomain.Working) {
                List<SocketAsyncEventArgs> elementsToDelete;
                lock (LockCacheObject) {
                    elementsToDelete = _cacheOfActiveAsyncArgs.Where(ShouldDeleteAsyncArgs).ToList();
                }
                if (elementsToDelete.Count != 0) {
                    elementsToDelete.ForEach(CloseClientSocket);
                }
                Thread.Sleep(60 * 1000);//wait for one minute
            }
        }

        private bool ShouldDeleteAsyncArgs(SocketAsyncEventArgs e) {
            var userToken = e.UserToken as DataHoldingUserToken;
            if (userToken == null) { return true; }
            return (DateTime.Now - userToken.LastReceiveTime).TotalMinutes > 15;
        }
    }

}