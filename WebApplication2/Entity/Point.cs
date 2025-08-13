namespace WebApplication2.Entity;

public class Point
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string WKT { get; set; } // Well-Known Text representation of the point. Point, LineString, Polygon, etc.

}