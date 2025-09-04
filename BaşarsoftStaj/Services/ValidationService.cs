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
            var rules = await _context.Rules
                .Where(r => r.Enabled && r.GeometryType == shape.Geometry.OgcGeometryType.ToString())
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (shape.Type == rule.ShapeType)
                {
                    if (rule.ValidationType == "CannotIntersect")
                    {
                        var intersects = await CheckIntersection(shape, rule, pendingShapes);
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
            }

            return new ApiResponse<object> { Success = true };
        }

        private async Task<bool> CheckIntersection(Shape shape, Rule rule, List<Shape>? pendingShapes)
        {
            if (Enum.TryParse<OgcGeometryType>(rule.RelatedGeometryType, true, out var ogcGeometryType))
            {
                var geometryToValidate = shape.Geometry;
                if (rule.Buffer > 0)
                {
                    geometryToValidate = geometryToValidate.Buffer(rule.Buffer);
                }

                // Check against database
                var dbIntersects = await _unitOfWork.Shapes.AnyAsync(s =>
                    s.Geometry.OgcGeometryType == ogcGeometryType &&
                    s.Type == rule.RelatedShapeType &&
                    s.Geometry.Intersects(geometryToValidate));

                if (dbIntersects) return true;

                // Check against pending shapes
                if (pendingShapes != null)
                {
                    var pendingIntersects = pendingShapes.Any(s =>
                        s.Geometry.OgcGeometryType == ogcGeometryType &&
                        s.Type == rule.RelatedShapeType &&
                        s.Geometry.Intersects(geometryToValidate));
                    
                    if (pendingIntersects) return true;
                }
            }
            return false;
        }
    }
}
