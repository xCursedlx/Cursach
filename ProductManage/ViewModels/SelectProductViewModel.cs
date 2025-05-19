using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для выбора товаров из списка
    /// </summary>
    /// <remarks>
    /// Обеспечивает:
    /// - Просмотр полного каталога товаров
    /// - Поиск по названию и описанию
    /// - Выбор одного или нескольких товаров
    /// - Возврат результатов выбора
    /// </remarks>
    public class SelectProductViewModel : BaseViewModel
    {
        private readonly ProductRepository _productRepo = new ProductRepository();
        private string _searchTerm;
        private Product _selectedProduct;
        private bool _isMultiSelect;

        public event Action<Product> ProductSelected;
        public event Action SelectionCompleted;

        public SelectProductViewModel(bool multiSelect = false)
        {
            Products = new ObservableCollection<Product>();
            FilteredProducts = new ObservableCollection<Product>();

            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            SearchCommand = new RelayCommand(async _ => await FilterProductsAsync());
            SelectProductCommand = new RelayCommand(_ => OnProductSelected());
            CompleteSelectionCommand = new RelayCommand(_ => OnSelectionCompleted());

            _isMultiSelect = multiSelect;
            LoadProductsAsync().ConfigureAwait(false);
        }

        #region Свойства данных
        /// <summary>Полный список товаров</summary>
        public ObservableCollection<Product> Products { get; }

        /// <summary>Отфильтрованный список товаров</summary>
        public ObservableCollection<Product> FilteredProducts { get; }

        /// <summary>Текущий поисковый запрос</summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetField(ref _searchTerm, value);
        }

        /// <summary>Выбранный товар (в режиме единичного выбора)</summary>
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetField(ref _selectedProduct, value);
        }

        /// <summary>Флаг режима множественного выбора</summary>
        public bool IsMultiSelect => _isMultiSelect;
        #endregion

        #region Команды
        /// <summary>Команда загрузки товаров</summary>
        public ICommand LoadProductsCommand { get; }

        /// <summary>Команда поиска товаров</summary>
        public ICommand SearchCommand { get; }

        /// <summary>Команда выбора товара</summary>
        public ICommand SelectProductCommand { get; }

        /// <summary>Команда завершения выбора</summary>
        public ICommand CompleteSelectionCommand { get; }
        #endregion

        #region Основные методы
        /// <summary>
        /// Загружает товары из базы данных
        /// </summary>
        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _productRepo.GetAllProductsAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                        FilteredProducts.Add(product);
                    }
                });
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка загрузки товаров: {ex.Message}", LogLevel.Error);
                MessageBox.Show("Ошибка загрузки данных");
            }
        }

        /// <summary>
        /// Фильтрует товары по поисковому запросу
        /// </summary>
        private async Task FilterProductsAsync()
        {
            try
            {
                var filtered = await _productRepo.SearchProductsAsync(SearchTerm);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FilteredProducts.Clear();
                    foreach (var product in filtered)
                    {
                        FilteredProducts.Add(product);
                    }
                });
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка поиска: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Обработчик выбора товара
        /// </summary>
        private void OnProductSelected()
        {
            if (SelectedProduct != null && !_isMultiSelect)
            {
                ProductSelected?.Invoke(SelectedProduct);
                SelectionCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Обработчик завершения выбора
        /// </summary>
        private void OnSelectionCompleted()
        {
            SelectionCompleted?.Invoke();
        }
        #endregion

        #region Валидация
        /// <summary>
        /// Возвращает список выбранных товаров
        /// </summary>
        public ObservableCollection<Product> GetSelectedProducts()
        {
            return new ObservableCollection<Product>(
                FilteredProducts.Where(p => p.IsSelected)
            );
        }
        #endregion
    }
}