using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Smartline.Server.Runtime.TransportLayout {
    internal sealed class SocketAsyncEventArgsPoolAccept {
        private Int32 _nextTokenId;
        private readonly EventHandler<SocketAsyncEventArgs> _ioComplete;
        private readonly Stack<SocketAsyncEventArgs> _pool;
        private const int AcceptedObject = 200;

        internal SocketAsyncEventArgsPoolAccept(EventHandler<SocketAsyncEventArgs> ioComplete) {
            _ioComplete = ioComplete;
            _pool = new Stack<SocketAsyncEventArgs>(AcceptedObject);
        }

        internal Int32 Count {
            get { return _pool.Count; }
        }

        internal Int32 AssignTokenId() {
            Int32 tokenId = Interlocked.Increment(ref _nextTokenId);
            return tokenId;
        }

        internal SocketAsyncEventArgs Pop() {
            lock (_pool) {
                if (_pool.Count == 0) {
                    Push(CreateNewSaeaForAccept());
                }
                return _pool.Pop();
            }
        }

        internal void Push(SocketAsyncEventArgs item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            lock (_pool) {
                _pool.Push(item);
            }
        }

        internal void CreateSaeaObjects() {
            for (Int32 i = 0; i < AcceptedObject; i++) {
                Push(CreateNewSaeaForAccept());
            }
        }

        internal SocketAsyncEventArgs CreateNewSaeaForAccept() {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += _ioComplete;

            var theAcceptOpToken = new AcceptOpUserToken(AssignTokenId() + 10000);
            acceptEventArg.UserToken = theAcceptOpToken;
            return acceptEventArg;
        }
    }
}