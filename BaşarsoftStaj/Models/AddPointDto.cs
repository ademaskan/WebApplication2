using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Models;

public class AddShapeDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Geometry Geometry { get; set; }
}