using System.Collections.Generic;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        IEnumerable<Product> GetAvailable();
        IEnumerable<Product> GetByCategory(int categoryId);
        Product GetWithDetails(int id);
        IEnumerable<Product> GetAllWithCategory();
        void AttachReviews(System.Collections.Generic.IEnumerable<Product> products);
    }
}
