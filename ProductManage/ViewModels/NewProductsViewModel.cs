using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для окна новых продуктов
    /// </summary>
    public class NewProductsViewModel : BaseViewModel
    {
        private readonly ProductRepository _productRepository = new ProductRepository();

        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product _selectedProduct;
        private string _statusMessage;
        private bool _isBusy;

        public NewProductsViewModel()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadProductsAsync(), _ => !IsBusy);
            CloseCommand = new RelayCommand(_ => OnRequestClose(false));

            // Загрузка данных при инициализации
            _ = LoadProductsAsync();
        }

        /// <summary>
        /// Коллекция продуктов для отображения
        /// </summary>
        public ObservableCollection<Product> Products
        {
            get => _products;
            private set => SetField(ref _products, value);
        }

        /// <summary>
        /// Выбранный продукт в списке
        /// </summary>
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetField(ref _selectedProduct, value);
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
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetField(ref _isBusy, value))
                {
                    ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CloseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Команда обновления списка продуктов
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Команда закрытия окна
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Событие запроса закрытия окна
        /// </summary>
        public event Action<bool> RequestClose;

        /// <summary>
        /// Загрузка списка новых продуктов из репозитория
        /// </summary>
        private async Task LoadProductsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка новых продуктов...";

                var products = await _productRepository.GetNewProductsAsync();

                Products = new ObservableCollection<Product>(products);

                StatusMessage = $"Загружено продуктов: {Products.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await Logger.LogAsync($"NewProductsViewModel.LoadProductsAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Вызов события закрытия окна
        /// </summary>
        /// <param name="result">Результат закрытия (например, true - успешно, false - отмена)</param>
        protected void OnRequestClose(bool result)
        {
            RequestClose?.Invoke(result);
        }
    }
}
