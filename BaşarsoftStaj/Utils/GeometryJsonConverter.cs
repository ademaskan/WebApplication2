using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace BaşarsoftStaj.Utils
{
    public class GeometryConverter : JsonConverter<Geometry>
    {
        private readonly WKTReader _wktReader = new WKTReader();
        private readonly WKTWriter _wktWriter = new WKTWriter();
        private readonly GeoJsonReader _geoJsonReader = new GeoJsonReader();
        private readonly GeoJsonWriter _geoJsonWriter = new GeoJsonWriter();

        private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    // Handle WKT string
                    return _wktReader.Read(reader.GetString());
                    
                case JsonTokenType.StartObject:
                    // Handle GeoJSON object
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        var geoJsonString = doc.RootElement.GetRawText();
                        var geometry = _geoJsonReader.Read<Geometry>(geoJsonString);
                        geometry.SRID = _geometryFactory.SRID;
                        return geometry;
                    }
                    
                case JsonTokenType.Null:
                    return null;
                    
                default:
                    throw new JsonException($"Unexpected token type {reader.TokenType} for geometry");
            }
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Write as GeoJSON object
            var geoJson = _geoJsonWriter.Write(value);
            using (var doc = JsonDocument.Parse(geoJson))
            {
                doc.RootElement.WriteTo(writer);
            }
        }
    }
}