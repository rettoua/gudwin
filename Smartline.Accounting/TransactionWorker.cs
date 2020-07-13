using System;
using System.Collections.Generic;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Accounting {
    public class TransactionWorker : IAccountWorker {
        private readonly IAccountingTransactionsProvider _transactionsProvider;

        public TransactionWorker(IAccountingTransactionsProvider transactionsProvider) {
            _transactionsProvider = transactionsProvider;
        }

        public void Process() {
            try {
                ProcessWaitingTransaction();
                ProcessPendingTransaction();
                ProcessCommitedTransaction();
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private void ProcessWaitingTransaction() {
            List<PaymentConfirmation> waitingTransaction = _transactionsProvider.GetPaymentTransactions(TransactionState.Waiting);
            foreach (PaymentConfirmation paymentConfirmation in waitingTransaction) {
                paymentConfirmation.State = TransactionState.Pending;
                _transactionsProvider.SaveTransaction(paymentConfirmation);
            }
        }

        private void ProcessPendingTransaction() {
            List<PaymentConfirmation> pendingTransactions = _transactionsProvider.GetPaymentTransactions(TransactionState.Pending);
            foreach (PaymentConfirmation paymentConfirmation in pendingTransactions) {
                Account account = _transactionsProvider.GetAccount(paymentConfirmation.UserId);
                string transactionId = paymentConfirmation.GetId();
                if (!account.Transactions.Contains(transactionId)) {
                    account.Transactions.Add(transactionId);
                    if (account.Save()) {
                        paymentConfirmation.State = TransactionState.Commited;
                        _transactionsProvider.SaveTransaction(paymentConfirmation);
                    }
                } else {
                    paymentConfirmation.State = TransactionState.Commited;
                    _transactionsProvider.SaveTransaction(paymentConfirmation);
                }
            }
        }

        private void ProcessCommitedTransaction() {
            List<PaymentConfirmation> commitedTransactions = _transactionsProvider.GetPaymentTransactions(TransactionState.Commited);
            GlobalAccountingSettings settings = _transactionsProvider.GetAccountingSettings();
            foreach (PaymentConfirmation paymentConfirmation in commitedTransactions) {
                Account account = _transactionsProvider.GetAccount(paymentConfirmation.UserId);
                string transactionId = paymentConfirmation.GetId();
                if (account.Transactions.Contains(transactionId)) {
                    account.Transactions.Remove(transactionId);
                    paymentConfirmation.AccountAmountBefore = account.Amount;
                    account.Amount += paymentConfirmation.Amount;
                    paymentConfirmation.AccountAmountAfter = account.Amount;
                    Account.UpdateAccountLockStatus(account, settings);
                    if (account.Save()) {
                        paymentConfirmation.State = TransactionState.Done;
                        _transactionsProvider.SaveTransaction(paymentConfirmation);
                        return;
                    }
                } else {
                    paymentConfirmation.State = TransactionState.Done;
                    _transactionsProvider.SaveTransaction(paymentConfirmation);
                }
            }
        }
    }
}