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
    /// ViewModel для управления поставками на складе
    /// </summary>
    public class WarehouseViewModel : BaseViewModel
    {
        private readonly SupplyRepository _supplyRepository = new SupplyRepository();
        private readonly SupplierRepository _supplierRepository = new SupplierRepository();
        private readonly User _currentUser;

        private ObservableCollection<Supply> _supplies;
        private ObservableCollection<Supplier> _suppliers;

        private Supply _selectedSupply;
        private string _searchText;
        private string _statusMessage;
        private bool _isLoading;
        public bool IsNotLoading => !IsLoading;


        public WarehouseViewModel(User user)
        {
            Supplies = new ObservableCollection<Supply>();
            Suppliers = new ObservableCollection<Supplier>();

            LoadSuppliesCommand = new RelayCommand(async _ => await LoadSuppliesAsync());
            AddSupplyCommand = new RelayCommand(_ => OpenNewSupplyWindow());
            RefreshCommand = new RelayCommand(async _ => await LoadSuppliesAsync());
            _currentUser = user;

            // Автозагрузка данных
            Task.Run(async () =>
            {
                await LoadSuppliersAsync();
                await LoadSuppliesAsync();
            });
        }

        #region Properties

        /// <summary>
        /// Список поставок
        /// </summary>
        public ObservableCollection<Supply> Supplies
        {
            get => _supplies;
            set => SetField(ref _supplies, value);
        }

        /// <summary>
        /// Список поставщиков (для фильтрации или выбора)
        /// </summary>
        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetField(ref _suppliers, value);
        }

        /// <summary>
        /// Выбранная поставка
        /// </summary>
        public Supply SelectedSupply
        {
            get => _selectedSupply;
            set => SetField(ref _selectedSupply, value);
        }

        /// <summary>
        /// Текст для поиска по номеру накладной или имени поставщика
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    _ = LoadSuppliesAsync();
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
                    ((RelayCommand)LoadSuppliesCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddSupplyCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadSuppliesCommand { get; }
        public ICommand AddSupplyCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Загружает список поставок с учетом фильтрации по SearchText
        /// </summary>
        private async Task LoadSuppliesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка поставок...";

                // Загружаем все поставки и поставщиков
                var allSupplies = await _supplyRepository.GetAllSuppliesAsync();
                var allSuppliers = await _supplierRepository.GetAllSuppliersAsync();

                // Создаем словарь для быстрого поиска по SupplierId
                var suppliersDict = allSuppliers.ToDictionary(s => s.Id, s => s.Name.ToLower());

                // Заполняем SupplierName у каждой поставки
                foreach (var supply in allSupplies)
                {
                    if (supply.SupplierId.HasValue && suppliersDict.ContainsKey(supply.SupplierId.Value))
                    {
                        supply.SupplierName = suppliersDict[supply.SupplierId.Value];
                    }
                    else
                    {
                        supply.SupplierName = "Неизвестный поставщик";
                    }
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lowerSearch = SearchText.ToLower();
                    allSupplies = allSupplies
                        .Where(s => (s.InvoiceNumber != null && s.InvoiceNumber.ToLower().Contains(lowerSearch)) ||
                                    (s.SupplierId.HasValue && suppliersDict.ContainsKey(s.SupplierId.Value) && suppliersDict[s.SupplierId.Value].Contains(lowerSearch)))
                        .ToList();
                }


                Application.Current.Dispatcher.Invoke(() =>
                {
                    Supplies.Clear();
                    foreach (var supply in allSupplies)
                        Supplies.Add(supply);
                });

                StatusMessage = $"Загружено поставок: {Supplies.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки поставок: {ex.Message}";
                await Logger.LogAsync($"WarehouseViewModel LoadSuppliesAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// Загружает список поставщиков
        /// </summary>
        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllSuppliersAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Suppliers.Clear();
                    foreach (var supplier in suppliers)
                        Suppliers.Add(supplier);
                });
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"WarehouseViewModel LoadSuppliersAsync error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Открывает окно создания новой поставки
        /// </summary>
        private void OpenNewSupplyWindow()
        {
            try
            {
                var newSupplyWindow = new NewSupplyWindow(_currentUser);
                var vm = new ViewModels.NewSupplyViewModel(_currentUser);
                newSupplyWindow.DataContext = vm;
                newSupplyWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);

                bool? result = newSupplyWindow.ShowDialog();
                if (result == true)
                {
                    // После успешного добавления обновляем список
                    _ = LoadSuppliesAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка открытия окна новой поставки: {ex.Message}";
                Logger.LogAsync($"WarehouseViewModel OpenNewSupplyWindow error: {ex}", LogLevel.Error).Wait();
            }
        }

        #endregion
    }
}
