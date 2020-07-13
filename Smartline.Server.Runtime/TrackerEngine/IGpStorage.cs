using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.TrackerEngine {
    public interface IGpStorage {
        Gp GetLastGp(int trackerUid);
        void Save(Gp gp, string id, bool saveToDataBase);
    }
}