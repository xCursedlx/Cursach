using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using ProductManage.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private User _currentUser;
     

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (_selectedMenuItem != value)
                {
                    _selectedMenuItem = value;
                    OnPropertyChanged();

                    // При выборе пункта меню вызываем открытие окна
                    if (_selectedMenuItem != null)
                    {
                        OpenWindow(_selectedMenuItem.WindowKey);
                        // Сбрасываем выбор, чтобы можно было выбрать тот же пункт повторно
                        SelectedMenuItem = null;
                    }
                }
            }
        }

        public ObservableCollection<MenuItem> MenuItems { get; } = new ObservableCollection<MenuItem>();

        public ICommand LogoutCommand { get; }

        public MenuViewModel(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));

            LogoutCommand = new RelayCommand(_ => Logout());

            LoadMenuItems();
        }

        private readonly UserRepository _userRepository = new UserRepository();

        private async Task RefreshCurrentUserAsync()
        {
            if (_currentUser == null) return;

            var updatedUser = await _userRepository.GetUserByIdAsync(_currentUser.Id);
            if (updatedUser != null)
            {
                _currentUser = updatedUser;
                LoadMenuItems(); // Обновляем меню, если роль могла измениться
            }
        }


        private void LoadMenuItems()
        {
            MenuItems.Clear();

            // Общие пункты для всех ролей
            AddMenuItem("Список товаров", "ListWindow");
            AddMenuItem("Профиль", "ProfileWindow");

            // Ролевые пункты
            switch (_currentUser.Role)
            {
                case "admin":
                    AddMenuItem("Управление пользователями", "EditUserWindow");
                    AddMenuItem("Финансовые отчёты", "FinancialReportWindow");
                    AddMenuItem("Логи системы", "LogViewerWindow");
                    AddMenuItem("Расчёт прибыли", "CalculationWindow");
                    AddMenuItem("Управление поставками", "WarehouseWindow");
                    break;

                case "accountant":
                    AddMenuItem("Финансовые операции", "FinancialWindow");
                    AddMenuItem("Расчёт прибыли", "CalculationWindow");
                    break;

                case "warehouse":
                    AddMenuItem("Управление поставками", "WarehouseWindow");
                    break;
            }

        }

        private void AddMenuItem(string title, string windowKey)
        {
            if (IsMenuItemVisible(windowKey))
            {
                MenuItems.Add(new MenuItem
                {
                    Title = title,
                    WindowKey = windowKey,
                    IsVisible = true
                });
            }
        }

        private bool IsMenuItemVisible(string windowKey)
        {
            // Всегда видимые пункты
            if (windowKey == "Logout") return true;

            return _currentUser.Role switch
            {
                "admin" => true,
                "accountant" => windowKey != "UserManagementWindow",
                "warehouse" => windowKey == "WarehouseWindow" || windowKey == "NewSupplyWindow",
                _ => false
            };
        }

        private async void OpenWindow(string windowKey)
        {
            if (string.IsNullOrEmpty(windowKey)) return;

            await RefreshCurrentUserAsync();

            Window windowToOpen = windowKey switch
            {
                "ListWindow" => new ListWindow(),
                "ProfileWindow" => new ProfileWindow(_currentUser),
                "EditUserWindow" => new SelectUserWindow(),
                "FinancialReportWindow" => new FinancialReportWindow(_currentUser.Id),
                "LogViewerWindow" => new LogViewerWindow(),
                "CalculationWindow" => new CalculationWindow(),
                "WarehouseWindow" => new WarehouseWindow(_currentUser),
                _ => null
            };

            if (windowToOpen != null)
            {
                windowToOpen.Owner = Application.Current.MainWindow;
                windowToOpen.Show();
            }
            else if (windowKey == "Logout")
            {
                Logout();
            }
        }

        private void Logout()
        {
            AuthService.Logout();

            // Закрываем все окна кроме LoginWindow
            foreach (Window w in Application.Current.Windows.OfType<Window>().ToList())
            {
                if (!(w is MainWindow))
                    w.Close();
            }

            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        // Вложенный класс модели пункта меню
        public class MenuItem
        {
            public string Title { get; set; }
            public string WindowKey { get; set; }
            public bool IsVisible { get; set; }
        }
    }
}
