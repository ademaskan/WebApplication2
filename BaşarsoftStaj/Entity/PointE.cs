using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json.Serialization;

namespace BaşarsoftStaj.Entity;

public class PointE
{
    private static readonly WKTReader _wktReader = new WKTReader();
    private string _wkt = string.Empty;
    private Geometry? _wellKnownText;

    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string WKT 
    { 
        get => _wkt;
        set
        {
            _wkt = value;
            // Automatically convert WKT to geometry when WKT is set
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    _wellKnownText = _wktReader.Read(value);
                }
                catch
                {
                    _wellKnownText = null;
                }
            }
            else
            {
                _wellKnownText = null;
            }
        }
    }
    
    [Required]
    [JsonIgnore] // Exclude from JSON serialization to prevent infinite value errors
    public Geometry WellKnownText 
    { 
        get => _wellKnownText ?? throw new InvalidOperationException("WellKnownText is not initialized. Set WKT property first.");
        set => _wellKnownText = value;
    }
}