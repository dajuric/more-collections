 var lazyMatrixCache = new  LazyMemoryCache<int, double[,]>(maxMemoryOccupation: 0.8f);

 lazyMatrixCache.AddOrUpdate(10, createBigMatrix);
 
 Console.WriteLine("Is element loaded: " + lazyMatrixCache[10].IsValueCreated); //false
 Console.WriteLine("Item (0,0) value: "  + lazyMatrixCache[10].Value[0,0]); //value at (0,0)
 Console.WriteLine("Is element loaded: " + lazyMatrixCache[10].IsValueCreated); //true

 //elements are automatically evicted if RAM occupation exceeds 80%

 //------------------------------------
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

//-----------------------------------
Warning: the application is 32-bit which may cause OutOfMemoryException due to 2GiB limit.