using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class CalculationViewModel : BaseViewModel
    {
        private readonly FinancialRepository _financialRepo = new FinancialRepository();
        private decimal _totalIncome;
        private decimal _totalExpense;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _statusMessage;
        private bool _isLoading;

        public CalculationViewModel()
        {
            StartDate = DateTime.Today.AddMonths(-1);
            EndDate = DateTime.Today;

            CalculateCommand = new RelayCommand(async _ => await CalculateAsync(), _ => CanCalculate());
            ExportCommand = new RelayCommand(async _ => await ExportDataAsync(), _ => CanExport());
            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        #region Properties

        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                if (SetField(ref _totalIncome, value))
                    OnPropertyChanged(nameof(Profit));
            }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set
            {
                if (SetField(ref _totalExpense, value))
                    OnPropertyChanged(nameof(Profit));
            }
        }

        public decimal Profit => TotalIncome - TotalExpense;


        [Required(ErrorMessage = "Укажите начальную дату")]
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetField(ref _startDate, value))
                    OnPropertyChanged(nameof(Profit));
            }
        }

        [Required(ErrorMessage = "Укажите конечную дату")]
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetField(ref _endDate, value))
                    OnPropertyChanged(nameof(Profit));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetField(ref _isLoading, value))
                {
                    ((RelayCommand)CalculateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand CalculateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand CloseCommand { get; }

        #endregion

        #region Methods

        private bool ValidateDates()
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (StartDate > DateTime.Today || EndDate > DateTime.Today)
            {
                MessageBox.Show("Даты не могут быть в будущем", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CanCalculate()
        {
            return !IsLoading && StartDate <= EndDate;
        }

        private bool CanExport()
        {
            return !IsLoading && (TotalIncome != 0 || TotalExpense != 0);
        }

        private async Task CalculateAsync()
        {
            if (!ValidateDates())
            {
                StatusMessage = "Ошибка: некорректные даты";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Выполнение расчетов...";

                var report = await _financialRepo.GetFinancialReportAsync(StartDate, EndDate);

                TotalIncome = report.TotalIncome;
                TotalExpense = report.TotalExpense;

                StatusMessage = $"Расчет завершён. Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                await Logger.LogAsync($"CalculationViewModel.CalculateAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportDataAsync()
        {
            try
            {
                var exportService = new ExportService();
                var data = new[]
                {
                    new
                    {
                        PeriodStart = StartDate.ToString("dd.MM.yyyy"),
                        PeriodEnd = EndDate.ToString("dd.MM.yyyy"),
                        TotalIncome,
                        TotalExpense,
                        Profit
                    }
                };

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"ProfitReport_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    bool success = await exportService.ExportToCsvAsync(data, saveDialog.FileName);
                    StatusMessage = success ? "Данные успешно экспортированы" : "Ошибка при экспорте данных";
                    MessageBox.Show(StatusMessage, "Экспорт", MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                await Logger.LogAsync($"CalculationViewModel.ExportDataAsync error: {ex}", LogLevel.Error);
                MessageBox.Show(StatusMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        #endregion
    }
}
