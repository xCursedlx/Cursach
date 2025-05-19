using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// ViewModel для управления финансовыми операциями
    /// </summary>
    public class FinancialOperationViewModel : BaseViewModel
    {
        private readonly FinancialRepository _financialRepo = new FinancialRepository();

        private FinancialOperation _selectedOperation;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _searchKeyword;
        private string _statusMessage;
        private bool _isLoading;
        private List<FinancialOperation> _operations = new List<FinancialOperation>();

        public FinancialOperationViewModel()
        {
            // Инициализация периода по умолчанию: последний месяц
            StartDate = DateTime.Today.AddMonths(-1);
            EndDate = DateTime.Today;

            LoadOperationsCommand = new RelayCommand(async _ => await LoadOperationsAsync());
            AddOperationCommand = new RelayCommand(_ => AddNewOperation());
            SaveOperationCommand = new RelayCommand(async _ => await SaveOperationAsync(), _ => CanSaveOperation());
            DeleteOperationCommand = new RelayCommand(async _ => await DeleteOperationAsync(), _ => CanDeleteOperation());
            ExportCommand = new RelayCommand(_ => ExportData(), _ => CanExport());
        }

        #region Properties

        /// <summary>
        /// Список финансовых операций
        /// </summary>
        public List<FinancialOperation> Operations
        {
            get => _operations;
            private set => SetField(ref _operations, value);
        }

        /// <summary>
        /// Выбранная операция для редактирования
        /// </summary>
        public FinancialOperation SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetField(ref _selectedOperation, value))
                {
                    ((RelayCommand)SaveOperationCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteOperationCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Начальная дата фильтрации
        /// </summary>
        [Required(ErrorMessage = "Укажите начальную дату")]
        public DateTime StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        /// <summary>
        /// Конечная дата фильтрации
        /// </summary>
        [Required(ErrorMessage = "Укажите конечную дату")]
        public DateTime EndDate
        {
            get => _endDate;
            set => SetField(ref _endDate, value);
        }

        /// <summary>
        /// Фильтр по описанию операции
        /// </summary>
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetField(ref _searchKeyword, value);
        }

        /// <summary>
        /// Статусное сообщение
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        /// <summary>
        /// Флаг выполнения операции
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        /// <summary>
        /// Доступные типы операций
        /// </summary>
        public List<string> OperationTypes { get; } = new List<string> { "income", "expense" };

        #endregion

        #region Commands

        public ICommand LoadOperationsCommand { get; }
        public ICommand AddOperationCommand { get; }
        public ICommand SaveOperationCommand { get; }
        public ICommand DeleteOperationCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Загрузка операций с фильтрацией
        /// </summary>
        private async Task LoadOperationsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка операций...";

                if (!ValidateDates())
                {
                    StatusMessage = "Некорректный период!";
                    return;
                }

                var operations = await _financialRepo.GetOperationsByDateRangeAsync(StartDate, EndDate);

                var filtered = string.IsNullOrWhiteSpace(SearchKeyword)
                    ? operations
                    : operations.Where(op => op.Description?.IndexOf(SearchKeyword, StringComparison.OrdinalIgnoreCase) >= 0);

                Operations = filtered.OrderByDescending(op => op.OperationDate).ToList();

                StatusMessage = $"Загружено операций: {Operations.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                await Logger.LogAsync($"LoadOperations error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Создание новой операции
        /// </summary>
        private void AddNewOperation()
        {
            SelectedOperation = new FinancialOperation
            {
                OperationDate = DateTime.Today,
                Type = "income",
                RecordedById = AuthService.CurrentUser?.Id ?? 0
            };
        }

        /// <summary>
        /// Сохранение операции
        /// </summary>
        private async Task SaveOperationAsync()
        {
            try
            {
                if (!ValidateOperation())
                {
                    StatusMessage = "Исправьте ошибки в данных!";
                    return;
                }

                if (SelectedOperation == null)
                {
                    StatusMessage = "Нет выбранной операции для сохранения.";
                    return;
                }

                if (SelectedOperation.Id == 0)
                {
                    await _financialRepo.AddAsync(SelectedOperation);
                }
                else
                {
                    await _financialRepo.UpdateAsync(SelectedOperation);
                }

                StatusMessage = "Операция сохранена!";
                await LoadOperationsAsync();
                SelectedOperation = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения: {ex.Message}";
                await Logger.LogAsync($"SaveOperation error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Удаление операции
        /// </summary>
        private async Task DeleteOperationAsync()
        {
            try
            {
                if (SelectedOperation == null || SelectedOperation.Id == 0)
                {
                    StatusMessage = "Выберите операцию для удаления.";
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить операцию от {SelectedOperation.OperationDate:d}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                await _financialRepo.DeleteAsync(SelectedOperation.Id);

                StatusMessage = "Операция удалена!";
                await LoadOperationsAsync();
                SelectedOperation = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
                await Logger.LogAsync($"DeleteOperation error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Экспорт данных в CSV
        /// </summary>
        private async void ExportData()
        {
            try
            {
                if (Operations == null || Operations.Count == 0)
                {
                    StatusMessage = "Нет данных для экспорта.";
                    return;
                }

                var exportService = new ExportService();
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"FinancialOperations_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    bool success = await exportService.ExportToCsvAsync(
                        Operations.Select(op => new
                        {
                            Date = op.OperationDate.ToString("yyyy-MM-dd"),
                            Type = op.Type,
                            Amount = op.Amount,
                            Description = op.Description
                        }),
                        saveDialog.FileName);

                    StatusMessage = success
                        ? $"Экспортировано {Operations.Count} операций"
                        : "Ошибка экспорта!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                await Logger.LogAsync($"Export error: {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Валидация периода
        /// </summary>
        private bool ValidateDates()
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (StartDate > DateTime.Today || EndDate > DateTime.Today)
            {
                MessageBox.Show("Даты не могут быть в будущем!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Валидация операции
        /// </summary>
        private bool ValidateOperation()
        {
            if (SelectedOperation == null)
                return false;

            var context = new ValidationContext(SelectedOperation);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(SelectedOperation, context, results, true);

            if (!isValid)
            {
                StatusMessage = string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
            }

            return isValid;
        }

        /// <summary>
        /// Проверка возможности сохранения
        /// </summary>
        private bool CanSaveOperation()
        {
            return SelectedOperation != null &&
                   SelectedOperation.Amount > 0 &&
                   !string.IsNullOrWhiteSpace(SelectedOperation.Type) &&
                   SelectedOperation.OperationDate <= DateTime.Today;
        }

        /// <summary>
        /// Проверка возможности удаления
        /// </summary>
        private bool CanDeleteOperation()
        {
            return SelectedOperation != null && SelectedOperation.Id > 0;
        }

        /// <summary>
        /// Проверка возможности экспорта
        /// </summary>
        private bool CanExport()
        {
            return Operations != null && Operations.Any();
        }

        #endregion
    }
}
