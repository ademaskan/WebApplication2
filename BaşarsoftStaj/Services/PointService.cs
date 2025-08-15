using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using System.Text.RegularExpressions;

namespace BaşarsoftStaj.Services;

public class PointService : IPointService
{
    private static List<Point> _points = new List<Point>();
    private static int _idCounter = 1;

    public ApiResponse<List<Point>> GetAllPoints()
    {
        return ApiResponse<List<Point>>.SuccessResponse(_points, "PointsRetrievedSuccessfully");
    }

    public ApiResponse<Point> GetPointById(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }
        return ApiResponse<Point>.SuccessResponse(point, "PointRetrievedSuccessfully");
    }

    public ApiResponse<Point> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT))
        {
            return ApiResponse<Point>.ErrorResponse("ValidationError");
        }

        if (!IsValidWkt(pointDto.WKT))
        {
            return ApiResponse<Point>.ErrorResponse("InvalidWktFormat");
        }

        var point = new Point
        {
            Id = _idCounter++,
            Name = pointDto.Name,
            WKT = pointDto.WKT
        };

        _points.Add(point);
        return ApiResponse<Point>.SuccessResponse(point, "PointAddedSuccessfully");
    }

    public ApiResponse<List<Point>> AddRangePoints(List<AddPointDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<Point>>.ErrorResponse("InvalidInput");
        }

        var validPoints = new List<Point>();

        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT) || !IsValidWkt(pointDto.WKT))
            {
                return ApiResponse<List<Point>>.ErrorResponse("ValidationError");
            }

            var point = new Point
            {
                Id = _idCounter++,
                Name = pointDto.Name,
                WKT = pointDto.WKT
            };
            validPoints.Add(point);
        }

        _points.AddRange(validPoints);
        return ApiResponse<List<Point>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<Point> UpdatePoint(int id, string newName, string newWkt)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }

        if (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newWkt))
        {
            return ApiResponse<Point>.ErrorResponse("ValidationError");
        }

        if (!string.IsNullOrEmpty(newWkt) && !IsValidWkt(newWkt))
        {
            return ApiResponse<Point>.ErrorResponse("InvalidWktFormat");
        }

        if (!string.IsNullOrEmpty(newName))
            point.Name = newName;

        if (!string.IsNullOrEmpty(newWkt))
            point.WKT = newWkt;

        return ApiResponse<Point>.SuccessResponse(point, "PointUpdatedSuccessfully");
    }

    public ApiResponse<Point> DeletePoint(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }
        _points.Remove(point);

        return ApiResponse<Point>.SuccessResponse(point, "PointDeletedSuccessfully");
    }

    private bool IsValidWkt(string wkt)
    {
        if (string.IsNullOrEmpty(wkt))
            return false;

        // Basic WKT validation patterns
        var patterns = new[]
        {
            @"^POINT\s*\(\s*-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*\)$",
            @"^LINESTRING\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)$",
            @"^POLYGON\s*\(\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)\s*\)$"
        };

        return patterns.Any(pattern => Regex.IsMatch(wkt.Trim(), pattern, RegexOptions.IgnoreCase));
    }
}
