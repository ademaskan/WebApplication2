using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPointRepository Points { get; }
        Task<int> SaveAsync();
    }
}