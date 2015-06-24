using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MoreCollections.Tests
{
    [TestClass]
    public class IndexedCollectionTests
    {
        [TestMethod]
        public void TestMain()
        {
            var arr = new string[] { "three", "one", "two" };
            var indices = new int[] { 1, 2, 0 };

            var indexedCollection = new IndexedCollection<string>(arr, indices);

            Assert.IsTrue(indexedCollection[0] == "one");
            Assert.IsTrue(indexedCollection[1] == "two");
            Assert.IsTrue(indexedCollection[2] == "three");
        }
    }
}
