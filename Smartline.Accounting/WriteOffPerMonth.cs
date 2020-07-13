using System;
using System.Collections.Generic;
using Smartline.Mapping;

namespace Smartline.Accounting {
    internal class WriteOffPerMonth : IWriteOffManager {

        bool IWriteOffManager.WriteOffRequired(Tracker tracker, Account account) {
            DateTime lastWriteOff;
            if (account.WriteOffs.TryGetValue(tracker.Id, out lastWriteOff)) {
                bool writeOffRequired = lastWriteOff.Date.AddMonths(1) < DateTime.Now.Date;
                if (!writeOffRequired) { return false; }
            }
            return IsPackagesExist(tracker.Id, DateTime.Now.AddMonths(-1), DateTime.Now);
        }

        List<WriteOff> IWriteOffManager.MakeWriteOffs(Tracker tracker, Account account, GlobalAccountingSettings accountingSettings) {
            var writeOff = new WriteOff {
                Time = DateTime.Now,
                UserId = account.UserId,
                TrackerUid = tracker.Id,
                Amount = accountingSettings.WriteOffPerMonthAmount,
                State = TransactionState.Waiting
            };
            return new List<WriteOff> { writeOff };
        }

        private bool IsPackagesExist(int trackeUid, DateTime from, DateTime to) {
            return CouchbaseManager.IsPackageExists(trackeUid, from, to);
        }
    }
}