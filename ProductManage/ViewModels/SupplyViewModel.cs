using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для управления списком поставок
    /// </summary>
    public class SupplyViewModel : BaseViewModel
    {
        private readonly SupplyRepository _supplyRepo;
        private readonly SupplyItemRepository _supplyItemRepo;
        private string _selectedStatusFilter;
        private Supply _selectedSupply;
        private string _statusMessage;

        public SupplyViewModel()
        {
            _supplyRepo = new SupplyRepository();
            _supplyItemRepo = new SupplyItemRepository();

            Supplies = new ObservableCollection<Supply>();
            StatusFilters = new ObservableCollection<string>
            {
                "Все", "pending", "delivered", "cancelled"
            };
            SupplyItems = new ObservableCollection<SupplyItem>();

            InitializeCommands();
            LoadSuppliesAsync();
        }

        #region Properties
        /// <summary>Список всех поставок</summary>
        public ObservableCollection<Supply> Supplies { get; }

        /// <summary>Элементы выбранной поставки</summary>
        public ObservableCollection<SupplyItem> SupplyItems { get; }

        /// <summary>Доступные фильтры статусов</summary>
        public ObservableCollection<string> StatusFilters { get; }

        /// <summary>Выбранный фильтр статуса</summary>
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                SetField(ref _selectedStatusFilter, value);
                LoadSuppliesAsync();
            }
        }

        /// <summary>Выбранная поставка</summary>
        public Supply SelectedSupply
        {
            get => _selectedSupply;
            set
            {
                SetField(ref _selectedSupply, value);
                LoadSupplyDetailsAsync();
                OnPropertyChanged(nameof(CanDeleteSupply));
            }
        }

        /// <summary>Статусное сообщение</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        /// <summary>Возможность удаления поставки</summary>
        public bool CanDeleteSupply =>
            SelectedSupply != null &&
            SelectedSupply.Status == "pending";
        #endregion

        #region Commands
        /// <summary>Команда обновления списка</summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>Команда удаления поставки</summary>
        public ICommand DeleteSupplyCommand { get; private set; }
        #endregion

        #region Methods
        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadSuppliesAsync());
            DeleteSupplyCommand = new RelayCommand(async _ => await DeleteSupplyAsync(), _ => CanDeleteSupply);
        }

        /// <summary>Загрузка списка поставок</summary>
        private async Task LoadSuppliesAsync()
        {
            try
            {
                var supplies = string.IsNullOrEmpty(SelectedStatusFilter) || SelectedStatusFilter == "Все"
                    ? await _supplyRepo.GetAllSuppliesAsync()
                    : await _supplyRepo.GetSuppliesByStatusAsync(SelectedStatusFilter);

                Supplies.Clear();
                foreach (var supply in supplies.OrderByDescending(s => s.DeliveryDate))
                {
                    Supplies.Add(supply);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await Logger.LogAsync($"SupplyVM Load Error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Загрузка деталей поставки</summary>
        private async Task LoadSupplyDetailsAsync()
        {
            try
            {
                if (SelectedSupply == null) return;

                var items = await _supplyItemRepo.GetItemsBySupplyAsync(SelectedSupply.Id);
                SupplyItems.Clear();
                foreach (var item in items)
                {
                    SupplyItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки деталей: {ex.Message}";
                await Logger.LogAsync($"SupplyVM Details Error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Удаление поставки</summary>
        private async Task DeleteSupplyAsync()
        {
            try
            {
                if (MessageBox.Show("Удалить выбранную поставку?", "Подтверждение",
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

                await _supplyRepo.DeleteSupplyAsync(SelectedSupply.Id);
                Supplies.Remove(SelectedSupply);
                StatusMessage = "Поставка успешно удалена";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
                await Logger.LogAsync($"SupplyVM Delete Error: {ex}", LogLevel.Error);
            }
        }
        #endregion
    }
}