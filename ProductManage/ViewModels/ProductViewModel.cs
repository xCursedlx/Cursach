using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using ProductManage.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly ProductRepository _repository;
        private Product _selectedProduct;
        private string _searchTerm;
        private string _statusMessage;
        private bool _isLoading;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // Команды
        public ICommand LoadProductsCommand { get; }
        public ICommand SearchProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductViewModel()
        {
            _repository = new ProductRepository();

            // Инициализация команд
            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            SearchProductsCommand = new RelayCommand(async _ => await SearchProductsAsync(), _ => !string.IsNullOrWhiteSpace(SearchTerm));
            AddProductCommand = new RelayCommand(_ => OpenAddWindow());
            EditProductCommand = new RelayCommand(_ => OpenEditWindow(), _ => SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);

            // Автоматическая загрузка при создании
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка товаров...";

                Products.Clear();
                var products = await _repository.GetAllProductsAsync();

                foreach (var product in products)
                {
                    Products.Add(product);
                }

                StatusMessage = $"Успешно загружено: {Products.Count} товаров";
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

        private async Task SearchProductsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Поиск: '{SearchTerm}'...";

                Products.Clear();
                var results = await _repository.SearchProductsAsync(SearchTerm);

                foreach (var product in results)
                {
                    Products.Add(product);
                }

                StatusMessage = $"Найдено: {Products.Count} совпадений";
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
            var window = new AddProductWindow();
            if (window.ShowDialog() == true && window.NewProduct != null)
            {
                Products.Insert(0, window.NewProduct);
                StatusMessage = "Товар успешно добавлен";
            }
        }

        private void OpenEditWindow()
        {
            var window = new EditProductWindow(SelectedProduct);
            if (window.ShowDialog() == true)
            {
                _ = LoadProductsAsync(); // Обновляем список после редактирования
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            var confirm = MessageBox.Show(
                $"Удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
