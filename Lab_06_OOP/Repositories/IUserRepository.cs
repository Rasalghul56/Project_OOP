using Confectionery.Models;

namespace Confectionery.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByEmail(string email);
        bool EmailExists(string email);
    }
}
