using System;
using System.Collections.Generic;
using System.Linq;
using Smartline.Common.Runtime;
using Smartline.Mapping;

namespace Smartline.Accounting {
    public class WriteOffWorker : IAccountWorker {
        private readonly IAccountingWriteOffsProvider _writeOffsProvider;

        public WriteOffWorker(IAccountingWriteOffsProvider writeOffsProvider) {
            _writeOffsProvider = writeOffsProvider;
        }

        public void Process() {
            CreateWriteOffs();
            ProcessWriteOffs();
        }

        private void CreateWriteOffs() {
            List<Account> accounts = _writeOffsProvider.GetAccounts();
            if (accounts == null || accounts.Count == 0) { return; }
            GlobalAccountingSettings accountingSettings = _writeOffsProvider.GetAccountingSettings();
            accounts.ForEach(account => DoCreateWriteOffs(account, accountingSettings));
        }

        private void DoCreateWriteOffs(Account account, GlobalAccountingSettings accountingSettings) {
            if (account.IsFinansialLock) { return; }
            try {
                User user = _writeOffsProvider.GetUser(account.UserId);
                if (user == null || user.UserName == "Administrator") { return; }

                IWriteOffManager writeOffManager = GetWriteOffManagerByAccount(account);
                foreach (Tracker tracker in user.Trackers) {
                    if (!writeOffManager.WriteOffRequired(tracker, account)) { continue; }
                    List<WriteOff> writeOffs = writeOffManager.MakeWriteOffs(tracker, account, accountingSettings);
                    if (!writeOffs.Any()) { continue; }
                    foreach (WriteOff writeOff in writeOffs) {
                        if (WriteOffCanLeadToFinancialLock(account, writeOff, accountingSettings)) { return; }
                        if (_writeOffsProvider.WriteOffExist(writeOff)) { continue; }
                        _writeOffsProvider.SaveWriteOff(writeOff);
                    }
                }

            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private bool WriteOffCanLeadToFinancialLock(Account account, WriteOff writeOff, GlobalAccountingSettings settings) {
            account.Amount -= writeOff.Amount;
            Account.UpdateAccountLockStatus(account, settings);
            return account.IsFinansialLock;
        }

        private IWriteOffManager GetWriteOffManagerByAccount(Account account) {
            //if (account.WriteOff == WriteOffEnum.PerDay) {
            //    return new WriteOffPerDay();
            //}
            return new WriteOffPerMonth();
        }

        private void ProcessWriteOffs() {
            try {
                ProcessWaitingWriteOffs();
                ProcessPendingWriteOffs();
                ProcessCommmitedWriteOffs();
            } catch (Exception exception) {
                Logger.Write(exception);
            }
        }

        private void ProcessWaitingWriteOffs() {
            List<WriteOff> waitingTransaction = _writeOffsProvider.GetWriteOffTransactions(TransactionState.Waiting);
            foreach (WriteOff writeOff in waitingTransaction) {
                writeOff.State = TransactionState.Pending;
                _writeOffsProvider.SaveWriteOff(writeOff);
            }
        }

        private void ProcessPendingWriteOffs() {
            List<WriteOff> pendingTransactions = _writeOffsProvider.GetWriteOffTransactions(TransactionState.Pending);
            foreach (WriteOff writeOff in pendingTransactions) {
                var account = _writeOffsProvider.GetAccount(writeOff.UserId);
                string transactionId = writeOff.GetId();
                if (!account.WriteOffTransactions.Contains(transactionId)) {
                    account.WriteOffTransactions.Add(transactionId);
                    account.WriteOffs[writeOff.TrackerUid] = writeOff.Time;
                    if (account.Save()) {
                        writeOff.State = TransactionState.Commited;
                        _writeOffsProvider.SaveWriteOff(writeOff);
                    }
                } else {
                    writeOff.State = TransactionState.Commited;
                    _writeOffsProvider.SaveWriteOff(writeOff);
                }
            }
        }

        private void ProcessCommmitedWriteOffs() {
            List<WriteOff> commitedTransactions = _writeOffsProvider.GetWriteOffTransactions(TransactionState.Commited);
            GlobalAccountingSettings settings = _writeOffsProvider.GetAccountingSettings();
            foreach (WriteOff writeOff in commitedTransactions) {
                var account = _writeOffsProvider.GetAccount(writeOff.UserId);
                string transactionId = writeOff.GetId();
                if (account.WriteOffTransactions.Contains(transactionId)) {
                    account.WriteOffTransactions.Remove(transactionId);
                    writeOff.AmountBefore = account.Amount;
                    account.Amount -= writeOff.Amount;
                    writeOff.AmountAfter = account.Amount;
                    Account.UpdateAccountLockStatus(account, settings);
                    if (account.Save()) {
                        writeOff.State = TransactionState.Done;
                        _writeOffsProvider.SaveWriteOff(writeOff);
                        return;
                    }
                } else {
                    writeOff.State = TransactionState.Done;
                    _writeOffsProvider.SaveWriteOff(writeOff);
                }
            }
        }
    }
}