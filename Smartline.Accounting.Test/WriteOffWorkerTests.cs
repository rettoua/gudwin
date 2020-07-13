using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smartline.Mapping;

namespace Smartline.Accounting.Test {
    [TestClass]
    public class WriteOffWorkerTests {
        [TestMethod]
        public void ProcessWaitingWriteOffsExpectStateUpdated() {
            var waitingWriteOffs = new List<WriteOff> {
                                                                        new WriteOff { State = TransactionState.Waiting },
                                                                        new WriteOff { State = TransactionState.Waiting }
                                                                    };
            var fake = A.Fake<IAccountingWriteOffsProvider>();
            fake.CallsTo(x => x.GetWriteOffTransactions(TransactionState.Waiting)).Returns(waitingWriteOffs);
            var worker = new WriteOffWorker(fake);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessWaitingWriteOffs", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (WriteOff writeOff in waitingWriteOffs) {
                Assert.AreEqual(TransactionState.Pending, writeOff.State);
            }
        }

        [TestMethod]
        public void ProcessPendingWriteOffsExpectStateUpdated() {
            var waitingWriteOffs = new List<WriteOff> {                                                                      
                new WriteOff { State = TransactionState.Waiting, UserId = 1,TrackerUid = 2,Time = DateTime.Now}                                                                        
                                                                    };
            var fakeAccount = A.Fake<Account>();
            fakeAccount.CallsTo(x => x.Save()).Returns(true);

            var fakeProvider = A.Fake<IAccountingWriteOffsProvider>();
            fakeProvider.CallsTo(x => x.GetWriteOffTransactions(TransactionState.Pending)).Returns(waitingWriteOffs);
            fakeProvider.CallsTo(x => x.SaveWriteOff(null)).Returns(true);
            fakeProvider.CallsTo(x => x.GetAccount(1)).Returns(fakeAccount);

            var worker = new WriteOffWorker(fakeProvider);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessPendingWriteOffs", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (WriteOff writeOff in waitingWriteOffs) {
                Assert.AreEqual(TransactionState.Commited, writeOff.State);
                Assert.IsTrue(fakeAccount.WriteOffTransactions.Contains(writeOff.GetId()));
                Assert.AreEqual(fakeAccount.WriteOffs[writeOff.TrackerUid], writeOff.Time);
            }
        }

        [TestMethod]
        public void ProcessPendingWriteOffsWithTransactionsExpectStateUpdated() {
            var waitingWriteOffs = new List<WriteOff> {                                                                      
                new WriteOff { State = TransactionState.Waiting, UserId = 1,TrackerUid = 2,Time = DateTime.Now}                                                                        
                                                                    };
            var fakeAccount = A.Fake<Account>();
            fakeAccount.CallsTo(x => x.Save()).Returns(true);
            fakeAccount.WriteOffTransactions.Add(waitingWriteOffs.First().GetId());

            var fakeProvider = A.Fake<IAccountingWriteOffsProvider>();
            fakeProvider.CallsTo(x => x.GetWriteOffTransactions(TransactionState.Pending)).Returns(waitingWriteOffs);
            fakeProvider.CallsTo(x => x.SaveWriteOff(null)).Returns(true);
            fakeProvider.CallsTo(x => x.GetAccount(1)).Returns(fakeAccount);

            var worker = new WriteOffWorker(fakeProvider);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessPendingWriteOffs", BindingFlags.Instance | BindingFlags.NonPublic);

            fakeAccount.CallsTo(x => x.Save()).MustNotHaveHappened();

            foreach (WriteOff writeOff in waitingWriteOffs) {
                Assert.AreEqual(TransactionState.Commited, writeOff.State);
                Assert.IsTrue(fakeAccount.WriteOffTransactions.Contains(writeOff.GetId()));
            }
        }

        [TestMethod]
        public void ProcessCommmitedWriteOffsWithTransactionsExpectStateUpdated() {
            var commitedWriteOffs = new List<WriteOff> {                                                                      
                new WriteOff { State = TransactionState.Waiting, UserId = 1,TrackerUid = 2,Time = DateTime.Now, Amount = 10}                                                                        
                                                                    };
            var fakeAccount = A.Fake<Account>();
            fakeAccount.Amount = 50;
            fakeAccount.CallsTo(x => x.Save()).Returns(true);
            fakeAccount.WriteOffTransactions.Add(commitedWriteOffs.First().GetId());

            var fakeProvider = A.Fake<IAccountingWriteOffsProvider>();
            fakeProvider.CallsTo(x => x.GetWriteOffTransactions(TransactionState.Commited)).Returns(commitedWriteOffs);
            fakeProvider.CallsTo(x => x.SaveWriteOff(null)).Returns(true);
            fakeProvider.CallsTo(x => x.GetAccount(1)).Returns(fakeAccount);

            var worker = new WriteOffWorker(fakeProvider);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessCommmitedWriteOffs", BindingFlags.Instance | BindingFlags.NonPublic);

            fakeAccount.CallsTo(x => x.Save()).MustHaveHappened(Repeated.Exactly.Once);

            foreach (WriteOff writeOff in commitedWriteOffs) {
                Assert.AreEqual(TransactionState.Done, writeOff.State);
            }
            Assert.AreEqual(0, fakeAccount.WriteOffTransactions.Count);
            Assert.AreEqual(50, commitedWriteOffs.First().AmountBefore);
            Assert.AreEqual(40, commitedWriteOffs.First().AmountAfter);
            Assert.AreEqual(40, fakeAccount.Amount);
        }

        [TestMethod]
        public void ProcessCommmitedWriteOffsWithoutTransactionsExpectStateUpdated() {
            var commitedWriteOffs = new List<WriteOff> {                                                                      
                new WriteOff { State = TransactionState.Waiting, UserId = 1,TrackerUid = 2,Time = DateTime.Now, Amount = 10}                                                                        
                                                                    };
            var fakeAccount = A.Fake<Account>();
            fakeAccount.CallsTo(x => x.Save()).Returns(true);

            var fakeProvider = A.Fake<IAccountingWriteOffsProvider>();
            fakeProvider.CallsTo(x => x.GetWriteOffTransactions(TransactionState.Commited)).Returns(commitedWriteOffs);
            fakeProvider.CallsTo(x => x.SaveWriteOff(null)).Returns(true);
            fakeProvider.CallsTo(x => x.GetAccount(1)).Returns(fakeAccount);

            var worker = new WriteOffWorker(fakeProvider);
            var privateObject = new PrivateObject(worker);
            privateObject.Invoke("ProcessCommmitedWriteOffs", BindingFlags.Instance | BindingFlags.NonPublic);

            fakeAccount.CallsTo(x => x.Save()).MustNotHaveHappened();

            foreach (WriteOff writeOff in commitedWriteOffs) {
                Assert.AreEqual(TransactionState.Done, writeOff.State);
            }
            Assert.AreEqual(0, fakeAccount.WriteOffTransactions.Count);
            Assert.AreEqual(0, fakeAccount.Amount);
            Assert.AreEqual(10, commitedWriteOffs.First().Amount);
        }
    }
}