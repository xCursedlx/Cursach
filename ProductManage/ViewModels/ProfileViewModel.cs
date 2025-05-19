using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly User _user;

        public string Username => _user.Username;

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }


        public string RoleDisplay => _user.RoleDisplay;

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string StatusDisplay => _user.StatusDisplay;

        public string CreatedAtDisplay => _user.CreatedAtDisplay;

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public ProfileViewModel(User user)
        {
            _user = user;

            _fullName = user.FullName;

            _isActive = user.IsActive;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
        }

        private async Task SaveAsync()
        {
            StatusMessage = string.Empty;

            // Валидация FullName
            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "Полное имя не может быть пустым.";
                return;
            }


            // Обновляем модель
            _user.FullName = FullName;


            try
            {
                // Сохраняем пользователя через сервис (замените на вашу реализацию)
                await UserService.UpdateUserAsync(_user);
                StatusMessage = "Профиль успешно сохранён.";
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Ошибка при сохранении: {ex.Message}";
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
