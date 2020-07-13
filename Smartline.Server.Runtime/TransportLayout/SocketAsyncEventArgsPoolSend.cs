using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Smartline.Server.Runtime.Monitoring;

namespace Smartline.Server.Runtime.TransportLayout {
    internal sealed class SocketAsyncEventArgsPoolSend {
        private Int32 _nextTokenId;
        readonly SocketListenerSettings _socketListenerSettings;
        private readonly StatisticController _statisticController;
        private readonly BufferManager _bufferManager;
        private readonly EventHandler<SocketAsyncEventArgs> _ioComplete;
        private readonly Queue<SocketAsyncEventArgs> _pool;
        private readonly SocketListener _socketListener;

        internal SocketAsyncEventArgsPoolSend(SocketListenerSettings socketListenerSettings,
            StatisticController statisticController,
            BufferManager bufferManager,
            EventHandler<SocketAsyncEventArgs> ioComplete,
            SocketListener socketListener) {
            _socketListenerSettings = socketListenerSettings;
            _statisticController = statisticController;
            _bufferManager = bufferManager;
            _ioComplete = ioComplete;
            _pool = new Queue<SocketAsyncEventArgs>(_socketListenerSettings.NumberOfSaeaForRecSend);
            _socketListener = socketListener;
        }

        internal Int32 Count {
            get {
                lock (_pool) {
                    return _pool.Count;
                }
            }
        }

        internal Int32 AssignTokenId() {
            Int32 tokenId = Interlocked.Increment(ref _nextTokenId);
            return tokenId;
        }

        internal SocketAsyncEventArgs Pop() {
            lock (_pool) {
                return _pool.Dequeue();
            }
        }

        internal void Push(SocketAsyncEventArgs item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            lock (_pool) {
                _pool.Enqueue(item);
            }
        }

        internal void CreateSaeaObjects() {
            for (Int32 i = 0; i < _socketListenerSettings.NumberOfSaeaForRecSend; i++) {
                Push(CreateSaeaForSendReceive());
            }
        }

        internal SocketAsyncEventArgs CreateSaeaForSendReceive() {
            var eventArgObjectForPool = new SocketAsyncEventArgs();
            _bufferManager.SetBuffer(eventArgObjectForPool);
            Int32 tokenId = AssignTokenId() + 1000000;

            //Attach the SocketAsyncEventArgs object
            //to its event handler. Since this SocketAsyncEventArgs object is 
            //used for both receive and send operations, whenever either of those 
            //completes, the IO_Completed method will be called.
            eventArgObjectForPool.Completed += _ioComplete;

            //We can store data in the UserToken property of SAEA object.
            var theTempReceiveSendUserToken = new DataHoldingUserToken(_statisticController.AddOnlineTracker,
                _statisticController.RemoveOnlineTracker,
                eventArgObjectForPool.Offset,
                eventArgObjectForPool.Offset + _socketListenerSettings.BufferSize, tokenId, _statisticController, _socketListener);
            theTempReceiveSendUserToken.Saea = eventArgObjectForPool;

            eventArgObjectForPool.UserToken = theTempReceiveSendUserToken;
            return eventArgObjectForPool;
        }
    }
}