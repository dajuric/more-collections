var map = new Map<int, string>();
map.Add(42, "Hello");
Console.WriteLine(map.Forward[42]); // Outputs "Hello"
Console.WriteLine(map.Reverse["Hello"]); //Outputs 42