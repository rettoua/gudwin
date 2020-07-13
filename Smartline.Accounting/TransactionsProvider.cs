using System.Collections.Generic;
using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Accounting {
    internal class TransactionsProvider : IAccountingTransactionsProvider {
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

        public List<PaymentConfirmation> GetPaymentTransactions(TransactionState state) {
            return CouchbaseManager.GetPaymentTransactions(state);
        }

        public void SaveTransaction(PaymentConfirmation paymentConfirmation) {
            string serialized = JSON.Serialize(paymentConfirmation);
            CouchbaseManager.SaveToAccountingBucket(StoreMode.Set, PaymentConfirmation.GetId(paymentConfirmation), serialized);
        }
    }
}