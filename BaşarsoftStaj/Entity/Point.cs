using System.ComponentModel.DataAnnotations;

namespace BaşarsoftStaj.Entity;

public class Point
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string WKT { get; set; } = string.Empty; // Well-Known Text representation of the point. Point, LineString, Polygon, etc.
}