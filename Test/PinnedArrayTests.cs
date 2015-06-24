using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace MoreCollections.Tests
{
    [TestClass]
    public class PinnedArrayTests
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [TestMethod]
        public void TestMain()
        {
            var arr = new Point[] { new Point(0, 0), new Point(1, 2) }; //point is struct for (x,y) pair

            var pinnedArray = new PinnedArray<Point>(arr); //data is shared

            unsafe
            {
                Point* pt = (Point*)pinnedArray.Data;
                pt->X = 3;
            }

            Assert.IsTrue(arr[0].X == 3);
        }
    }
}
