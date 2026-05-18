using Confectionery.Data;
using Confectionery.Repositories;

namespace Confectionery.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IUserRepository _users;
        private IProductRepository _products;
        private ICategoryRepository _categories;
        private IOrderRepository _orders;
        private IReviewRepository _reviews;

        public UnitOfWork()
        {
            _context = new AppDbContext();
        }

        public IUserRepository Users
            => _users ?? (_users = new UserRepository(_context));

        public IProductRepository Products
            => _products ?? (_products = new ProductRepository(_context));

        public ICategoryRepository Categories
            => _categories ?? (_categories = new CategoryRepository(_context));

        public IOrderRepository Orders
            => _orders ?? (_orders = new OrderRepository(_context));

        public IReviewRepository Reviews
            => _reviews ?? (_reviews = new ReviewRepository(_context));

        public void Save()
            => _context.SaveChanges();

        public void Dispose()
            => _context.Dispose();
    }
}
