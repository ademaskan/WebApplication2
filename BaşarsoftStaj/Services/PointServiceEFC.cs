using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using BaşarsoftStaj.Data;
using System.Text.RegularExpressions;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Services;

public class PointServiceEFC : IPointService
{
    private readonly AppDbContext _context;

    public PointServiceEFC(AppDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<PointE>> GetAllPoints()
    {
        var points = _context.PointsEF.ToList();
        return ApiResponse<List<PointE>>.SuccessResponse(points, "PointsRetrievedSuccessfully");
    }

    public ApiResponse<PointE> GetPointById(int id)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        return ApiResponse<PointE>.SuccessResponse(point, "PointRetrievedSuccessfully");
    }

    public ApiResponse<PointE> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        try
        {
            var point = new PointE
            {
                Name = pointDto.Name,
                Geometry = pointDto.Geometry
            };

            _context.PointsEF.Add(point);
            _context.SaveChanges();

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
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
            {
                return ApiResponse<List<PointE>>.ErrorResponse("ValidationError");
            }

            try
            {
                var point = new PointE
                {
                    Name = pointDto.Name,
                    Geometry = pointDto.Geometry
                };
                validPoints.Add(point);
            }
            catch (Exception)
            {
                return ApiResponse<List<PointE>>.ErrorResponse("InvalidWktFormat");
            }
        }

        _context.PointsEF.AddRange(validPoints);
        _context.SaveChanges();

        return ApiResponse<List<PointE>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<PointE> UpdatePoint(int id, string newName, Geometry newGeometry)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }

        if (string.IsNullOrEmpty(newName) && newGeometry == null)
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        try
        {
            if (!string.IsNullOrEmpty(newName))
                point.Name = newName;

            if (newGeometry != null)
                point.Geometry = newGeometry;

            _context.SaveChanges();

            return ApiResponse<PointE>.SuccessResponse(point, "PointUpdatedSuccessfully");
        }
        catch (Exception)
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }
    }

    public ApiResponse<PointE> DeletePoint(int id)
    {
        var point = _context.PointsEF.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        
        _context.PointsEF.Remove(point);
        _context.SaveChanges();

        return ApiResponse<PointE>.SuccessResponse(point, "PointDeletedSuccessfully");
    }
}
