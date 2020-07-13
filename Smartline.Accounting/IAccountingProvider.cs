using System.Collections.Generic;
using Smartline.Mapping;

namespace Smartline.Accounting {
    public interface IAccountingProvider {
        GlobalAccountingSettings GetAccountingSettings();
        Account GetAccount(int userId);
        User GetUser(int userId);
    }

    public interface IAccountingTransactionsProvider : IAccountingProvider {
        List<PaymentConfirmation> GetPaymentTransactions(TransactionState state);
        void SaveTransaction(PaymentConfirmation paymentConfirmation);
    }

    public interface IAccountingWriteOffsProvider : IAccountingProvider {
        List<WriteOff> GetWriteOffTransactions(TransactionState state);
        List<Account> GetAccounts();
        bool WriteOffExist(WriteOff writeOff);
        bool SaveWriteOff(WriteOff writeOff);
    }
}