using ProductManage.Data;
using ProductManage.Models;
using ProductManage.Helpers;
using ProductManage.Services;
using System.Windows;
using System.Windows.Input;
using ProductManage.Windows;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для управления авторизованным пользователем
    /// </summary>
    public class AuthUserViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private User _currentUser;

        // Данные для изменения пароля
        private string _oldPassword;
        private string _newPassword;

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            set => SetField(ref _currentUser, value);
        }

        /// <summary>
        /// Старый пароль для изменения
        /// </summary>
        public string OldPassword
        {
            get => _oldPassword;
            set => SetField(ref _oldPassword, value);
        }

        /// <summary>
        /// Новый пароль
        /// </summary>
        public string NewPassword
        {
            get => _newPassword;
            set => SetField(ref _newPassword, value);
        }

        // Команды
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }



        public AuthUserViewModel(User user)
        {
            CurrentUser = user;
            ChangePasswordCommand = new RelayCommand(async _ => await ChangePassword());
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        /// <summary>
        /// Смена пароля пользователя
        /// </summary>
        private async Task ChangePassword()
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            try
            {
                // Проверка старого пароля (без хеширования)
                if (CurrentUser.Password != OldPassword)
                {
                    MessageBox.Show("Неверный текущий пароль");
                    return;
                }

                // Обновление пароля
                CurrentUser.Password = NewPassword;
                await _userRepo.UpdateUserAsync(CurrentUser);

                MessageBox.Show("Пароль успешно изменен");
                ClearPasswordFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены пароля: {ex.Message}");
                await Logger.LogAsync($"Password change error: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        private void Logout()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.Hide();
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    break;
                }
            }
        }

        /// <summary>
        /// Очистка полей ввода паролей
        /// </summary>
        private void ClearPasswordFields()
        {
            OldPassword = string.Empty;
            NewPassword = string.Empty;
        }
    }
}