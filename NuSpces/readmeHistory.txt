var hist = new History<string>(maxCount: 2);
  
hist.Add("one");
hist.Add("two");
hist.Add("three");
  
//outputs: "three", "two"
foreach(var item in hist)
{
    Console.Write(" " + item);
}