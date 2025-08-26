using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IShapeRepository Points { get; }
        Task<int> SaveAsync();
    }
}