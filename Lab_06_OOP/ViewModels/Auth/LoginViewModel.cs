using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Services;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Auth
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigation;

        private string _email;
        private string _password;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set { SetProperty(ref _email, value); ((RelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
        }

        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); ((RelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(IAuthService authService, INavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

            LoginCommand = new RelayCommand(ExecuteLogin, CanLogin);
            GoToRegisterCommand = new RelayCommand(ExecuteGoToRegister);
        }

        private bool CanLogin(object p)
            => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        private void ExecuteLogin(object p)
        {
            ErrorMessage = null;
            var user = _authService.Login(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Неверный email или пароль. Проверьте данные.";
                return;
            }

            SessionService.Login(user);

            if (user.Role == "Admin")
                _navigation.NavigateToAdminHome();
            else
                _navigation.NavigateToClientHome();
        }

        private void ExecuteGoToRegister(object p)
            => _navigation.NavigateToRegister();
    }
}
