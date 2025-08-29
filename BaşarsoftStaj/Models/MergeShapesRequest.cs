using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace BaşarsoftStaj.Models
{
    public class MergeShapesRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Geometry Geometry { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public int[] DeleteIds { get; set; }
    }
}
