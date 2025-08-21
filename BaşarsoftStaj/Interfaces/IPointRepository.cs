using BaşarsoftStaj.Entity;

namespace BaşarsoftStaj.Interfaces
{
    public interface IPointRepository : IRepository<PointE>
    {
        Task<IEnumerable<PointE>> GetPointsByNameAsync(string name);
    }
}