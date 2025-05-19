using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class AddFinancialOperationViewModel : BaseViewModel
    {
        private readonly FinancialRepository _financialRepo = new FinancialRepository();

        private DateTime _operationDate = DateTime.Today;
        private string _selectedType;
        private decimal _amount;
        private string _description;
        private string _statusMessage;

        public AddFinancialOperationViewModel()
        {
            OperationTypes = new ObservableCollection<string> { "income", "expense" };
            SelectedType = OperationTypes[0];

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public DateTime OperationDate
        {
            get => _operationDate;
            set => SetField(ref _operationDate, value);
        }

        public ObservableCollection<string> OperationTypes { get; }

        [Required(ErrorMessage = "Выберите тип операции")]
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetField(ref _selectedType, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (SetField(ref _amount, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose; // true - сохранено, false - отмена

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(SelectedType) && Amount > 0;
        }

        private async Task SaveAsync()
        {
            // Валидация через атрибуты
            var validationContext = new ValidationContext(this);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

            if (!isValid)
            {
                StatusMessage = string.Join(Environment.NewLine, validationResults.ConvertAll(r => r.ErrorMessage));
                return;
            }

            try
            {
                var operation = new FinancialOperation
                {
                    OperationDate = OperationDate,
                    Type = SelectedType,
                    Amount = Amount,
                    Description = Description
                };

                await _financialRepo.AddAsync(operation);

                StatusMessage = "Операция успешно добавлена";
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения: {ex.Message}";
                await Logger.LogAsync($"AddFinancialOperationViewModel SaveAsync error: {ex}", LogLevel.Error);
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }
    }
}
