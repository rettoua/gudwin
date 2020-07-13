using Smartline.Mapping;

namespace Smartline.Compacting {
    public interface IGpsDayProvider {
        void AssignTracker(Tracker tracker);
        GpsDay GetLastDay();
        GpsDay CreateDay();
    }
}