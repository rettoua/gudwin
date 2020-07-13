using System;

namespace Smartline.Mapping {
    public class AccountingHelper {
        public const int PaymentRequest = 0;
        public const int PaymentConfirmation = 1;
        public const int WriteOff = 2;
        public const int Account = 3;

        public static void UpdateFinancialLocking(Account account, GlobalAccountingSettings settings) {
            if (account.IsFinansialLock) {
                bool lockRequired = account.Amount <= settings.OffAmount;
                if (lockRequired) { return; }
                account.IsFinansialLock = false;
                account.WriteOffTime = DateTime.Now;
            } else {
                account.IsFinansialLock = account.Amount <= settings.OffAmount;
            }
        }
    }
}