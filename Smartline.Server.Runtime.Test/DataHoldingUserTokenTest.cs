using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smartline.Server.Runtime.TransportLayout;


namespace Smartline.Server.Runtime.Test {
    [TestClass]
    public class DataHoldingUserTokenTest {
        private DataHoldingUserToken UserToken;
        private PrivateObject PrivateUserToken;

        [TestInitialize]
        public void Initialize() {
            UserToken = new DataHoldingUserToken(null, null, 0, 0, 0, null, null);
            PrivateUserToken = new PrivateObject(UserToken);
        }

        [TestMethod]
        public void ParseCoreWithoutSyncBytesExpectEmptyParseResult() {
            var buffer = new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(null, parseResult.Buffer);
            Assert.AreEqual(1, parseResult.OkMessageCount);
            Assert.AreEqual(0, parseResult.Packages.Count);
        }

        [TestMethod]
        public void ParseCoreWithOneFullMessageExpectOneMessageParsed() {
            var buffer = new byte[] { 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(1, parseResult.OkMessageCount);
            Assert.AreEqual(1, parseResult.Packages.Count);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[0]);
            Assert.AreEqual(null, parseResult.Buffer);
        }

        [TestMethod]
        public void ParseCoreWithPartOfPreviousMessageAndFullNextMethodExpectOneMessageParsed() {
            var buffer = new byte[] { 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(2, parseResult.OkMessageCount);
            Assert.AreEqual(1, parseResult.Packages.Count);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[0]);
            Assert.AreEqual(null, parseResult.Buffer);
        }

        [TestMethod]
        public void ParseCoreWithPartOfPreviousMessageAndPartOfNextMethodExpectNoMessageParsed() {
            var buffer = new byte[] { 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(1, parseResult.OkMessageCount);
            Assert.AreEqual(0, parseResult.Packages.Count);
            CollectionAssert.AreEqual(new byte[] { 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0 }, parseResult.Buffer);
        }

        [TestMethod]
        public void ParseCoreWithPartOfPreviousMessageAndTwoFullMessageExpectTwoMessageParsed() {
            var buffer = new byte[] { 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 56, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(3, parseResult.OkMessageCount);
            Assert.AreEqual(2, parseResult.Packages.Count);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[0]);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 56, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[1]);
            CollectionAssert.AreEqual(null, parseResult.Buffer);
        }

        [TestMethod]
        public void ParseCoreWithPartOfPreviousMessageAndTwoFullMessageAndPartOfNextMessageExpectTwoMessageParsed() {
            var buffer = new byte[] { 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38, 36, 36, 36, 53, 0, 0, 1, 104, 116, 5, 9, 25, 56, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 38, 38, 38, 36 };
            PrivateUserToken.SetField("LocalBuffer", buffer);
            var parseResult = (DataHoldingUserToken.ParseResult)PrivateUserToken.Invoke("ParseCore");
            Assert.AreEqual(3, parseResult.OkMessageCount);
            Assert.AreEqual(2, parseResult.Packages.Count);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 55, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[0]);
            CollectionAssert.AreEqual(new byte[] { 53, 0, 0, 1, 104, 116, 5, 9, 25, 56, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, parseResult.Packages[1]);
            CollectionAssert.AreEqual(new byte[] { 36 }, parseResult.Buffer);
        }

    }
}
