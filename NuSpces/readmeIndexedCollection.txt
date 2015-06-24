var arr = new string[]{"three", "one", "two"};
var indexes = new int[]{1, 2, 0};
  
var indexedCollection = new IndexedCollection<string>(arr, indexes);
  
//outputs: "one", "two", "three"
foreach(var item in indexedCollection)
{
    Console.Write(" " + item);
}