using System.Collections.Generic;
using System.Linq;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class CarsRepository {
        public List<Repository> GetCarListForUser(User user) {
            if (user == null) {
                return new List<Repository>();
            }
            List<Repository> repositories = (from c in user.Trackers
                                             select Repository.Transform(c, true)).ToList();
            UpdateRepositories(repositories);
            return repositories;
        }

        public List<Repository> GetCarListForUser(User user, InternalUser internalUser) {
            if (user == null) {
                return new List<Repository>();
            }
            var repositories = (from c in user.Trackers
                                where internalUser.TrackerUids != null && internalUser.TrackerUids.Contains(c.TrackerId)
                                select Repository.Transform(c, true)).ToList();
            UpdateRepositories(repositories);
            return repositories;
        }
        private static void UpdateRepositories(List<Repository> repositories) {
            Dictionary<string, Gp> onlinePackages = CouchbaseManager.GetMultipleValuesFromOnline<Gp>(repositories.Select(repo => repo.Id + ""));
            foreach (Repository repository in repositories) {
                Gp gp;
                onlinePackages.TryGetValue(repository.Id + "", out gp);
                repository.Update(gp);
            }
        }
    }
}