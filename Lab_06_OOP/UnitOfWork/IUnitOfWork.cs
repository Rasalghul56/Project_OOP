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

        /// <summary>Clears HasStatusNotification for a single order using a direct SQL UPDATE (no EF cache).</summary>
        void ClearSingleOrderNotification(int orderId);
    }
}
