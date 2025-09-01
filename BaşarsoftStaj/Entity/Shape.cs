using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;


namespace BaşarsoftStaj.Entity;

public class Shape
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Geometry Geometry { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    public string? ImagePath { get; set; }
    
    [NotMapped] 
    public string Wkt => new WKTWriter().Write(Geometry);
}