using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Models;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Interfaces
{
    public interface IShapeRepository : IRepository<Shape>
    {
        Task<PagedResult<Shape>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Shape>> GetPointsByNameAsync(string name);
        Task DeleteAllAsync();
        Task DeleteRangeAsync(IEnumerable<int> ids);
        Task<bool> HasIntersectingLineStringsAsync(Geometry geometry, string[] types);
        Task<bool> HasIntersectingPointsAsync(Geometry geometry, string[] types);
    }
}