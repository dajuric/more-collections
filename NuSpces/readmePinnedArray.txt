var arr = new Point[]{new Point(0, 0), new Point(1, 2) }; //point is struct for (x,y) pair
  
var pinnedArray = new PinnedArray<Point>(arr); //data is shared
  
unsafe
{
    Point* pt = (Point*)pinnedArray.Data;
	pt->X = 3;
}
  
Console.WriteLine(arr[0].X); //outputs "3"