using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Models;

namespace BaşarsoftStaj.Interfaces;

public interface IPointService
{
    ApiResponse<List<Point>> GetAllPoints();
    ApiResponse<Point> GetPointById(int id);
    ApiResponse<Point> AddPoint(AddPointDto pointDto);
    ApiResponse<List<Point>> AddRangePoints(List<AddPointDto> pointDtos);
    ApiResponse<Point> UpdatePoint(int id, string newName, string newWkt);
    ApiResponse<Point> DeletePoint(int id);
}
