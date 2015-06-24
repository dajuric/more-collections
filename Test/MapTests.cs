using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MoreCollections.Tests
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void TestMain()
        {
            var map = new Map<int, string>();
            map.Add(42, "Hello");

            Assert.IsTrue(map.Forward[42] == "Hello");
            Assert.IsTrue(map.Reverse["Hello"] == 42);
        }
    }
}
