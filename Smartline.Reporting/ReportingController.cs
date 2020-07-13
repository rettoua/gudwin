using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Reporting {
    public class ReportingController {
        private readonly ServiceBase _serviceBase;
        private Thread _thread;

        public ReportingController(ServiceBase serviceBase)
            : this() {
            _serviceBase = serviceBase;
        }
        public ReportingController() {
            ServerDomain.Working = true;
            GlobalSaver.Instance.Start();
        }

        private IEnumerable<Tracker> LoadTrackers() {
            return CouchbaseManager.GetAllTrackers<Tracker>();
        }

        public void Start() {
            _thread = new Thread(StartInternal) { IsBackground = true };
            _thread.Start();
        }

        private void StartInternal() {
            while (ServerDomain.Working) {
                List<Tracker> trackers = LoadTrackers().ToList();
                if (trackers.Count == 0) {
                    return;
                }
                foreach (Tracker tracker in trackers) {
                    try {
                        var updated = new TrackerReportUpdater(tracker);
                        updated.UpdateReport();
                    } catch (Exception exception) {
                        Logger.Write(exception);
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(new TimeSpan(0, 0, 10));
            }
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
    }
}