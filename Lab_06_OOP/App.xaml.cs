using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Windows;
using Confectionery.Data;
using Confectionery.Migrations;
using Confectionery.Services;
using Confectionery.ViewModels.Auth;
using Confectionery.Views.Auth;

namespace Confectionery
{
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {

            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());


            using (var ctx = new AppDbContext())
            {
                ctx.Database.Initialize(force: false);
            }


            var uow = new UnitOfWork.UnitOfWork();
            var authService = new AuthService(uow);
            var navigation = new NavigationService(authService);

            var loginVm = new LoginViewModel(authService, navigation);
            var loginWindow = new LoginWindow { DataContext = loginVm };
            MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}
