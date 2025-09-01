using Microsoft.EntityFrameworkCore;
using BaşarsoftStaj.Data;
using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Services
{
    public class ShapeRepository : Repository<Shape>, IShapeRepository
    {
        private const double BufferDistance = 0.001; // ~11 meters
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