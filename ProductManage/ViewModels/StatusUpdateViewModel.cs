using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для управления статусами поставок
    /// </summary>
    /// <remarks>
    /// Обеспечивает:
    /// - Просмотр текущего статуса поставки
    /// - Выбор нового статуса из допустимых значений
    /// - Валидацию и сохранение изменений
    /// - Обратную связь о результате операции
    /// </remarks>
    public class StatusUpdateViewModel : BaseViewModel
    {
        private readonly SupplyRepository _supplyRepo = new SupplyRepository();
        private string _statusMessage;
        private Supply _selectedSupply;

        public StatusUpdateViewModel(int supplyId)
        {
            Statuses = new ObservableCollection<string> { "pending", "delivered", "cancelled" };
            LoadSupplyCommand = new RelayCommand(async _ => await LoadSupplyAsync(supplyId));
            UpdateStatusCommand = new RelayCommand(async _ => await UpdateStatusAsync(), CanUpdateStatus);
        }

        #region Свойства
        /// <summary>Текущая выбранная поставка</summary>
        public Supply SelectedSupply
        {
            get => _selectedSupply;
            set => SetField(ref _selectedSupply, value);
        }

        /// <summary>Доступные статусы поставок</summary>
        public ObservableCollection<string> Statuses { get; }

        /// <summary>Выбранный новый статус</summary>
        [Required(ErrorMessage = "Выберите новый статус")]
        public string SelectedStatus { get; set; }

        /// <summary>Сообщение о результате операции</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
        #endregion

        #region Команды
        /// <summary>Команда загрузки данных поставки</summary>
        public ICommand LoadSupplyCommand { get; }

        /// <summary>Команда обновления статуса</summary>
        public ICommand UpdateStatusCommand { get; }
        #endregion

        #region Основные методы
        /// <summary>
        /// Загружает данные поставки по ID
        /// </summary>
        private async Task LoadSupplyAsync(int supplyId)
        {
            try
            {
                SelectedSupply = await _supplyRepo.GetSupplyByIdAsync(supplyId);
                SelectedStatus = SelectedSupply?.Status;
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки данных";
                await Logger.LogAsync($"Ошибка загрузки поставки: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Обновляет статус поставки в системе
        /// </summary>
        private async Task UpdateStatusAsync()
        {
            try
            {
                if (SelectedSupply == null) return;

                // Проверка на изменение статуса
                if (SelectedSupply.Status == SelectedStatus)
                {
                    StatusMessage = "Статус не изменен";
                    return;
                }

                // Бизнес-правило: нельзя отменить доставленную поставку
                if (SelectedSupply.Status == "delivered" && SelectedStatus == "cancelled")
                {
                    StatusMessage = "Доставленную поставку нельзя отменить";
                    return;
                }

                // Сохранение изменений
                await _supplyRepo.UpdateSupplyStatusAsync(SelectedSupply.Id, SelectedStatus);

                // Обновление локальных данных
                SelectedSupply.Status = SelectedStatus;
                StatusMessage = "Статус успешно обновлен";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка обновления статуса";
                await Logger.LogAsync($"UpdateStatus error: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion

        #region Валидация
        /// <summary>
        /// Проверяет возможность обновления статуса
        /// </summary>
        private bool CanUpdateStatus(object parameter)
        {
            return SelectedSupply != null &&
                   !string.IsNullOrEmpty(SelectedStatus) &&
                   SelectedStatus != SelectedSupply.Status;
        }
        #endregion
    }
}