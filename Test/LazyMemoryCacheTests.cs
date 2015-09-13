using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MoreCollections.Test
{
    [TestClass]
    public class LazyMemoryCacheTests
    {
        LazyMemoryCache<int, double[,]> lazyMatrixCache = null;

        [TestInitialize]
        public void OnBeforeTest()
        {
            if (IntPtr.Size == 4)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning: the application is 32-bit which may cause OutOfMemoryException due to 2GiB limit.");
                Console.ResetColor();
            }

            lazyMatrixCache = new LazyMemoryCache<int, double[,]>(0.9f, false);

            Console.WriteLine("Filling lazy cache (with constructors)...");
            for (int i = 0; i < 50; i++)
            {
                lazyMatrixCache.AddOrUpdate(i, (key) => createBigMatrix(key));
            }
        }

        [TestMethod]
        public void TestElementCreation()
        {
            Assert.IsTrue(lazyMatrixCache[10].IsValueCreated == false);
            Assert.IsTrue(lazyMatrixCache[10].Value[0, 0] == 10);
            Assert.IsTrue(lazyMatrixCache[10].IsValueCreated == true);
        }

        [TestMethod]
        public void TestEnumerable()
        {
            Assert.IsTrue(lazyMatrixCache.Count == 50);

            foreach (var pair in lazyMatrixCache)
            {
                var lazyItem = pair.Value;
                Assert.IsTrue(lazyItem.Value[0, 0] == pair.Key);
            }
        }

        private static double[,] createBigMatrix(int key)
        {
            var mat = new double[1000, 1000];
            for (int r = 0; r < 1000; r++)
            {
                for (int c = 0; c < 1000; c++)
                {
                    mat[r, c] = key;
                }
            }

            System.Threading.Thread.Sleep(60); //additionally slow the operation

            return mat;
        }
    }
}
