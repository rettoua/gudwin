using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Mapping;

namespace Smartline.Accounting {
    internal class WriteOffPerDay : IWriteOffManager {

        List<WriteOff> IWriteOffManager.MakeWriteOffs(Tracker tracker, Account account, GlobalAccountingSettings accountingSettings) {
            var writeOffs = new List<WriteOff>();
            List<DateTime> dates = GetDatesForWriteOff(tracker, account);
            if (dates.Count == 0) { return writeOffs; }
            IEnumerable<string> ids = dates.Select(o => GpsDay.GetId(o, tracker.Id));
            List<GpsDay> gpaDays = CouchbaseManager.GetGpsDays(ids);

            return writeOffs;
        }

        bool IWriteOffManager.WriteOffRequired(Tracker tracker, Account account) {
            DateTime lastWriteOff;
            if (account.WriteOffs.TryGetValue(tracker.Id, out lastWriteOff)) {
                return lastWriteOff.Date < DateTime.Now.Date;
            }
            return true;
        }

        private List<DateTime> GetDatesForWriteOff(Tracker tracker, Account account) {
            var datesForWriteOfs = new List<DateTime>();
            DateTime startDate = account.WriteOffTime.Value.AddDays(1);
            while (true) {
                if (startDate.Date > account.WriteOffTime.Value.Date && startDate.Date < DateTime.Now.Date) {
                    datesForWriteOfs.Add(startDate);
                    startDate = startDate.AddDays(1);
                } else {
                    break;
                }
            }
            return datesForWriteOfs;
        }
    }
}