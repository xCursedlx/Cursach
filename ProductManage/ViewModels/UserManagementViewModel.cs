using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using ProductManage.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для управления пользователями (админский функционал)
    /// </summary>
    public class UserManagmentViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly UserRoleRepository _roleRepository = new UserRoleRepository();

        private ObservableCollection<User> _users;
        private ObservableCollection<UserRole> _roles;

        private User _selectedUser;
        private string _searchText;
        private string _statusMessage;
        private bool _isLoading;

        public UserManagmentViewModel()
        {
            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<UserRole>();

            LoadDataCommand = new RelayCommand(async _ => await LoadUsersAsync());
            AddUserCommand = new RelayCommand(_ => OpenUserEditor(null));
            EditUserCommand = new RelayCommand(_ => OpenUserEditor(SelectedUser), _ => SelectedUser != null);
            DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => SelectedUser != null);
            ToggleActivationCommand = new RelayCommand(async _ => await ToggleActivationAsync(), _ => SelectedUser != null);

            // Загружаем данные при создании VM
            Task.Run(async () =>
            {
                await LoadRolesAsync();
                await LoadUsersAsync();
            });
        }

        #region Properties

        /// <summary>
        /// Список пользователей
        /// </summary>
        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetField(ref _users, value);
        }

        /// <summary>
        /// Список ролей для выбора (если нужно)
        /// </summary>
        public ObservableCollection<UserRole> Roles
        {
            get => _roles;
            set => SetField(ref _roles, value);
        }

        /// <summary>
        /// Выбранный пользователь
        /// </summary>
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetField(ref _selectedUser, value))
                {
                    // Обновляем команды, зависящие от выбора
                    ((RelayCommand)EditUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ToggleActivationCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Текст для поиска пользователей (по логину или ФИО)
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    // Автоматическая фильтрация при вводе
                    _ = LoadUsersAsync();
                }
            }
        }

        /// <summary>
        /// Статусное сообщение для отображения пользователю
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        /// <summary>
        /// Флаг загрузки данных
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetField(ref _isLoading, value))
                {
                    ((RelayCommand)LoadDataCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)EditUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ToggleActivationCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ToggleActivationCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Загружает список пользователей из базы с учетом фильтра по SearchText
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка пользователей...";

                var allUsers = await _userRepository.GetAllUsersAsync();

                // Фильтрация по логину или ФИО, если есть текст поиска
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lowerSearch = SearchText.ToLower();
                    allUsers = allUsers
                        .Where(u => (u.Username != null && u.Username.ToLower().Contains(lowerSearch)) ||
                                    (u.FullName != null && u.FullName.ToLower().Contains(lowerSearch)))
                        .ToList();
                }

                // Обновляем коллекцию в UI-потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Clear();
                    foreach (var user in allUsers)
                        Users.Add(user);
                });

                StatusMessage = $"Загружено пользователей: {Users.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки пользователей: {ex.Message}";
                await Logger.LogAsync($"UserManagmentViewModel LoadUsersAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загружает роли из базы (если нужно для UI)
        /// </summary>
        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllRolesAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Roles.Clear();
                    foreach (var role in roles)
                        Roles.Add(role);
                });
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"UserManagmentViewModel LoadRolesAsync error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Открывает окно редактирования пользователя.
        /// Если user == null, открывает для создания нового пользователя.
        /// </summary>
        /// <param name="user">Пользователь для редактирования или null для создания</param>
        private void OpenUserEditor(User user)
        {
            try
            {
                var editWindow = new EditUserWindow(user?.Id ?? 0);


                if (user == null)
                {
                    // Новый пользователь
                    editWindow.DataContext = new EditUserViewModel(0); // 0 или другой признак нового
                }
                else
                {
                    // Редактирование существующего
                    editWindow.DataContext = new EditUserViewModel(user.Id);
                }

                editWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    // После сохранения обновляем список
                    _ = LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка открытия окна редактирования: {ex.Message}";
                Logger.LogAsync($"UserManagmentViewModel OpenUserEditor error: {ex}", LogLevel.Error).Wait();
            }
        }

        /// <summary>
        /// Удаляет выбранного пользователя после подтверждения
        /// </summary>
        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
                return;

            if (SelectedUser.Username == AuthService.CurrentUser?.Username)
            {
                MessageBox.Show("Нельзя удалить текущего пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Вы действительно хотите удалить пользователя \"{SelectedUser.Username}\"?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                var result = await _userRepository.DeleteUserAsync(SelectedUser.Id);

                if (result > 0)
                {
                    StatusMessage = $"Пользователь \"{SelectedUser.Username}\" удален";
                    Users.Remove(SelectedUser);
                    SelectedUser = null;
                }
                else
                {
                    StatusMessage = "Ошибка удаления пользователя";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
                await Logger.LogAsync($"UserManagmentViewModel DeleteUserAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Переключает активность пользователя (активен/неактивен)
        /// </summary>
        private async Task ToggleActivationAsync()
        {
            if (SelectedUser == null)
                return;

            if (SelectedUser.Username == AuthService.CurrentUser?.Username)
            {
                MessageBox.Show("Нельзя деактивировать текущего пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                SelectedUser.IsActive = !SelectedUser.IsActive;
                var result = await _userRepository.UpdateUserAsync(SelectedUser);

                if (result > 0)
                {
                    StatusMessage = SelectedUser.IsActive
                        ? $"Пользователь \"{SelectedUser.Username}\" активирован"
                        : $"Пользователь \"{SelectedUser.Username}\" деактивирован";
                }
                else
                {
                    StatusMessage = "Ошибка обновления статуса пользователя";
                    // Откатываем изменение
                    SelectedUser.IsActive = !SelectedUser.IsActive;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления: {ex.Message}";
                await Logger.LogAsync($"UserManagmentViewModel ToggleActivationAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
