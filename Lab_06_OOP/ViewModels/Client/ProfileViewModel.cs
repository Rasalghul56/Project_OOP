using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Client
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        private string _name;
        private string _phone;
        private string _oldPassword;
        private string _newPassword;
        private string _confirmNewPassword;
        private string _successMessage;
        private string _errorMessage;

        public string Email => SessionService.CurrentUser?.Email ?? "";

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string OldPassword
        {
            get => _oldPassword;
            set => SetProperty(ref _oldPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public ProfileViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            var user = SessionService.CurrentUser;
            if (user != null)
            {
                Name  = user.Name;
                Phone = user.Phone;
            }

            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile,
                p => !string.IsNullOrWhiteSpace(Name));

            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword,
                p => !string.IsNullOrWhiteSpace(OldPassword)
                  && !string.IsNullOrWhiteSpace(NewPassword)
                  && !string.IsNullOrWhiteSpace(ConfirmNewPassword));
        }

        private void ExecuteSaveProfile(object p)
        {
            ErrorMessage = SuccessMessage = null;
            var session = SessionService.CurrentUser;
            if (session == null) return;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = GetString("Validation_NameRequired", "Укажите имя.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                ErrorMessage = GetString("Validation_PhoneRequired", "Укажите номер телефона.");
                return;
            }

            if (!PhoneValidationHelper.IsValid(Phone))
            {
                ErrorMessage = PhoneValidationHelper.GetErrorMessage();
                return;
            }

            // Загружаем свежую сущность из текущего UoW-контекста
            var user = _uow.Users.GetById(session.Id);
            if (user == null) return;

            user.Name  = Name.Trim();
            user.Phone = Phone.Trim();
            _uow.Users.Update(user);
            _uow.Save();

            // Синхронизируем SessionService
            session.Name  = user.Name;
            session.Phone = user.Phone;

            SuccessMessage = "Профиль обновлён.";
        }

        private void ExecuteChangePassword(object p)
        {
            ErrorMessage = SuccessMessage = null;
            var session = SessionService.CurrentUser;
            if (session == null) return;

            if (PasswordHelper.Hash(OldPassword) != session.PasswordHash)
            {
                ErrorMessage = "Текущий пароль введён неверно.";
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Новые пароли не совпадают.";
                return;
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Новый пароль должен быть не менее 6 символов.";
                return;
            }

            var user = _uow.Users.GetById(session.Id);
            if (user == null) return;

            user.PasswordHash = PasswordHelper.Hash(NewPassword);
            _uow.Users.Update(user);
            _uow.Save();

            session.PasswordHash = user.PasswordHash;
            OldPassword = NewPassword = ConfirmNewPassword = null;
            SuccessMessage = "Пароль успешно изменён.";
        }

        private static string GetString(string key, string fallback)
            => System.Windows.Application.Current?.TryFindResource(key) as string ?? fallback;
    }
}
