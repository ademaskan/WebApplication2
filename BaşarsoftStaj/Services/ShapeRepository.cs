using Microsoft.EntityFrameworkCore;
using BaşarsoftStaj.Data;
using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Services
{
    public class ShapeRepository : Repository<Shape>, IShapeRepository
    {
        public ShapeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Shape>> GetPointsByNameAsync(string name)
        {
            return await _dbSet
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }

        public async Task DeleteAllAsync()
        {
            var allShapes = await _dbSet.ToListAsync();
            _dbSet.RemoveRange(allShapes);
        }
    }
}