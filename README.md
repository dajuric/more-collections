# More collections

Provides a set of portable collections installable as source files via NuGet.

## Collections / NuGet packages

  + <a href="https://www.nuget.org/packages/MoreCollections.Map">MoreCollections.Map</a>  
  Two-way dictionary (keyA-keyB).

   ``` csharp
  var map = new Map<int, string>();
  map.Add(42, "Hello");
  Console.WriteLine(map.Forward[42]); // Outputs "Hello"
  Console.WriteLine(map.Reverse["Hello"]); //Outputs 42
    ```

  + <a href="https://www.nuget.org/packages/MoreCollections.CirclularList">MoreCollections.CircularList</a>  
  List which supports modulo indexing.

  ``` csharp
  var list = new string[]{"one", "two", "three"}.ToList();
  
  var circularList = list.ToCircularList();
  Console.WriteLine(circularList[-1]); //outputs "three"
  Console.WriteLine(circularList[6]); //outputs "one"
  ```
  
  + <a href="https://www.nuget.org/packages/MoreCollections.History">MoreCollections.History</a>  
  Stack-like structure with predefined capacity.

  ``` csharp
  var hist = new History<string>(maxCount: 2);
  
  hist.Add("one");
  hist.Add("two");
  hist.Add("three");
  
  //outputs: "three", "two"
  foreach(var item in hist)
  {
     Console.Write(" " + item);
  }
  ```
  
  + <a href="https://www.nuget.org/packages/MoreCollections.IndexedCollection">MoreCollections.IndexedCollection</a>  
  Indirect indexing collection.

  ``` csharp
  var arr = new string[]{"three", "one", "two"};
  var indexes = new int[]{1, 2, 0};
  
  var indexedCollection = new IndexedCollection<string>(arr, indexes);
  
  //outputs: "one", "two", "three"
  foreach(var item in indexedCollection)
  {
     Console.Write(" " + item);
  }
  ```
  
  + <a href="https://www.nuget.org/packages/MoreCollections.PinnedArray">MoreCollections.PinnedArray</a>  
  Pinned array where elements are blittable-type objects.

  ``` csharp  
  var arr = new Point[]{new Point(0, 0), new Point(1, 2) }; //point is struct for (x,y) pair
  
  var pinnedArray = new PinnedArray<Point>(arr); //data is shared
  
  unsafe
  {
     Point* pt = (Point*)pinnedArray.Data;
	 pt->X = 3;
  }
  
  Console.WriteLine(arr[0].X); //outputs "3"
  ```

  + <a href="https://www.nuget.org/packages/MoreCollections.LazyMemoryCache">MoreCollections.LazyMemoryCache</a>  
  Lazy object loading-unloading for huge collections which do not fit into RAM.

  ``` csharp  
  var lazyMatrixCache = new  LazyMemoryCache<int, double[,]>(maxMemoryOccupation: 0.8f);

  //load items by calling lazyMatrixCache.AddOrUpdate(<key>, <matrix constructor>)
 
  Console.WriteLine("Is element loaded: " + lazyMatrixCache[10].IsValueCreated); //false
  Console.WriteLine("Item (0,0) value: "  + lazyMatrixCache[10].Value[0,0]); //value at (0,0)
  Console.WriteLine("Is element loaded: " + lazyMatrixCache[10].IsValueCreated); //true

  //elements are automatically evicted if RAM occupation exceeds 80%
  ```

## How to Engage, Contribute and Provide Feedback  
Remember: Your opinion is important and will define the future roadmap.
+ questions, comments - message on Github, or write to: darko.juric2 [at] gmail.com
+ code contributions are welcome
+ **spread the word** 

## Final word
If you like the project please **star it** in order to help to spread the word. That way you will make the framework more significant and in the same time you will motivate me to improve it, so the benefit is mutual.
