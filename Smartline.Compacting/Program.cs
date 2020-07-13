using System;
using Smartline.Common.Runtime;

namespace Smartline.Compacting {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            var entryPoint = new EntryPoint<CompactingService, MainForm>();
            entryPoint.Start(args);
        }
    }
}
