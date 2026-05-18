using System.Linq;
using Confectionery.Data;
using Confectionery.Models;

namespace Confectionery.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public User GetByEmail(string email)
            => Context.Users.FirstOrDefault(u => u.Email == email);

        public bool EmailExists(string email)
            => Context.Users.Any(u => u.Email == email);
    }
}
