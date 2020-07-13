using System;
using System.Net;

namespace Smartline.Server.Runtime.TransportLayout {
    public class SocketListenerSettings {
        // the maximum number of connections the sample is designed to handle simultaneously 
        private readonly Int32 _maxConnections;

        // this variable allows us to create some extra SAEA objects for the pool,
        // if we wish.
        private readonly Int32 _numberOfSaeaForRecSend;

        // max # of pending connections the listener can hold in queue
        private readonly Int32 _backlog;

        // tells us how many objects to put in pool for accept operations
        private readonly Int32 _maxSimultaneousAcceptOps;

        // buffer size to use for each socket receive operation
        private readonly Int32 _receiveBufferSize;

        // length of message prefix for receive ops
        private readonly Int32 _receivePrefixLength;

        // length of message prefix for send ops
        private readonly Int32 _sendPrefixLength;

        // See comments in buffer manager.
        private readonly Int32 _opsToPreAllocate;

        // Endpoint for the listener.
        private readonly IPEndPoint _localEndPoint;

        public SocketListenerSettings(Int32 maxConnections, Int32 excessSaeaObjectsInPool, Int32 backlog, Int32 maxSimultaneousAcceptOps, Int32 receivePrefixLength, Int32 receiveBufferSize, Int32 sendPrefixLength, Int32 opsToPreAlloc, IPEndPoint theLocalEndPoint) {
            _maxConnections = maxConnections;
            _numberOfSaeaForRecSend = maxConnections + excessSaeaObjectsInPool;
            _backlog = backlog;
            _maxSimultaneousAcceptOps = maxSimultaneousAcceptOps;
            _receivePrefixLength = receivePrefixLength;
            _receiveBufferSize = receiveBufferSize;
            _sendPrefixLength = sendPrefixLength;
            _opsToPreAllocate = opsToPreAlloc;
            _localEndPoint = theLocalEndPoint;
        }

        public Int32 MaxConnections {
            get {
                return _maxConnections;
            }
        }
        public Int32 NumberOfSaeaForRecSend {
            get {
                return _numberOfSaeaForRecSend;
            }
        }
        public Int32 Backlog {
            get {
                return _backlog;
            }
        }
        public Int32 MaxAcceptOps {
            get {
                return _maxSimultaneousAcceptOps;
            }
        }
        public Int32 ReceivePrefixLength {
            get {
                return _receivePrefixLength;
            }
        }
        public Int32 BufferSize {
            get {
                return _receiveBufferSize;
            }
        }
        public Int32 SendPrefixLength {
            get {
                return _sendPrefixLength;
            }
        }
        public Int32 OpsToPreAllocate {
            get {
                return _opsToPreAllocate;
            }
        }
        public IPEndPoint LocalEndPoint {
            get {
                return _localEndPoint;
            }
        }
    }
}
