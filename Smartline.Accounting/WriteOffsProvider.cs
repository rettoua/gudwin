using System.Collections.Generic;
using Smartline.Mapping;

namespace Smartline.Accounting {
    public class WriteOffsProvider : IAccountingWriteOffsProvider {
        public GlobalAccountingSettings GetAccountingSettings() {
            GlobalAccountingSettings accountingSettings = GlobalAccountingSettings.Get();
            if (accountingSettings == null) {
                accountingSettings = new GlobalAccountingSettings();
                accountingSettings.Save();
            }
            return accountingSettings;
        }

        public Account GetAccount(int userId) {
            return Account.Get(userId);
        }

        public User GetUser(int userId) {
            return CouchbaseManager.GetUser(userId);
        }

        public List<WriteOff> GetWriteOffTransactions(TransactionState state) {
            return CouchbaseManager.GetWriteOffTransactions(state);
        }

        public List<Account> GetAccounts() {
            return CouchbaseManager.GetAccounts();
        }

        public bool WriteOffExist(WriteOff writeOff) {
            return CouchbaseManager.AccountingDocumentExist(writeOff.GetId());
        }

        public bool SaveWriteOff(WriteOff writeOff) {
            return CouchbaseManager.SaveWriteOff(writeOff);
        }
    }
}