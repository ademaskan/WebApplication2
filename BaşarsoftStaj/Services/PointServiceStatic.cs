using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using System.Text.RegularExpressions;
using NetTopologySuite.IO;

namespace BaşarsoftStaj.Services;

public class PointServiceStatic : IPointService
{
    private static List<PointE> _points = new List<PointE>();
    private static int _idCounter = 1;
    private static readonly WKTReader _wktReader = new WKTReader();

    public ApiResponse<List<PointE>> GetAllPoints()
    {
        return ApiResponse<List<PointE>>.SuccessResponse(_points, "PointsRetrievedSuccessfully");
    }

    public ApiResponse<PointE> GetPointById(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        return ApiResponse<PointE>.SuccessResponse(point, "PointRetrievedSuccessfully");
    }

    public ApiResponse<PointE> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT))
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        if (!IsValidWkt(pointDto.WKT))
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }

        try
        {
            var point = new PointE
            {
                Id = _idCounter++,
                Name = pointDto.Name,
                WKT = pointDto.WKT // Geometry conversion happens automatically
            };

            _points.Add(point);
            return ApiResponse<PointE>.SuccessResponse(point, "PointAddedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<List<PointE>> AddRangePoints(List<AddPointDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<PointE>>.ErrorResponse("InvalidInput");
        }

        var validPoints = new List<PointE>();

        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT) || !IsValidWkt(pointDto.WKT))
            {
                return ApiResponse<List<PointE>>.ErrorResponse("ValidationError");
            }

            try
            {
                var point = new PointE
                {
                    Id = _idCounter++,
                    Name = pointDto.Name,
                    WKT = pointDto.WKT // Geometry conversion happens automatically
                };
                validPoints.Add(point);
            }
            catch (Exception)
            {
                return ApiResponse<List<PointE>>.ErrorResponse("InvalidWktFormat");
            }
        }

        _points.AddRange(validPoints);
        return ApiResponse<List<PointE>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<PointE> UpdatePoint(int id, string newName, string newWkt)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }

        if (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newWkt))
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        if (!string.IsNullOrEmpty(newWkt) && !IsValidWkt(newWkt))
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }

        try
        {
            if (!string.IsNullOrEmpty(newName))
                point.Name = newName;

            if (!string.IsNullOrEmpty(newWkt))
                point.WKT = newWkt; // Geometry conversion happens automatically

            return ApiResponse<PointE>.SuccessResponse(point, "PointUpdatedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<PointE> DeletePoint(int id)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        
        _points.Remove(point);
        return ApiResponse<PointE>.SuccessResponse(point, "PointDeletedSuccessfully");
    }

    private bool IsValidWkt(string wkt)
    {
        if (string.IsNullOrEmpty(wkt))
            return false;
        
        var patterns = new[]
        {
            @"^POINT\s*\(\s*-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*\)$",
            @"^LINESTRING\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)$",
            @"^POLYGON\s*\(\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)\s*\)$"
        };

        return patterns.Any(pattern => Regex.IsMatch(wkt.Trim(), pattern, RegexOptions.IgnoreCase));
    }
}
