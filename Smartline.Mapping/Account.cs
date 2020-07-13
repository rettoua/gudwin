using System;
using System.Collections.Generic;

namespace Smartline.Mapping {
    public class Account {
        public int T { get { return AccountingHelper.Account; } }

        public int UserId { get; set; }

        public double Amount { get; set; }

        public DateTime? ProcessedTime { get; set; }

        public DateTime? WriteOffTime { get; set; }

        public bool IsFinansialLock { get; set; }

        public WriteOffEnum WriteOff { get; set; }

        public Dictionary<int, DateTime> WriteOffs { get; set; }

        public List<string> WriteOffTransactions { get; set; }

        public List<string> Transactions { get; set; }

        public Account() {
            WriteOffTransactions = new List<string>();
            Transactions = new List<string>();
            WriteOffs = new Dictionary<int, DateTime>();
            WriteOff = WriteOffEnum.PerMonth;
        }

        public string GetId() { return GetId(UserId); }

        public static string GetId(Account account) {
            return GetId(account.UserId);
        }

        public static string GetId(int userId) {
            return string.Format("a_{0}", userId);
        }

        public static Account Get(int userId) {
            string id = GetId(userId);
            var account = CouchbaseManager.GetSingleValueFromAcounting<Account>(id);
            if (account == null) {
                account = new Account { Amount = 0, UserId = userId };
                account.Save();
            }
            return account;
        }

        public virtual bool Save() {
            return CouchbaseManager.SaveAccount(this);
        }

        public static void UpdateAccountLockStatus(Account account, GlobalAccountingSettings settings) {
            account.IsFinansialLock = account.Amount < settings.OffAmount;
        }
    }

    public enum WriteOffEnum {
        PerDay = 0,
        PerMonth = 1,
    }
}