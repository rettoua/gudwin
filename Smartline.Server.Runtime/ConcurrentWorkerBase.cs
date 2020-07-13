using System;
using System.Collections.Concurrent;
using System.Threading;
using Smartline.Common.Runtime;

namespace Smartline.Server.Runtime {
    public abstract class ConcurrentWorkerBase<T> : IWorker {
        protected readonly ConcurrentQueue<T> _queue;
        protected Thread _thread;
        protected ServerDomain _serverDomain;
        protected int SleepIntervalWhileWorking = 50;
        protected int SleepIntervalIfFailureExecute = 50;

        protected ConcurrentWorkerBase() {
            _queue = new ConcurrentQueue<T>();
        }

        protected ConcurrentWorkerBase(ServerDomain serverDomain)
            : this() {
            _serverDomain = serverDomain;
        }

        protected virtual void Working() {
            while (ServerDomain.Working) {
                T value;
                while (_queue.TryDequeue(out value)) {
                    while (!Execute(value)) {
                        Thread.Sleep(50);
                    }
                }
                BeforeSleep();
                Thread.Sleep(SleepIntervalWhileWorking);
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

        public void Start(ServerDomain serverDomain) {
            _serverDomain = serverDomain;
            Start();
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