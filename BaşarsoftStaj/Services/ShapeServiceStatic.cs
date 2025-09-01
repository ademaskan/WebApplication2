using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;


namespace BaşarsoftStaj.Services;

public class ShapeServiceStatic : IShapeService
{
    private static List<Shape> _points = new List<Shape>();
    private static int _idCounter = 1;

    public ApiResponse<List<Shape>> GetAllPoints()
    {
        return ApiResponse<List<Shape>>.SuccessResponse(_points, "PointsRetrievedSuccessfully");
    }

    public ApiResponse<Shape> GetPointById(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        return ApiResponse<Shape>.SuccessResponse(point, "PointRetrievedSuccessfully");
    }

    public ApiResponse<Shape> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.Geometry))
        {
            return ApiResponse<Shape>.ErrorResponse("ValidationError");
        }

        try
        {
            var reader = new GeoJsonReader();
            var geometry = reader.Read<Geometry>(pointDto.Geometry);
            var point = new Shape
            {
                Id = _idCounter++,
                Name = pointDto.Name,
                Geometry = geometry
            };

            _points.Add(point);
            return ApiResponse<Shape>.SuccessResponse(point, "PointAddedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<Shape>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<List<Shape>> AddRangePoints(List<AddPointDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<Shape>>.ErrorResponse("InvalidInput");
        }

        var validPoints = new List<Shape>();
        var reader = new GeoJsonReader();

        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.Geometry))
            {
                return ApiResponse<List<Shape>>.ErrorResponse("ValidationError");
            }

            try
            {
                var geometry = reader.Read<Geometry>(pointDto.Geometry);
                var point = new Shape
                {
                    Id = _idCounter++,
                    Name = pointDto.Name,
                    Geometry = geometry
                };
                validPoints.Add(point);
            }
            catch (Exception)
            {
                return ApiResponse<List<Shape>>.ErrorResponse("InvalidWktFormat");
            }
        }

        _points.AddRange(validPoints);
        return ApiResponse<List<Shape>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<Shape> UpdatePoint(int id, string newName, Geometry newGeometry)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }

        if (string.IsNullOrEmpty(newName) && newGeometry == null)
        {
            return ApiResponse<Shape>.ErrorResponse("ValidationError");
        }

        try
        {
            if (!string.IsNullOrEmpty(newName))
                point.Name = newName;

            if (newGeometry != null)
                point.Geometry = newGeometry; 

            return ApiResponse<Shape>.SuccessResponse(point, "PointUpdatedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<Shape>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<Shape> DeletePoint(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        
        _points.Remove(point);
        return ApiResponse<Shape>.SuccessResponse(point, "PointDeletedSuccessfully");
    }
}
