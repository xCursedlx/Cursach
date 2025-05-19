using MySql.Data.MySqlClient;
using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для окна регистрации новых пользователей
    /// </summary>
    /// <remarks>
    /// Реализует:
    /// - Валидацию вводимых данных
    /// - Проверку уникальности логина
    /// - Сохранение неактивного пользователя
    /// - Выбор роли из доступных в системе
    /// </remarks>
    public class RegisterViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly UserRoleRepository _roleRepo = new UserRoleRepository();
        private string _statusMessage;

        public RegisterViewModel()
        {
            Roles = new ObservableCollection<UserRole>();
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync(), CanRegister);
            LoadRolesAsyncVoid();
        }

        private async void LoadRolesAsyncVoid()
        {
            var rolesFromDb = await LoadRolesAsync();
            Roles.Clear();
            foreach (var role in rolesFromDb)
                Roles.Add(role);
        }

        #region Свойства данных
        /// <summary>Логин пользователя (обязательное поле)</summary>
        [Required(ErrorMessage = "Введите логин")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина логина 3-50 символов")]
        public string Username { get; set; }

        /// <summary>Пароль (обязательное поле)</summary>
        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Минимум 6 символов")]
        public string Password { get; set; }

        /// <summary>Полное имя пользователя</summary>
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string FullName { get; set; }

        /// <summary>Выбранная роль пользователя</summary>
        [Required(ErrorMessage = "Выберите роль")]
        public UserRole SelectedRole { get; set; }

        /// <summary>Список доступных ролей</summary>
        public ObservableCollection<UserRole> Roles { get; }

        /// <summary>Статусное сообщение</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
        #endregion

        #region Команды
        /// <summary>Команда регистрации нового пользователя</summary>
        public ICommand RegisterCommand { get; }
        #endregion

        #region Основные методы
        /// <summary>
        /// Загружает список доступных ролей из системы
        /// </summary>
        public async Task<List<UserRole>> LoadRolesAsync()
        {
            var roles = new List<UserRole>();
            const string sql = "SELECT code, display_name, description FROM roles";

            using (var conn = DatabaseManager.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync() as MySqlDataReader)
                {
                    if (reader == null)
                        throw new InvalidOperationException("Не удалось получить MySqlDataReader");
                    while (await reader.ReadAsync())
                    {
                        roles.Add(MapFromReader(reader));
                    }
                }
            }
            return roles;
        }

        /// <summary>
        /// Регистрирует нового пользователя в системе
        /// </summary>
        private async Task RegisterAsync()
        {
            try
            {
                // Проверка уникальности логина
                if (await _userRepo.UserExistsAsync(Username))
                {
                    StatusMessage = "Этот логин уже занят";
                    return;
                }

                // Создание нового пользователя
                var newUser = new User
                {
                    Username = Username,
                    Password = Password, // Хранится открыто по ТЗ
                    FullName = FullName,
                    Role = SelectedRole.Code,
                    IsActive = false, // Требует активации администратором
                    CreatedAt = DateTime.Now
                };

                // Сохранение в базу данных
                await _userRepo.AddUserAsync(newUser);

                StatusMessage = "Регистрация успешна! Ожидайте активации аккаунта.";
                ClearForm();
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка регистрации";
                await Logger.LogAsync($"Ошибка регистрации: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Очищает поля формы
        /// </summary>
        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            FullName = string.Empty;
            SelectedRole = null;
        }
        #endregion

        #region Валидация команд
        /// <summary>
        /// Проверяет возможность регистрации
        /// </summary>
        private bool CanRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                StatusMessage = "Логин не должен быть пустым";
                return false;
            }
            else if (Username.Length < 3)
            {
                StatusMessage = "Логин должен быть больше 2 символов";
                return false;
            }
            else if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Пароль не должен быть пустым";
                return false;
            }
            else if (Password.Length < 6)
            {
                StatusMessage = "Пароль должен быть больше 5 символов";
                return false;
            }

            else if (SelectedRole == null)
            {
                StatusMessage = "Выберите роль";
                return false;
            }
            StatusMessage = null;
            return !string.IsNullOrWhiteSpace(Username) &&
                           Username.Length >= 3 &&
                           !string.IsNullOrWhiteSpace(Password) &&
                           Password.Length >= 6 &&
                           SelectedRole != null;
            }
        #endregion
        public event Action<bool> RequestClose;

        protected void OnRequestClose(bool result)
        {
            RequestClose?.Invoke(result);
        }
        public void CloseRequest(bool result)
        {
            OnRequestClose(result);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
        }
        private UserRole MapFromReader(MySqlDataReader reader)
        {
            return new UserRole(
                reader.GetString("code"),
                reader.GetString("display_name"),
                reader.GetString("description")
            );
        }
    }
}