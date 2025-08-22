using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;



namespace BaşarsoftStaj.Entity;

public class PointE
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Geometry Geometry { get; set; }
    
}