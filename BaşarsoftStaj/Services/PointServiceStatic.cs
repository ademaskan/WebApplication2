using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using NetTopologySuite.Geometries;


namespace BaşarsoftStaj.Services;

public class PointServiceStatic : IPointService
{
    private static List<PointE> _points = new List<PointE>();
    private static int _idCounter = 1;

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
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        try
        {
            var point = new PointE
            {
                Id = _idCounter++,
                Name = pointDto.Name,
                Geometry = pointDto.Geometry
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
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
            {
                return ApiResponse<List<PointE>>.ErrorResponse("ValidationError");
            }

            try
            {
                var point = new PointE
                {
                    Id = _idCounter++,
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

        _points.AddRange(validPoints);
        return ApiResponse<List<PointE>>.SuccessResponse(validPoints, "PointsAddedSuccessfully");
    }

    public ApiResponse<PointE> UpdatePoint(int id, string newName, Geometry newGeometry)
    {
        var point = _points.FirstOrDefault(p => p.Id == id);
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
}
