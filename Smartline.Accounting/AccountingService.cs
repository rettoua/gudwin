using System.Collections.Generic;
using System.ServiceProcess;

namespace Smartline.Accounting {
    partial class AccountingService : ServiceBase {
        private readonly AccountingController _accountingController;
        public AccountingService() {
            InitializeComponent();
            ServiceName = "GUDWIN Accounting Service";
            _accountingController = new AccountingController(this,
                new List<IAccountWorker> {
                                             new TransactionWorker(new TransactionsProvider()), 
                                             new WriteOffWorker(new WriteOffsProvider())
                                         });
        }

        protected override void OnStart(string[] args) {
            _accountingController.Start();
        }

        protected override void OnStop() {
            if (_accountingController == null) { return; }
            _accountingController.Stop(false);
        }
    }
}
