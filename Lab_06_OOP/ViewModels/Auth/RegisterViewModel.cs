using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Services;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Auth
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigation;

        private string _name;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _phone;
        private string _errorMessage;
        private string _successMessage;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(IAuthService authService, INavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

            RegisterCommand = new RelayCommand(ExecuteRegister, CanRegister);
            GoToLoginCommand = new RelayCommand(p => _navigation.NavigateToLogin());
        }

        private bool CanRegister(object p)
            => !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password)
            && !string.IsNullOrWhiteSpace(ConfirmPassword);

        private void ExecuteRegister(object p)
        {
            ErrorMessage = null;
            SuccessMessage = null;

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль должен содержать минимум 6 символов.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(Phone) && !PhoneValidationHelper.IsValid(Phone))
            {
                ErrorMessage = PhoneValidationHelper.GetErrorMessage();
                return;
            }

            var ok = _authService.Register(Name, Email, Password, Phone?.Trim());
            if (!ok)
            {
                ErrorMessage = "Пользователь с таким email уже существует.";
                return;
            }

            // Авто-вход после успешной регистрации
            var user = _authService.Login(Email, Password);
            if (user != null)
            {
                SessionService.Login(user);
                _navigation.NavigateToClientHome();
            }
            else
            {
                SuccessMessage = "Регистрация прошла успешно! Войдите в систему.";
                _navigation.NavigateToLogin();
            }
        }
    }
}
