using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class EditUserViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly UserRoleRepository _roleRepo = new UserRoleRepository();

        private User _originalUser;

        public EditUserViewModel(int userId)
        {
            Roles = new ObservableCollection<UserRole>();
            SaveCommand = new RelayCommand(async _ => await SaveUserAsync(), CanSave);
            CancelCommand = new RelayCommand(_ => CloseWindow());

            _ = LoadDataAsync(userId);
        }

        #region Properties

        private UserListItem _currentUser = new UserListItem();
        public UserListItem CurrentUser
        {
            get => _currentUser;
            set => SetField(ref _currentUser, value);
        }

        private ObservableCollection<UserRole> _roles;
        public ObservableCollection<UserRole> Roles
        {
            get => _roles;
            set => SetField(ref _roles, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value))
                {
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync(int userId)
        {
            try
            {
                _originalUser = await _userRepo.GetUserByIdAsync(userId);
                if (_originalUser == null)
                {
                    StatusMessage = "Пользователь не найден!";
                    return;
                }

                var roles = await _roleRepo.GetAllRolesAsync();
                Roles = new ObservableCollection<UserRole>(roles);

                var currentRole = roles.FirstOrDefault(r => r.Code == _originalUser.Role);

                CurrentUser = new UserListItem
                {
                    Id = _originalUser.Id,
                    Username = _originalUser.Username,
                    FullName = _originalUser.FullName,
                    IsActive = _originalUser.IsActive,  
                };

                SelectedRoleCode = currentRole?.Code ?? _originalUser.Role;

                Password = _originalUser.Password ?? string.Empty;

                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await Logger.LogAsync($"EditUserVM LoadDataAsync error: {ex}", LogLevel.Error);
            }
        }

        private string _selectedRoleCode;
        public string SelectedRoleCode
        {
            get => _selectedRoleCode;
            set
            {
                if (SetField(ref _selectedRoleCode, value))
                {
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private async Task SaveUserAsync()
        {
            try
            {
                if (!ValidateUser())
                {
                    StatusMessage = "Исправьте ошибки в форме!";
                    return;
                }

                if (await IsUsernameTaken())
                {
                    StatusMessage = "Этот логин уже занят!";
                    return;
                }

                _originalUser.Username = CurrentUser.Username;
                _originalUser.FullName = CurrentUser.FullName;
                _originalUser.Role = SelectedRoleCode;
                _originalUser.IsActive = CurrentUser.IsActive;

                if (!string.IsNullOrWhiteSpace(Password))
                {
                    _originalUser.Password = Password; // Рекомендуется хешировать пароль!
                }

                var result = await _userRepo.UpdateUserAsync(_originalUser);

                if (result > 0)
                {
                    StatusMessage = "Данные сохранены!";
                    CloseWindow();
                }
                else
                {
                    StatusMessage = "Ошибка при сохранении данных.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения: {ex.Message}";
                await Logger.LogAsync($"EditUserVM SaveUserAsync error: {ex}", LogLevel.Error);
            }
        }

        private async Task<bool> IsUsernameTaken()
        {
            var existingUser = await _userRepo.GetUserByUsernameAsync(CurrentUser.Username);
            return existingUser != null && existingUser.Id != _originalUser.Id;
        }

        private bool ValidateUser()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Username) || CurrentUser.Username.Length < 3)
            {
                StatusMessage = "Логин должен содержать минимум 3 символа.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedRoleCode))
            {
                StatusMessage = "Выберите роль пользователя.";
                return false;
            }

            return true;
        }

        private void CloseWindow()
        {
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentUser.Username)
                   && CurrentUser.Username.Length >= 3
                   && !string.IsNullOrWhiteSpace(SelectedRoleCode);
        }

        #endregion
    }
}
