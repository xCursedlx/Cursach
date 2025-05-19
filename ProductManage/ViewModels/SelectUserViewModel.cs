using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class SelectUserViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetField(ref _selectedUser, value))
                    ((RelayCommand)EditUserCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand EditUserCommand { get; }
        public ICommand RefreshCommand { get; }

        public SelectUserViewModel()
        {
            EditUserCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
            RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());

            _ = LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepo.GetAllUsersAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Clear();
                    foreach (var u in users)
                        Users.Add(u);
                });
            }
            catch (System.Exception ex)
            {
                await Logger.LogAsync($"SelectUserViewModel LoadUsersAsync error: {ex}", LogLevel.Error);
                MessageBox.Show("Ошибка загрузки пользователей", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser()
        {
            if (SelectedUser == null) return;

            var editWindow = new ProductManage.Windows.EditUserWindow(SelectedUser.Id)
            {
                Owner = Application.Current.MainWindow
            };
            editWindow.ShowDialog();

            // Обновить список после редактирования
            _ = LoadUsersAsync();
        }
    }
}
