using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public interface ISensorHandler {
        void Update(Gp gp);
    }
}