using System.Linq;

namespace Smartline.Mapping {
    public class CouchbaseAssignHelper {
        public static void AssingUserEvosIntergation(User basedUser, User destinationUser) {
            destinationUser.EvosIntegration = basedUser.EvosIntegration;
        }

        public static void AddNewTrackers(User basedUser, User destinationUser) {
            var newTrackers = basedUser.Trackers.Where(track => !destinationUser.Trackers.Select(z => z.Id).Contains(track.Id));
            destinationUser.Trackers.AddRange(newTrackers);
        }

        public static void RemoveTrackers(User basedUser, User destinationUser) {
            destinationUser.Trackers.RemoveAll(track => !basedUser.Trackers.Select(z => z.Id).Contains(track.Id));
        }
    }
}