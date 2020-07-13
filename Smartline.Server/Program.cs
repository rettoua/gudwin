using Smartline.Server.Runtime;
using Smartline.Common.Runtime;

namespace Smartline.Server {
    public class Program {
        static void Main(string[] args) {
            var entryPoint = new EntryPoint<HideService, MainForm>();
            entryPoint.Start(args);
        }
    }
}