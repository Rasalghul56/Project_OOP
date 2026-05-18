using Confectionery.Models;

namespace Confectionery.Services
{
    public interface IAuthService
    {
        User Login(string email, string password);
        bool Register(string name, string email, string password, string phone = null);
    }
}
