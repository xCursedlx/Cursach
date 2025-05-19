using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository = new UserRepository();

        private string _username;
        private string _password;
        private string _statusMessage;
        private bool _isBusy;
        public User AuthenticatedUser { get; private set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        [Required(ErrorMessage = "Введите логин")]
        public string Username
        {
            get => _username;
            set
            {
                if (SetField(ref _username, value))
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        // Пароль не имеет биндинга, передаётся через PasswordBox_PasswordChanged
        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value))
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose; // true - успешный вход, false - отмена

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;
        }

        private async Task LoginAsync()
        {
            StatusMessage = string.Empty;
            IsBusy = true;

            try
            {
                // Валидация
                var validationContext = new ValidationContext(this);
                var validationResults = new System.Collections.Generic.List<ValidationResult>();
                if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
                {
                    StatusMessage = string.Join(Environment.NewLine, validationResults.ConvertAll(r => r.ErrorMessage));
                    IsBusy = false;
                    return;
                }

                // Проверка пользователя
                var user = await _userRepository.AuthenticateAsync(Username.Trim(), Password);
                if (user == null)
                {
                    StatusMessage = "Неверный логин или пароль";
                }
                else if (!user.IsActive)
                {
                    StatusMessage = "Аккаунт не активирован. Дождитесь активации";
                }
                else
                {
                    // Успешный вход
                    AuthenticatedUser = user;
                    RequestClose?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при входе: {ex.Message}";
                await Logger.LogAsync($"LoginViewModel LoginAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
