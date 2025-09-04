using BaşarsoftStaj.Data;
using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaşarsoftStaj.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public ValidationService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ApiResponse<object>> ValidateShapeAsync(Shape shape, List<Shape>? pendingShapes = null)
        {
            var shapeGeomTypeStr = shape.Geometry.OgcGeometryType.ToString();
            var rules = await _context.Rules
                .Where(r => r.Enabled && (r.GeometryType == shapeGeomTypeStr || r.RelatedGeometryType == shapeGeomTypeStr))
                .ToListAsync();

            foreach (var rule in rules)
            {
                string targetGeometryType;
                string targetShapeType;

                var isSourceMatch = shapeGeomTypeStr == rule.GeometryType && shape.Type == rule.ShapeType;
                var isRelatedMatch = shapeGeomTypeStr == rule.RelatedGeometryType && shape.Type == rule.RelatedShapeType;

                if (isSourceMatch)
                {
                    targetGeometryType = rule.RelatedGeometryType;
                    targetShapeType = rule.RelatedShapeType;
                }
                else if (isRelatedMatch)
                {
                    targetGeometryType = rule.GeometryType;
                    targetShapeType = rule.ShapeType;
                }
                else
                {
                    continue; 
                }

                if (rule.ValidationType == "CannotIntersect")
                {
                    var intersects = await CheckIntersection(shape, rule.Buffer, targetGeometryType, targetShapeType, pendingShapes);
                    if (intersects)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Validation failed: {rule.Name}. {rule.Description}"
                        };
                    }
                }
            }

            return new ApiResponse<object> { Success = true };
        }

        private async Task<bool> CheckIntersection(Shape shape, double buffer, string relatedGeometryType, string relatedShapeType, List<Shape>? pendingShapes)
        {
            if (Enum.TryParse<OgcGeometryType>(relatedGeometryType, true, out var ogcGeometryType))
            {
                var geometryToValidate = shape.Geometry;
                if (buffer > 0)
                {
                    // Assuming buffer is in meters, convert to degrees (approximate)
                    // 1 degree of latitude is approximately 111.32 km
                    const double metersInOneDegree = 111320; 
                    var bufferInDegrees = buffer / metersInOneDegree;
                    geometryToValidate = geometryToValidate.Buffer(bufferInDegrees);
                }

                // db check
                var dbIntersects = await _unitOfWork.Shapes.AnyAsync(s =>
                    s.Geometry.OgcGeometryType == ogcGeometryType &&
                    s.Type == relatedShapeType &&
                    s.Geometry.Intersects(geometryToValidate));

                if (dbIntersects) return true;

                
                if (pendingShapes != null)
                {
                    var pendingIntersects = pendingShapes.Any(s =>
                        s.Geometry.OgcGeometryType == ogcGeometryType &&
                        s.Type == relatedShapeType &&
                        s.Geometry.Intersects(geometryToValidate));
                    
                    if (pendingIntersects) return true;
                }
            }
            return false;
        }
    }
}
