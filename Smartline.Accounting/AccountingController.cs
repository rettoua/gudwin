using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Accounting {
    public class AccountingController {
        private readonly ServiceBase _serviceBase;
        private readonly IEnumerable<IAccountWorker> _accountWorkers;

        private Thread _thread;

        public AccountingController(ServiceBase serviceBase, IEnumerable<IAccountWorker> accountWorkers)
            : this() {
            _serviceBase = serviceBase;
            _accountWorkers = accountWorkers;
        }

        public AccountingController() {
            ServerDomain.Working = true;
            GlobalSaver.Instance.Start();
        }

        public void Start() {
            _thread = new Thread(StartInternal) { IsBackground = true };
            _thread.Start();
        }

        internal void Stop(bool fromInside) {
            try {
                ServerDomain.Working = false;
                GlobalSaver.Instance.Stop();
                if (fromInside) { _serviceBase.Stop(); }
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private void StartInternal() {
            while (ServerDomain.Working) {
                foreach (IAccountWorker worker in _accountWorkers) {
                    worker.Process();
                }
                Thread.Sleep(1000 * 5);
            }
        }
    }
}