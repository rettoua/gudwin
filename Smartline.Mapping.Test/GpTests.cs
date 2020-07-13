using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smartline.Common.Runtime;

namespace Smartline.Mapping.Test {
    [TestClass]
    public class GpTests {

        [TestMethod]
        public void EqualsWithSameObjectExpectTrue() {
            var obj1 = new Gp {
                SendTime = DateTime.Now,
                Latitude = 3,
                Longitude = 4
            };
            Gp obj2 = obj1.Clone();
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void EqualsWithDifferentSpeedExpectFalse() {
            var obj1 = new Gp {
                SendTime = DateTime.Now,
                Latitude = 3,
                Longitude = 4
            };
            Gp obj2 = obj1.Clone();
            obj1.Speed = 3;
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void EqualsWithDifferentDistanceExpectFalse() {
            var obj1 = new Gp {
                SendTime = DateTime.Now,
                Latitude = 3,
                Longitude = 4
            };
            Gp obj2 = obj1.Clone();
            obj1.Distance = 3;
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void EqualsWithDifferentSendTimeExpectFalse() {
            var obj1 = new Gp {
                SendTime = DateTime.Now,
                Latitude = 3,
                Longitude = 4
            };
            Gp obj2 = obj1.Clone();
            obj1.SendTime = DateTime.Now.AddSeconds(1);
            Assert.AreNotEqual(obj1, obj2);
        }

        [TestMethod]
        public void AssignExpectObjectsAreEquals() {
            var obj1 = new Gp();
            var obj2 = new Gp {
                SendTime = DateTime.Now,
                Latitude = 3,
                Longitude = 4,
                Speed = 5,
                Distance = 6
            };
            obj1.Assign(obj2);
            Assert.AreEqual(obj1, obj2);
        }

    }
}
