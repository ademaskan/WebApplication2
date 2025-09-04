using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using NetTopologySuite.Geometries;
using System.Threading.Tasks;

namespace BaşarsoftStaj.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ValidationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<object>> ValidateShapeAsync(Shape shape)
        {
            if (shape.Geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                if (shape.Type == "A")
                {
                    var intersectsWithPoints = await _unitOfWork.Points.HasIntersectingPointsAsync(shape.Geometry, new[] { "B" });
                    if (intersectsWithPoints)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"The new LineString of type A cannot intersect with existing Points of type B." //message will be fetched from resx file
                        };
                    }
                }
            }
            else if (shape.Geometry.OgcGeometryType == OgcGeometryType.Point)
            {
                if (shape.Type == "B")
                {
                    var intersectsWithLineStrings = await _unitOfWork.Points.HasIntersectingLineStringsAsync(shape.Geometry, new[] { "A" });
                    if (intersectsWithLineStrings)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "The new Point of type B cannot intersect with existing LineStrings of type A."
                        };
                    }
                }
            }

            return new ApiResponse<object> { Success = true };
        }
    }
}
