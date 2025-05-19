using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class EditFinancialOperationViewModel : BaseViewModel
    {
        private DateTime _operationDate;
        private string _type;
        private decimal _amount;
        private string _description;
        private bool _isBusy;
        private string _statusMessage;

        public FinancialOperation Operation { get; }
        private int _recordedById;

        [Required(ErrorMessage = "Идентификатор пользователя обязателен")]
        public int RecordedById
        {
            get => _recordedById;
            set => SetField(ref _recordedById, value);
        }

        public EditFinancialOperationViewModel(FinancialOperation operation,int currentUserId)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            RecordedById = currentUserId;

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            OperationDate = Operation.OperationDate == default ? DateTime.Today : Operation.OperationDate;
            Type = string.IsNullOrWhiteSpace(Operation.Type) ? "income" : Operation.Type;
            Amount = Operation.Amount;
            Description = Operation.Description;
        }

        [Required(ErrorMessage = "Дата операции обязательна")]
        public DateTime OperationDate
        {
            get => _operationDate;
            set
            {
                if (SetField(ref _operationDate, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        [Required(ErrorMessage = "Тип операции обязателен")]
        [RegularExpression("income|expense", ErrorMessage = "Недопустимый тип операции")]
        public string Type
        {
            get => _type;
            set
            {
                if (SetField(ref _type, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        [Range(0.01, 1_000_000_000, ErrorMessage = "Сумма должна быть между 0.01 и 1 000 000 000")]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (SetField(ref _amount, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetField(ref _isBusy, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetField(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Событие запроса закрытия окна.
        /// Аргумент true - сохранить, false - отмена.
        /// </summary>
        public event Action<bool> RequestClose;

        private bool CanSave()
        {
            // Проверяем, что команда Save доступна только если:
            // - не идет операция сохранения
            // - сумма > 0
            // - тип операции корректен
            // - дата операции не в будущем
            return !IsBusy
                   && Amount > 0
                   && (Type == "income" || Type == "expense")
                   && OperationDate <= DateTime.Today;
        }

        private async Task SaveAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                var validationContext = new ValidationContext(this);
                var validationResults = new List<ValidationResult>();

                if (!Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true))
                {
                    StatusMessage = string.Join(Environment.NewLine, validationResults.ConvertAll(r => r.ErrorMessage));
                    return;
                }

                // Обновляем модель
                Operation.RecordedById = RecordedById;
                Operation.OperationDate = OperationDate;
                Operation.Type = Type;
                Operation.Amount = Amount;
                Operation.Description = Description;

                // Запрос закрытия окна с результатом "сохранить"
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения: {ex.Message}";
                await Logger.LogAsync($"EditFinancialOperationViewModel.SaveAsync error: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            // Запрос закрытия окна с результатом "отмена"
            RequestClose?.Invoke(false);
        }
    }
}
