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
                .Where(p => p.IsAvailable)
                .ToList();

        public IEnumerable<Product> GetByCategory(int categoryId)
            => Context.Products
                .Include(p => p.Category)
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
                .ToList();
    }
}
