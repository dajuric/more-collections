using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MoreCollections.Tests
{
    [TestClass]
    public class HistoryTests
    {
        [TestMethod]
        public void TestMain()
        {
            var hist = new History<string>(maxCount: 2);

            hist.Add("one");
            hist.Add("two");
            hist.Add("three");

            Assert.IsTrue(hist.Count == 2);
            Assert.IsTrue(hist[0] == "three");
            Assert.IsTrue(hist[1] == "two");
        }
    }
}
