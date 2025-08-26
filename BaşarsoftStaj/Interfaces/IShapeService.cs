using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Models;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Interfaces;

public interface IPointService
{
    ApiResponse<List<Shape>> GetAllPoints();
    ApiResponse<Shape> GetPointById(int id);
    ApiResponse<Shape> AddPoint(AddPointDto pointDto);
    ApiResponse<List<Shape>> AddRangePoints(List<AddPointDto> pointDtos);
    ApiResponse<Shape> UpdatePoint(int id, string newName, Geometry newGeometry);
    ApiResponse<Shape> DeletePoint(int id);
}
