using Microsoft.EntityFrameworkCore;
using BaşarsoftStaj.Data;
using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using NetTopologySuite.Geometries;
using BaşarsoftStaj.Models;

namespace BaşarsoftStaj.Services
{
    public class ShapeRepository : Repository<Shape>, IShapeRepository
    {
        private const double BufferDistance = 0.001; // ~11 meters
        public ShapeRepository(AppDbContext context) : base(context)
        {
        }

        public new async Task<PagedResult<Shape>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            var count = await query.CountAsync();

            if (pageSize > 0)
            {
                var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                return new PagedResult<Shape>(items, count, pageNumber, pageSize);
            }
            else
            {
                var items = await query.ToListAsync();
                return new PagedResult<Shape>(items, count, 1, count > 0 ? count : 1);
            }
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

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var shapesToDelete = await _dbSet.Where(s => ids.Contains(s.Id)).ToListAsync();
            if (shapesToDelete.Any())
            {
                _dbSet.RemoveRange(shapesToDelete);
            }
        }

        public async Task<bool> HasIntersectingLineStringsAsync(Geometry geometry, string[] types)
        {
            return await _dbSet
                .Where(s => s.Geometry.OgcGeometryType == OgcGeometryType.LineString && types.Contains(s.Type))
                .AnyAsync(s => s.Geometry.Intersects(geometry.Buffer(BufferDistance)));
        }

        public async Task<bool> HasIntersectingPointsAsync(Geometry geometry, string[] types)
        {
            return await _dbSet
                .Where(s => s.Geometry.OgcGeometryType == OgcGeometryType.Point && types.Contains(s.Type))
                .AnyAsync(s => s.Geometry.Buffer(BufferDistance).Intersects(geometry));
        }
    }
}