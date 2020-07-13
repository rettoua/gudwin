using System.Linq;

namespace Smartline.Mapping {
    public interface IUserTracker {
        User User { get; set; }
        Tracker Tracker { get; set; }
    }

    internal class UserTracker : IUserTracker {
        public User User { get; set; }
        public Tracker Tracker { get; set; }

        public UserTracker(User user, int trackerId) {
            User = user;
            Tracker = User.Trackers.FirstOrDefault(tracker => tracker.TrackerId == trackerId);
        }
    }
}