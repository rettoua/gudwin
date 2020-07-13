using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smartline.Mapping;

namespace Smartline.Accounting.Test {
    [TestClass]
    public class TransactionWorkerTests {

        [TestMethod]
        public void ProcessWaitingTransactionExpectStateUpdated() {
            var waitingTransactions = new List<PaymentConfirmation> {
                                                                        new PaymentConfirmation { State = TransactionState.Waiting },
                                                                        new PaymentConfirmation { State = TransactionState.Waiting }
                                                                    };
            var fake = A.Fake<IAccountingTransactionsProvider>();
            fake.CallsTo(x => x.GetPaymentTransactions(TransactionState.Waiting)).Returns(waitingTransactions);
            var worker = new TransactionWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessWaitingTransaction", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PaymentConfirmation paymentConfirmation in waitingTransactions) {
                Assert.AreEqual(TransactionState.Pending, paymentConfirmation.State);
            }
        }

        [TestMethod]
        public void ProcessPendingTransactionExpectStateUpdated() {
            var pendingTransactions = new List<PaymentConfirmation> {
                                                                        new PaymentConfirmation { State = TransactionState.Pending, Description = "1"},
                                                                        new PaymentConfirmation { State = TransactionState.Pending, Description = "1" }
                                                                    };
            var accountFake = A.Fake<Account>();
            accountFake.CallsTo(x => x.Save()).Returns(true);
            var fake = A.Fake<IAccountingTransactionsProvider>();
            fake.CallsTo(x => x.GetPaymentTransactions(TransactionState.Pending)).Returns(pendingTransactions);
            fake.CallsTo(x => x.GetAccount(1)).Returns(accountFake);

            var worker = new TransactionWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessPendingTransaction", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PaymentConfirmation paymentConfirmation in pendingTransactions) {
                Assert.AreEqual(TransactionState.Commited, paymentConfirmation.State);
                Assert.IsTrue(accountFake.Transactions.Contains(paymentConfirmation.GetId()));
            }
        }

        [TestMethod]
        public void ProcessPendingTransactionExpectAccountSaveNotCalled() {
            var pendingTransactions = new List<PaymentConfirmation> {
                                                                        new PaymentConfirmation { State = TransactionState.Pending, Description = "1"}                                                                        
                                                                    };
            var accountFake = A.Fake<Account>();
            accountFake.CallsTo(x => x.Save()).Returns(true);
            accountFake.Transactions.Add(pendingTransactions.First().GetId());
            //Account.Save() should not be called because transaction already exist in account 
            accountFake.CallsTo(x => x.Save()).MustHaveHappened(Repeated.Never);
            var fake = A.Fake<IAccountingTransactionsProvider>();
            fake.CallsTo(x => x.GetPaymentTransactions(TransactionState.Pending)).Returns(pendingTransactions);
            fake.CallsTo(x => x.GetAccount(1)).Returns(accountFake);

            var worker = new TransactionWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessPendingTransaction", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PaymentConfirmation paymentConfirmation in pendingTransactions) {
                Assert.AreEqual(TransactionState.Commited, paymentConfirmation.State);
            }
        }

        [TestMethod]
        public void ProcessCommitedTransactionWithNoAccountTransactionsExpectTransactionsStateUpdated() {
            var pendingTransactions = new List<PaymentConfirmation> {
                                                                        new PaymentConfirmation { State = TransactionState.Commited, Description = "1" , TransactionId = 1}                                                                        ,
                                                                        new PaymentConfirmation { State = TransactionState.Commited, Description = "1", TransactionId = 2}                                                                        
                                                                   };
            var accountFake = A.Fake<Account>();
            accountFake.CallsTo(x => x.Save()).Returns(true);
            var fake = A.Fake<IAccountingTransactionsProvider>();
            fake.CallsTo(x => x.GetPaymentTransactions(TransactionState.Commited)).Returns(pendingTransactions);
            fake.CallsTo(x => x.GetAccount(1)).Returns(accountFake);

            var worker = new TransactionWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessCommitedTransaction", BindingFlags.Instance | BindingFlags.NonPublic);

            //Account.Save() should not be called because transaction already exist in account 
            accountFake.CallsTo(x => x.Save()).MustHaveHappened(Repeated.Never);

            foreach (PaymentConfirmation paymentConfirmation in pendingTransactions) {
                Assert.AreEqual(TransactionState.Done, paymentConfirmation.State);
            }
            Assert.AreEqual(0, accountFake.Transactions.Count);
        }

        [TestMethod]
        public void ProcessCommitedTransactionWithTransactionsExpectTransactionsExecuted() {
            var pendingTransactions = new List<PaymentConfirmation> {
                                                                        new PaymentConfirmation { State = TransactionState.Commited, Description = "1" , TransactionId = 1, Amount = 10}                                                                                                                                                                                                                       
                                                                   };
            var accountFake = A.Fake<Account>();
            accountFake.Amount = 25;
            accountFake.IsFinansialLock = true;
            accountFake.Transactions.Add(pendingTransactions.First().GetId());
            var fake = A.Fake<IAccountingTransactionsProvider>();
            fake.CallsTo(x => x.GetPaymentTransactions(TransactionState.Commited)).Returns(pendingTransactions);
            fake.CallsTo(x => x.GetAccount(1)).Returns(accountFake);
            accountFake.CallsTo(x => x.Save()).Returns(true);

            var worker = new TransactionWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessCommitedTransaction", BindingFlags.Instance | BindingFlags.NonPublic);

            accountFake.CallsTo(x => x.Save()).MustHaveHappened(Repeated.Exactly.Once);

            foreach (PaymentConfirmation paymentConfirmation in pendingTransactions) {
                Assert.AreEqual(TransactionState.Done, paymentConfirmation.State);
            }
            Assert.AreEqual(0, accountFake.Transactions.Count);
            Assert.AreEqual(35, accountFake.Amount);
            Assert.AreEqual(25, pendingTransactions.First().AccountAmountBefore);
            Assert.AreEqual(35, pendingTransactions.First().AccountAmountAfter);
            Assert.IsFalse(accountFake.IsFinansialLock);
        }
    }
}