using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using BaşarsoftStaj.Data;
using System.Text.RegularExpressions;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Services;

public class ShapeServiceEFC : IShapeService
{
    private readonly AppDbContext _context;

    public ShapeServiceEFC(AppDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<Shape>> GetAllPoints()
    {
        var points = _context.PointsEF.ToList();
        return ApiResponse<List<Shape>>.SuccessResponse(points, "PointsRetrievedSuccessfully");
    }

    public ApiResponse<Shape> GetPointById(int id)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        return ApiResponse<Shape>.SuccessResponse(point, "PointRetrievedSuccessfully");
    }

    public ApiResponse<Shape> AddPoint(AddShapeDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
        {
            return ApiResponse<Shape>.ErrorResponse("ValidationError");
        }

        try
        {
            var point = new Shape
            {
                Name = pointDto.Name,
                Geometry = pointDto.Geometry
            };

            _context.PointsEF.Add(point);
            _context.SaveChanges();

            return ApiResponse<Shape>.SuccessResponse(point, "PointAddedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<Shape>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<List<Shape>> AddRangePoints(List<AddShapeDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<Shape>>.ErrorResponse("InvalidInput");
        }

        var validPoints = new List<Shape>();

        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
            {
                return ApiResponse<List<Shape>>.ErrorResponse("ValidationError");
            }

            try
            {
                var point = new Shape
                {
                    Name = pointDto.Name,
                    Geometry = pointDto.Geometry
                };
                validPoints.Add(point);
            }
            catch (Exception)
            {
                return ApiResponse<List<Shape>>.ErrorResponse("InvalidWktFormat");
            }
        }

        _context.PointsEF.AddRange(validPoints);
        _context.SaveChanges();

        return ApiResponse<List<Shape>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<Shape> UpdatePoint(int id, string newName, Geometry newGeometry)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
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

            _context.SaveChanges();

            return ApiResponse<Shape>.SuccessResponse(point, "PointUpdatedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<Shape>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<Shape> DeletePoint(int id)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        
        _context.PointsEF.Remove(point);
        _context.SaveChanges();

        return ApiResponse<Shape>.SuccessResponse(point, "PointDeletedSuccessfully");
    }
}
