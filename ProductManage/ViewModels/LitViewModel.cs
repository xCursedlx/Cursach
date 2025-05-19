using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using ProductManage.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private readonly ProductRepository _repository;

        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product _selectedProduct;
        private string _searchTerm;
        private string _statusMessage;
        private bool _isLoading;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetField(ref _products, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetField(ref _selectedProduct, value))
                {
                    // Обновляем состояние команд, зависящих от выбора
                    EditProductCommand.RaiseCanExecuteChanged();
                    DeleteProductCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetField(ref _searchTerm, value))
                {
                    SearchProductsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        // Команды
        public AsyncRelayCommand LoadProductsCommand { get; }
        public AsyncRelayCommand SearchProductsCommand { get; }
        public RelayCommand AddProductCommand { get; }
        public RelayCommand EditProductCommand { get; }
        public AsyncRelayCommand DeleteProductCommand { get; }

        public ListViewModel()
        {
            _repository = new ProductRepository();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            SearchProductsCommand = new AsyncRelayCommand(SearchProductsAsync, CanSearch);
            AddProductCommand = new RelayCommand(OpenAddWindow); 
            EditProductCommand = new RelayCommand(_ => OpenEditWindow(), _ => SelectedProduct != null);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, () => SelectedProduct != null);

            // Загрузка данных при инициализации
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка товаров...";

                var products = await _repository.GetAllProductsAsync();
                Products = new ObservableCollection<Product>(products);
                StatusMessage = $"Загружено товаров: {Products.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await Logger.LogAsync($"Ошибка загрузки товаров: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(SearchTerm);
        }

        private async Task SearchProductsAsync()
        {
            if (!CanSearch())
            {
                await LoadProductsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Поиск по '{SearchTerm}'...";

                var results = await _repository.SearchProductsAsync(SearchTerm);
                Products = new ObservableCollection<Product>(results);

                StatusMessage = $"Найдено товаров: {Products.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка поиска: {ex.Message}";
                await Logger.LogAsync($"Ошибка поиска товаров: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenAddWindow()
        {
            var addWindow = new AddProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                // Добавляем новый товар в коллекцию
                Products.Insert(0, addWindow.NewProduct);
                StatusMessage = "Товар успешно добавлен";
            }
        }

        private void OpenEditWindow()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editWindow = new EditProductWindow(SelectedProduct);
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем список после редактирования
                _ = LoadProductsAsync();
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                await _repository.DeleteProductAsync(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                StatusMessage = "Товар успешно удалён";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
                await Logger.LogAsync($"Ошибка удаления товара: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
