
using System.ComponentModel.DataAnnotations;

namespace BaşarsoftStaj.Entity
{
    public class Rule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string GeometryType { get; set; } = string.Empty; 

        [Required]
        public string ShapeType { get; set; } = string.Empty; 

        [Required]
        public string RelatedGeometryType { get; set; } = string.Empty; 

        [Required]
        public string RelatedShapeType { get; set; } = string.Empty; 

        [Required]
        public string ValidationType { get; set; } = string.Empty; 

        public double Buffer { get; set; } = 0;

        public bool Enabled { get; set; } = true;
    }
}
