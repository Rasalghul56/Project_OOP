using System.Windows;
using Confectionery.ViewModels.Admin;
using Confectionery.ViewModels.Auth;
using Confectionery.ViewModels.Client;
using Confectionery.Views.Auth;
using Confectionery.Views.Client;
using Confectionery.Views.Admin;

namespace Confectionery.Services
{
    /// <summary>
    /// Управляет переключением между главными окнами приложения.
    /// Выступает фабрикой ViewModel — обеспечивает ручной DI.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IAuthService _authService;

        public NavigationService(IAuthService authService)
        {
            _authService = authService;
        }

        public void NavigateToLogin()
        {
            var vm = new LoginViewModel(_authService, this);
            OpenWindow(new LoginWindow { DataContext = vm });
        }

        public void NavigateToRegister()
        {
            var vm = new RegisterViewModel(_authService, this);
            OpenWindow(new RegisterWindow { DataContext = vm });
        }

        public void NavigateToClientHome()
        {
            var uow = new UnitOfWork.UnitOfWork();
            var vm = new MainClientViewModel(uow, this);
            OpenWindow(new MainClientWindow { DataContext = vm });
        }

        public void NavigateToAdminHome()
        {
            var uow = new UnitOfWork.UnitOfWork();
            var vm = new AdminViewModel(uow, this);
            OpenWindow(new AdminWindow { DataContext = vm });
        }

        private static void OpenWindow(Window newWindow)
        {
            var old = Application.Current.MainWindow;
            Application.Current.MainWindow = newWindow;
            newWindow.Show();
            old?.Close();
        }
    }
}
