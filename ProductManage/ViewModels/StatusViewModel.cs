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
    /// ViewModel для управления статусами поставок
    /// </summary>
    public class StatusViewModel : BaseViewModel
    {
        private readonly SupplyRepository _supplyRepo;
        private string _selectedFilterStatus;
        private string _statusMessage;
        private Supply _selectedSupply;
        private string _newStatus;

        public StatusViewModel()
        {
            _supplyRepo = new SupplyRepository();

            Supplies = new ObservableCollection<Supply>();
            StatusFilters = new ObservableCollection<string>
            {
                "Все", "pending", "delivered", "cancelled"
            };

            InitializeCommands();
            LoadSuppliesAsync();
        }

        #region Properties
        /// <summary>Список поставок</summary>
        public ObservableCollection<Supply> Supplies { get; }

        /// <summary>Доступные статусы для фильтрации</summary>
        public ObservableCollection<string> StatusFilters { get; }

        /// <summary>Выбранный статус для фильтра</summary>
        public string SelectedFilterStatus
        {
            get => _selectedFilterStatus;
            set
            {
                SetField(ref _selectedFilterStatus, value);
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
                NewStatus = value?.Status;
                OnPropertyChanged(nameof(CanChangeStatus));
            }
        }

        /// <summary>Новый статус для выбранной поставки</summary>
        public string NewStatus
        {
            get => _newStatus;
            set => SetField(ref _newStatus, value);
        }

        /// <summary>Статусное сообщение</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        /// <summary>Доступные статусы для изменения</summary>
        public ObservableCollection<string> AvailableStatuses { get; } = new ObservableCollection<string>
        {
            "pending", "delivered", "cancelled"
        };

        /// <summary>Возможность изменения статуса</summary>
        public bool CanChangeStatus =>
            SelectedSupply != null &&
            SelectedSupply.Status != NewStatus;
        #endregion

        #region Commands
        /// <summary>Команда обновления статуса</summary>
        public ICommand UpdateStatusCommand { get; private set; }

        /// <summary>Команда обновления списка</summary>
        public ICommand RefreshCommand { get; private set; }
        #endregion

        #region Methods
        private void InitializeCommands()
        {
            UpdateStatusCommand = new RelayCommand(async _ =>
                await UpdateSupplyStatusAsync(),
                _ => CanChangeStatus);

            RefreshCommand = new RelayCommand(async _ =>
                await LoadSuppliesAsync());
        }

        /// <summary>Загрузка поставок с фильтрацией</summary>
        private async Task LoadSuppliesAsync()
        {
            try
            {
                var supplies = SelectedFilterStatus == "Все" || string.IsNullOrEmpty(SelectedFilterStatus)
                    ? await _supplyRepo.GetAllSuppliesAsync()
                    : await _supplyRepo.GetSuppliesByStatusAsync(SelectedFilterStatus);

                Supplies.Clear();
                foreach (var supply in supplies.OrderByDescending(s => s.DeliveryDate))
                {
                    Supplies.Add(supply);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await Logger.LogAsync($"StatusVM Load Error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Обновление статуса поставки</summary>
        private async Task UpdateSupplyStatusAsync()
        {
            try
            {
                // Бизнес-правило: нельзя отменить доставленную поставку
                if (SelectedSupply.Status == "delivered" && NewStatus == "cancelled")
                {
                    StatusMessage = "Доставленные поставки нельзя отменять";
                    return;
                }

                await _supplyRepo.UpdateSupplyStatusAsync(
                    SelectedSupply.Id,
                    NewStatus);

                // Обновление локальных данных
                SelectedSupply.Status = NewStatus;
                StatusMessage = "Статус успешно обновлен!";
                OnPropertyChanged(nameof(CanChangeStatus));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления: {ex.Message}";
                await Logger.LogAsync($"StatusVM Update Error: {ex}", LogLevel.Error);
            }
        }
        #endregion
    }
}