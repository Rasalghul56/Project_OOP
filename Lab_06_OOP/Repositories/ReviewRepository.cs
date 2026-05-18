using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Confectionery.Data;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public IEnumerable<Review> GetByProduct(int productId)
            => Context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

        public IEnumerable<Review> GetByUser(int userId)
            => Context.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

        public IEnumerable<Review> GetAllWithDetails()
            => Context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

        public bool UserHasReviewedProduct(int userId, int productId)
            => Context.Reviews.Any(r => r.UserId == userId && r.ProductId == productId);

        public bool UserHasOrderedProduct(int userId, int productId)
            => Context.OrderItems.Any(oi =>
                oi.ProductId == productId &&
                oi.Order.UserId == userId &&
                oi.Order.Status == OrderStatus.Completed);
    }
}
