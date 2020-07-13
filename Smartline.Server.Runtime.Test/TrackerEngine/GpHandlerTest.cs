using System;
using System.Collections.Generic;
using System.Reflection;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smartline.Common.Runtime;
using Smartline.Mapping;
using Smartline.Server.Runtime.TrackerEngine;

namespace Smartline.Server.Runtime.Test.TrackerEngine {
    [TestClass]
    public class GpHandlerTest {

        [TestMethod]
        public void IsParkingExpectTrue() {
            var gpHandler = new GpHandler(new Tracker(), null, null, null);
            var list = new List<Gp> {
                                         new Gp { Speed = 0, Distance = 1 },
                                         new Gp { Speed = 0, Distance = 0 },
                                         new Gp { Speed = 1, Distance = 0 },                                         
                                         new Gp { Speed = 1, Distance = null },                                         
                                    };

            var privateObject = new PrivateObject(gpHandler);
            foreach (Gp gp in list) {
                Assert.IsTrue((bool)privateObject.Invoke("IsParking", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { gp }));
            }
        }

        [TestMethod]
        public void IsParkingExpectFalse() {
            var gpHandler = new GpHandler(new Tracker(), null, null, null);
            var list = new List<Gp> {
                                         new Gp { Speed = 1, Distance = 1 },
                                         new Gp { Speed = 15, Distance = 25 },                                         
                                    };

            var privateObject = new PrivateObject(gpHandler);
            foreach (Gp gp in list) {
                Assert.IsFalse((bool)privateObject.Invoke("IsParking", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { gp }));
            }
        }

        [TestMethod]
        public void IsActualTimeExpectTrue() {
            var gpHandler = new GpHandler(new Tracker(), null, null, null);
            var list = new List<Gp> {
                                         new Gp { SendTime = DateTime.Now},
                                         new Gp { SendTime = DateTime.Now.AddMinutes(-10)},
                                         new Gp { SendTime = DateTime.Now.AddMinutes(10)},
                                         new Gp { SendTime = DateTime.Now.AddMinutes(29)},
                                    };

            var privateObject = new PrivateObject(gpHandler);
            foreach (Gp gp in list) {
                Assert.IsTrue((bool)privateObject.Invoke("IsActualTime", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { gp }));
            }
        }

        [TestMethod]
        public void IsActualTimeExpectFalse() {
            var gpHandler = new GpHandler(new Tracker(), null, null, null);
            var list = new List<Gp> {
                                         new Gp { SendTime    = DateTime.Now.AddMinutes(30)},
                                         new Gp { SendTime    = DateTime.Now.AddMinutes(35)}                                         
                                    };

            var privateObject = new PrivateObject(gpHandler);
            foreach (Gp gp in list) {
                Assert.IsFalse((bool)privateObject.Invoke("IsActualTime", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { gp }));
            }
        }

        [TestMethod]
        public void SetNewPointWithTwoMovingPointsExpectLastPointUpdated() {
            var gp1 = new Gp { Speed = 5, Distance = 25 };
            var gp2 = new Gp { Speed = 20, Distance = 400 };
            var gpHandler = new GpHandler(new Tracker(), null, A.Fake<IGpStorage>(), A.Fake<ISensorHandler>());
            var privateObject = new PrivateObject(gpHandler);
            gpHandler.SetNewPoint(gp1);
            Assert.AreEqual(gp1, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
            gpHandler.SetNewPoint(gp2);
            Assert.AreEqual(gp2, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
        }

        [TestMethod]
        public void SetNewPointWithTwoParkingPointsExpectLastPointUpdated() {
            var gp1 = new Gp { Speed = 0, Distance = 0, SendTime = DateTime.Now };
            var gp2 = new Gp { Speed = 0, Distance = 0, SendTime = DateTime.Now.AddSeconds(10) };
            var gpHandler = new GpHandler(new Tracker(), null, A.Fake<IGpStorage>(), A.Fake<ISensorHandler>());
            var privateObject = new PrivateObject(gpHandler);
            gpHandler.SetNewPoint(gp1);
            Assert.AreEqual(gp1, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
            gpHandler.SetNewPoint(gp2);
            Assert.AreEqual(gp1, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
            Assert.AreEqual(gp1.EndTime, gp2.SendTime);
        }

        [TestMethod]
        public void SetNewPointWithParkingAndMovingPointsExpectLastPointUpdated() {
            var gp1 = new Gp { Speed = 1, Distance = 15, SendTime = DateTime.Now };
            var gp2 = new Gp { Speed = 0, Distance = 0, SendTime = DateTime.Now.AddSeconds(10) };
            var gpHandler = new GpHandler(new Tracker(), null, A.Fake<IGpStorage>(), A.Fake<ISensorHandler>());
            var privateObject = new PrivateObject(gpHandler);
            gpHandler.SetNewPoint(gp1);
            Assert.AreEqual(gp1, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
            gpHandler.SetNewPoint(gp2);
            Assert.AreEqual(gp2, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));            
        }

        [TestMethod]
        public void SetNewPointWithMovingAndParkingPointsExpectLastPointUpdated() {
            var gp1 = new Gp { Speed = 0, Distance = 0, SendTime = DateTime.Now.AddSeconds(10) };
            var gp2 = new Gp { Speed = 1, Distance = 15, SendTime = DateTime.Now };
            var gpHandler = new GpHandler(new Tracker(), null, A.Fake<IGpStorage>(), A.Fake<ISensorHandler>());
            var privateObject = new PrivateObject(gpHandler);
            gpHandler.SetNewPoint(gp1);
            Assert.AreEqual(gp1, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));
            gpHandler.SetNewPoint(gp2);
            Assert.AreEqual(gp2, (Gp)privateObject.GetField("_lastGpPoint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField));            
        }
    }
}