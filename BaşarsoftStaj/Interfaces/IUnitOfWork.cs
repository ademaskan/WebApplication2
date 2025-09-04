using BaşarsoftStaj.Interfaces;

namespace BaşarsoftStaj.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IShapeRepository Shapes { get; }
        Task<int> SaveAsync();
    }
}