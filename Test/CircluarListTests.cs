using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MoreCollections.Tests
{
    [TestClass]
    public class CircluarListTests
    {
        [TestMethod]
        public void TestMain()
        {
            var list = new string[] { "one", "two", "three" }.ToList();

            var circularList = list.ToCircularList();
            Assert.IsTrue(circularList[-1] == "three");
            Assert.IsTrue(circularList[6] == "one");
        }
    }
}
