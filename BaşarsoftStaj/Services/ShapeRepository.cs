using Microsoft.EntityFrameworkCore;
using BaşarsoftStaj.Data;
using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Services
{
    public class PointRepository : Repository<Shape>, IPointRepository
    {
        public PointRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Shape>> GetPointsByNameAsync(string name)
        {
            return await _dbSet
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }
    }
}