using BaşarsoftStaj.Entity;

namespace BaşarsoftStaj.Interfaces
{
    public interface IShapeRepository : IRepository<Shape>
    {
        Task<IEnumerable<Shape>> GetPointsByNameAsync(string name);
        Task DeleteAllAsync();
    }
}