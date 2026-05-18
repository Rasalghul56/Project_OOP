using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.UnitOfWork;

namespace Confectionery.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public User Login(string email, string password)
        {
            var hash = PasswordHelper.Hash(password);
            var user = _uow.Users.GetByEmail(email);
            return (user != null && user.PasswordHash == hash) ? user : null;
        }

        public bool Register(string name, string email, string password, string phone = null)
        {
            if (_uow.Users.EmailExists(email))
                return false;

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = PasswordHelper.Hash(password),
                Phone = phone,
                Role = "Client"
            };

            _uow.Users.Add(user);
            _uow.Save();
            return true;
        }
    }
}
