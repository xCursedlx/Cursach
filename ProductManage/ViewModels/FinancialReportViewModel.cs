using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using ProductManage.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class FinancialReportViewModel : BaseViewModel
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private FinancialReport _report = new FinancialReport();
        private FinancialOperationType? _selectedOperationTypeFilter = null; // null - все типы
        private bool _isBusy;
        private string _statusMessage;
        private readonly int currentUserId;

        private FinancialOperation _selectedOperation;
        private bool _isOperationSelected;

        private int _selectedOperationTypeIndex = 0; // 0 - Все, 1 - Доход, 2 - Расход

        private readonly FinancialRepository _repository = new FinancialRepository();

        public FinancialReportViewModel(int cur)
        {
            FilteredOperations = new ObservableCollection<FinancialOperation>();

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
            ExportCommand = new AsyncRelayCommand(ExportAsync, () => FilteredOperations.Count > 0 && !IsBusy);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => !IsBusy);
            AddOperationCommand = new RelayCommand(_ => AddOperation(), _ => !IsBusy);
            EditOperationCommand = new RelayCommand(_ => EditOperation(), _ => IsOperationSelected && !IsBusy);
            DeleteOperationCommand = new RelayCommand(async _ => await DeleteOperationAsync(), _ => IsOperationSelected && !IsBusy);
            currentUserId = cur;
            var today = DateTime.Today;
            EndDate = today;
            StartDate = new DateTime(today.Year, today.Month, 1);

            IsBusy = false;

            _ = RefreshAsync();
        }

        #region Properties

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetField(ref _endDate, value))
                    ValidateDatesAndRefresh();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetField(ref _startDate, value))
                    ValidateDatesAndRefresh();
            }
        }

        public FinancialReport Report
        {
            get => _report;
            private set => SetField(ref _report, value);
        }

        public FinancialOperationType? SelectedOperationTypeFilter
        {
            get => _selectedOperationTypeFilter;
            private set
            {
                if (SetField(ref _selectedOperationTypeFilter, value))
                    ApplyFilters();
            }
        }

        public int SelectedOperationTypeIndex
        {
            get => _selectedOperationTypeIndex;
            set
            {
                if (SetField(ref _selectedOperationTypeIndex, value))
                {
                    SelectedOperationTypeFilter = value switch
                    {
                        1 => FinancialOperationType.Income,
                        2 => FinancialOperationType.Expense,
                        _ => null
                    };
                }
            }
        }

        private ObservableCollection<FinancialOperation> _filteredOperations;
        public ObservableCollection<FinancialOperation> FilteredOperations
        {
            get => _filteredOperations;
            private set => SetField(ref _filteredOperations, value);
        }

        public FinancialOperation SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetField(ref _selectedOperation, value))
                    IsOperationSelected = _selectedOperation != null;
            }
        }

        public bool IsOperationSelected
        {
            get => _isOperationSelected;
            private set => SetField(ref _isOperationSelected, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetField(ref _isBusy, value))
                {
                    RaiseCanExecuteChangedSafe(RefreshCommand);
                    RaiseCanExecuteChangedSafe(ExportCommand);
                    RaiseCanExecuteChangedSafe(ClearFiltersCommand);
                    RaiseCanExecuteChangedSafe(AddOperationCommand);
                    RaiseCanExecuteChangedSafe(EditOperationCommand);
                    RaiseCanExecuteChangedSafe(DeleteOperationCommand);
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetField(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand AddOperationCommand { get; }
        public ICommand EditOperationCommand { get; }
        public ICommand DeleteOperationCommand { get; }

        #endregion

        #region Methods

        private void RaiseCanExecuteChangedSafe(ICommand command)
        {
            if (command is AsyncRelayCommand asyncCmd)
                asyncCmd.RaiseCanExecuteChanged();
            else if (command is RelayCommand relayCmd)
                relayCmd.RaiseCanExecuteChanged();
        }

        private async Task RefreshAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка отчёта...";

                if (!ValidateDates())
                {
                    StatusMessage = "Некорректный период!";
                    FilteredOperations.Clear();
                    return;
                }

                var report = await _repository.GetFinancialReportAsync(StartDate, EndDate);
                Report = report;

                ApplyFilters();

                StatusMessage = $"Отчёт за период с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy} загружен.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки отчёта: {ex.Message}";
                await Logger.LogAsync($"FinancialReportViewModel.RefreshAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            if (Report?.Operations == null)
            {
                FilteredOperations.Clear();
                return;
            }

            IEnumerable<FinancialOperation> filtered = Report.Operations;

            if (SelectedOperationTypeFilter.HasValue)
            {
                filtered = filtered.Where(op => string.Equals(op.Type, SelectedOperationTypeFilter.Value.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = filtered.OrderByDescending(op => op.OperationDate).ToList();

            FilteredOperations = new ObservableCollection<FinancialOperation>(filteredList);

            UpdateStatus(filteredList);
        }

        private void UpdateStatus(List<FinancialOperation> filteredOperations)
        {
            var income = filteredOperations.Where(op => string.Equals(op.Type, "income", StringComparison.OrdinalIgnoreCase)).Sum(op => op.Amount);
            var expense = filteredOperations.Where(op => string.Equals(op.Type, "expense", StringComparison.OrdinalIgnoreCase)).Sum(op => op.Amount);

            StatusMessage = $"Показано {FilteredOperations.Count} операций. Итого доход: {income:C}, Итого расход: {expense:C}";
        }

        private async Task ExportAsync()
        {
            if (IsBusy || FilteredOperations.Count == 0)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Экспорт данных...";

                var exportService = new ExportService();

                var reportToExport = new FinancialReport
                {
                    StartDate = StartDate,
                    EndDate = EndDate
                };
                reportToExport.SetOperations(FilteredOperations);

                string filePath = await exportService.ExportFinancialReportToCsvAsync(reportToExport);

                StatusMessage = $"Отчёт успешно экспортирован в файл:\n{filePath}";
                MessageBox.Show(StatusMessage, "Экспорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                await Logger.LogAsync($"FinancialReportViewModel.ExportAsync error: {ex}", LogLevel.Error);
                MessageBox.Show(StatusMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearFilters()
        {
            SelectedOperationTypeFilter = null;
            SelectedOperationTypeIndex = 0;
            ApplyFilters();
        }

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
        private void ValidateDatesAndRefresh()
        {
            if (ValidateDates())
            {
                _ = RefreshAsync();
            }
            else
            {
                if (FilteredOperations != null)
                    FilteredOperations.Clear();
                StatusMessage = "Некорректный период!";
            }
        }


        private void AddOperation()
        { 
            var newOperation = new FinancialOperation
            {
                OperationDate = DateTime.Today,
                Type = "income",
                Amount = 0m,
                Description = string.Empty,
                RecordedById = currentUserId 
            };

            var editWindow = new EditFinancialOperationWindow(newOperation, currentUserId); 
            if (editWindow.ShowDialog() == true)
            {
                _ = AddOperationAsync(newOperation);
            }
        }


        private async Task AddOperationAsync(FinancialOperation operation)
        {
            try
            {
                IsBusy = true;
                await _repository.AddAsync(operation);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении операции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EditOperation()
        {
            if (SelectedOperation == null)
                return;
 
            var editWindow = new EditFinancialOperationWindow(SelectedOperation, currentUserId);
            if (editWindow.ShowDialog() == true)
            {
                _ = UpdateOperationAsync(SelectedOperation);
            }
        }


        private async Task UpdateOperationAsync(FinancialOperation operation)
        {
            try
            {
                IsBusy = true;
                await _repository.UpdateAsync(operation);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении операции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteOperationAsync()
        {
            if (SelectedOperation == null)
                return;

            var result = MessageBox.Show($"Удалить операцию \"{SelectedOperation.Description}\" от {SelectedOperation.OperationDate:dd.MM.yyyy}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsBusy = true;
                await _repository.DeleteAsync(SelectedOperation.Id);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении операции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}
