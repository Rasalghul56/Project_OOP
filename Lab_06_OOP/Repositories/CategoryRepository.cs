using Confectionery.Data;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }
    }
}
