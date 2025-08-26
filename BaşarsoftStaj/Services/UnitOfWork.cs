using BaşarsoftStaj.Data;
using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IShapeRepository? _points;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IShapeRepository Points => _points ??= new ShapeRepository(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}