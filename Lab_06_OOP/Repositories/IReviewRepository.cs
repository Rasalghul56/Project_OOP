using System.Collections.Generic;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        IEnumerable<Review> GetByProduct(int productId);
        IEnumerable<Review> GetByUser(int userId);
        IEnumerable<Review> GetAllWithDetails();
        bool UserHasReviewedProduct(int userId, int productId);
        bool UserHasOrderedProduct(int userId, int productId);
    }
}
