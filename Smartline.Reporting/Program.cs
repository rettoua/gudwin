using System;
using Smartline.Common.Runtime;

namespace Smartline.Reporting {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            var entryPoint = new EntryPoint<ReportingService, MainForm>();
            entryPoint.Start(args);
        }
    }
}
