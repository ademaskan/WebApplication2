using BaşarsoftStaj.Entity;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Interfaces
{
    public interface IShapeRepository : IRepository<Shape>
    {
        Task<IEnumerable<Shape>> GetPointsByNameAsync(string name);
        Task DeleteAllAsync();
        Task DeleteRangeAsync(IEnumerable<int> ids);
        Task<bool> HasIntersectingLineStringsAsync(Geometry geometry, string[] types);
    }
}