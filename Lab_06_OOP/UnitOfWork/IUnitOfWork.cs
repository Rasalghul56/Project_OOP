using System;
using Confectionery.Repositories;

namespace Confectionery.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IReviewRepository Reviews { get; }
        void Save();
    }
}
