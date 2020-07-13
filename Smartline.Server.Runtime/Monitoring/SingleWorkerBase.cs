using System;
using System.Threading;
using Smartline.Common.Runtime;

namespace Smartline.Server.Runtime.Monitoring {
    public abstract class SingleWorkerBase<T> : IWorker {
        protected T _item;
        protected Thread _thread;

        public abstract int Timeout { get; }
        public T Item { get { return _item; } }

        public void SetObject(T item) {
            if (item == null) {
                throw new Exception("Can not set item object. Item mustn't be null.");
            }
            _item = item;
        }

        protected virtual void Working() {
            while (ServerDomain.Working) {
                Execute(_item);
                BeforeSleep();
                Thread.Sleep(Timeout);
            }
        }

        protected virtual bool Execute(T item) {
            return true;
        }

        protected virtual void BeforeSleep() { }

        public void Start() {
            if (_thread != null) {
                try {
                    _thread.Abort();
                } catch (Exception exception) {
                    Logger.Write(exception);
                }
            }
            _thread = new Thread(Working) { IsBackground = true };
            _thread.Start();
        }

        public void Stop() {
            if (_thread == null) {
                return;
            }
            try {
                _thread.Abort();
            } catch (Exception) {
                //todo: probably nothing                
            }
        }
    }
}
