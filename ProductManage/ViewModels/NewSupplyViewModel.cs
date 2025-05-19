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
    /// <summary>
    /// ViewModel для создания и редактирования поставок
    /// </summary>
    public class NewSupplyViewModel : BaseViewModel
    {
        public event Action<bool> RequestClose;

        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly SupplyRepository _supplyRepo = new SupplyRepository();
        private readonly SupplyItemRepository _supplyItemRepo = new SupplyItemRepository();
        private readonly User _currentuser;

        private Product _selectedProduct;
        private int _quantity = 1;
        private SupplyItem _selectedSupplyItem;
        private Supplier _selectedSupplier;
        private string _invoiceNumber;
        private DateTime _deliveryDate = DateTime.Today;
        private string _status = "pending";
        private string _statusMessage;

        public NewSupplyViewModel(User user)
        {
            SupplyItems = new ObservableCollection<SupplyItem>();
            Suppliers = new ObservableCollection<Supplier>();
            Statuses = new ObservableCollection<string> { "pending", "delivered", "cancelled" };

            LoadSuppliersCommand = new RelayCommand(async _ => await LoadSuppliersAsync());
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => CanAddProduct());
            RemoveProductCommand = new RelayCommand(_ => RemoveProduct(), _ => CanRemoveProduct());
            SaveSupplyCommand = new RelayCommand(async _ => await SaveSupplyAsync(), _ => CanSaveSupply());
            CancelCommand = new RelayCommand(_ => CloseRequest(false));
            _currentuser = user;

            // Загрузка поставщиков при инициализации
            _ = LoadSuppliersAsync();
        }

        #region Properties
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetField(ref _products, value);
        }


        [Required(ErrorMessage = "Выберите поставщика")]
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set => SetField(ref _selectedSupplier, value);
        }

        [Required(ErrorMessage = "Укажите номер накладной")]
        [StringLength(50, ErrorMessage = "Максимум 50 символов")]
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set => SetField(ref _invoiceNumber, value);
        }

        [Required(ErrorMessage = "Укажите дату поставки")]
        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => SetField(ref _deliveryDate, value);
        }

        [Required(ErrorMessage = "Выберите статус")]
        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public ObservableCollection<Supplier> Suppliers { get; }
        public ObservableCollection<string> Statuses { get; }
        public ObservableCollection<SupplyItem> SupplyItems { get; }

        [Range(1, int.MaxValue, ErrorMessage = "Минимум 1 шт.")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                {
                    ((RelayCommand)AddProductCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetField(ref _selectedProduct, value))
                {
                    ((RelayCommand)AddProductCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public SupplyItem SelectedSupplyItem
        {
            get => _selectedSupplyItem;
            set
            {
                if (SetField(ref _selectedSupplyItem, value))
                {
                    ((RelayCommand)RemoveProductCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public decimal TotalAmount => SupplyItems.Sum(item => item.TotalPrice);

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        public ICommand LoadSuppliersCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductCommand { get; }
        public ICommand SaveSupplyCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierRepo.GetAllSuppliersAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Suppliers.Clear();
                    foreach (var supplier in suppliers)
                        Suppliers.Add(supplier);
                });
                var products = await _productRepo.GetAllProductsAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var products in products)
                        Products.Add(products);
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки поставщиков";
                await Logger.LogAsync($"LoadSuppliersAsync error: {ex}", LogLevel.Error);
                MessageBox.Show(StatusMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddProduct() => SelectedProduct != null && Quantity > 0;

        private void AddProduct()
        {
            if (!CanAddProduct())
            {
                StatusMessage = "Выберите продукт и укажите количество больше 0";
                return;
            }

            var existingItem = SupplyItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += Quantity;
            }
            else
            {
                SupplyItems.Add(new SupplyItem
                {
                    ProductId = SelectedProduct.Id,
                    ProductName = SelectedProduct.Name,
                    Quantity = Quantity,
                    UnitPrice = SelectedProduct.Price
                });
            }

            OnPropertyChanged(nameof(TotalAmount));
            ((RelayCommand)SaveSupplyCommand).RaiseCanExecuteChanged();

            // Сброс выбора и количества
            Quantity = 1;
            SelectedProduct = null;
            StatusMessage = string.Empty;
        }

        private bool CanRemoveProduct() => SelectedSupplyItem != null;

        private void RemoveProduct()
        {
            if (!CanRemoveProduct())
                return;

            SupplyItems.Remove(SelectedSupplyItem);
            OnPropertyChanged(nameof(TotalAmount));
            ((RelayCommand)SaveSupplyCommand).RaiseCanExecuteChanged();
        }

        private bool CanSaveSupply()
        {
            var context = new ValidationContext(this);
            var results = new System.Collections.Generic.List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(this, context, results, true);

            if (!isValid)
            {
                StatusMessage = string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
                return false;
            }

            if (SupplyItems.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы один товар в поставку";
                return false;
            }

            return true;
        }

        private async Task SaveSupplyAsync()
        {
            if (!CanSaveSupply())
                return;

            try
            {
                StatusMessage = "Сохранение...";
                var newSupply = new Supply
                {
                    SupplierId = SelectedSupplier.Id,
                    InvoiceNumber = InvoiceNumber,
                    DeliveryDate = DeliveryDate,
                    Status = Status,
                    TotalAmount = TotalAmount,
                    CreatedBy = _currentuser.Id
                };

                using (var unitOfWork = new UnitOfWork())
                {
                    var supplyId = await unitOfWork.Supplies.AddSupplyAsync(newSupply);

                    foreach (var item in SupplyItems)
                    {
                        item.SupplyId = supplyId;
                        await unitOfWork.SupplyItems.AddSupplyItemAsync(item);
                        await unitOfWork.Products.UpdateProductQuantityAsync(item.ProductId, item.Quantity);
                    }

                    unitOfWork.Commit();
                }

                StatusMessage = "Поставка успешно сохранена";
                MessageBox.Show(StatusMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                ResetForm();
                CloseRequest(true);
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка сохранения поставки";
                await Logger.LogAsync($"SaveSupplyAsync error: {ex}", LogLevel.Error);
                MessageBox.Show(StatusMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            SupplyItems.Clear();
            SelectedSupplier = null;
            InvoiceNumber = string.Empty;
            DeliveryDate = DateTime.Today;
            Status = "pending";
            Quantity = 1;
            SelectedProduct = null;
            StatusMessage = string.Empty;
            OnPropertyChanged(nameof(TotalAmount));
            ((RelayCommand)SaveSupplyCommand).RaiseCanExecuteChanged();
        }

        #endregion

        #region Close Request

        protected void OnRequestClose(bool result)
        {
            RequestClose?.Invoke(result);
        }

        public void CloseRequest(bool result)
        {
            OnRequestClose(result);
        }

        #endregion
    }
}
