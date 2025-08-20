using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Models;

namespace BaşarsoftStaj.Interfaces;

public interface IPointService
{
    ApiResponse<List<PointE>> GetAllPoints();
    ApiResponse<PointE> GetPointById(int id);
    ApiResponse<PointE> AddPoint(AddPointDto pointDto);
    ApiResponse<List<PointE>> AddRangePoints(List<AddPointDto> pointDtos);
    ApiResponse<PointE> UpdatePoint(int id, string newName, string newWkt);
    ApiResponse<PointE> DeletePoint(int id);
}
