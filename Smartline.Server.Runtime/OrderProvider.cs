using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smartline.Server.Runtime {
    public class OrderProvider {
        private static OrderProvider _instance;

        private OrderProvider() {
        }

        public static OrderProvider Instance {
            get { return _instance ?? (_instance = new OrderProvider()); }
            set { _instance = value; }
        }

        public void Start() {

        }

        public void Stop() {

        }

        public void Save() {

        }
    }
}