using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Confectionery.Data;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context) { }

        public IEnumerable<Product> GetAvailable()
            => Context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.IsAvailable)
                .ToList();

        public IEnumerable<Product> GetByCategory(int categoryId)
            => Context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                .ToList();

        public Product GetWithDetails(int id)
            => Context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews.Select(r => r.User))
                .FirstOrDefault(p => p.Id == id);

        public IEnumerable<Product> GetAllWithCategory()
            => Context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ToList();

        public void AttachReviews(IEnumerable<Product> products)
        {
            var list = products?.ToList();
            if (list == null || list.Count == 0) return;

            var ids = list.Select(p => p.Id).ToList();
            var byProduct = Context.Reviews
                .Where(r => ids.Contains(r.ProductId))
                .ToList()
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var p in list)
            {
                if (p.Reviews == null)
                    p.Reviews = new List<Review>();
                else
                    p.Reviews.Clear();

                if (byProduct.TryGetValue(p.Id, out var revs))
                {
                    foreach (var r in revs)
                        p.Reviews.Add(r);
                }
            }
        }
    }
}
