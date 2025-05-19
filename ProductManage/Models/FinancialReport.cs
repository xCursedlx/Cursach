using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace ProductManage.Models
{
    /// <summary>
    /// Модель финансового отчёта с операциями и суммами
    /// </summary>
    public class FinancialReport : INotifyPropertyChanged
    {
        private decimal _totalIncome;
        private decimal _totalExpense;
        private DateTime _startDate;
        private DateTime _endDate;

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Конечная дата отчёта
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    if (value < StartDate)
                        throw new ArgumentException("EndDate не может быть раньше StartDate");
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Начальная дата отчёта
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    if (value > EndDate)
                        throw new ArgumentException("StartDate не может быть позже EndDate");
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        

        /// <summary>
        /// Общая сумма доходов
        /// </summary>
        public decimal TotalIncome
        {
            get => _totalIncome;
            private set
            {
                if (_totalIncome != value)
                {
                    _totalIncome = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Profit));
                    OnPropertyChanged(nameof(TotalIncomeDisplay));
                }
            }
        }

        /// <summary>
        /// Общая сумма расходов
        /// </summary>
        public decimal TotalExpense
        {
            get => _totalExpense;
            private set
            {
                if (_totalExpense != value)
                {
                    _totalExpense = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Profit));
                    OnPropertyChanged(nameof(TotalExpenseDisplay));
                }
            }
        }

        /// <summary>
        /// Прибыль (доход минус расход)
        /// </summary>
        public decimal Profit => TotalIncome - TotalExpense;

        /// <summary>
        /// Отображение доходов в валютном формате
        /// </summary>
        public string TotalIncomeDisplay => TotalIncome.ToString("C");

        /// <summary>
        /// Отображение расходов в валютном формате
        /// </summary>
        public string TotalExpenseDisplay => TotalExpense.ToString("C");

        /// <summary>
        /// Отображение прибыли в валютном формате
        /// </summary>
        public string ProfitDisplay => Profit.ToString("C");

        /// <summary>
        /// Коллекция операций отчёта
        /// </summary>
        public ObservableCollection<FinancialOperation> Operations { get; } = new ObservableCollection<FinancialOperation>();

        /// <summary>
        /// Конструктор с установкой периода по умолчанию (последний месяц)
        /// </summary>
        public FinancialReport()
        {   
            EndDate = DateTime.Today;
            StartDate = DateTime.Today.AddMonths(-1);
            
        }

        /// <summary>
        /// Обновляет коллекцию операций, очищая старые и добавляя новые, а также пересчитывает итоги
        /// </summary>
        /// <param name="operations">Новые операции для отчёта</param>
        public void SetOperations(IEnumerable<FinancialOperation> operations)
        {
            Operations.Clear();
            if (operations != null)
            {
                foreach (var op in operations)
                    Operations.Add(op);
            }
            RecalculateTotals();
        }

        /// <summary>
        /// Устанавливает значения итогов напрямую
        /// </summary>
        /// <param name="income">Общий доход</param>
        /// <param name="expense">Общий расход</param>
        public void SetTotals(decimal income, decimal expense)
        {
            TotalIncome = income;
            TotalExpense = expense;
        }

        /// <summary>
        /// Пересчитывает итоги по операциям
        /// </summary>
        private void RecalculateTotals()
        {
            TotalIncome = Operations.Where(op => string.Equals(op.Type, "income", StringComparison.OrdinalIgnoreCase))
                                    .Sum(op => op.Amount);
            TotalExpense = Operations.Where(op => string.Equals(op.Type, "expense", StringComparison.OrdinalIgnoreCase))
                                     .Sum(op => op.Amount);
        }

        /// <summary>
        /// Генерация отчёта из заданного периода и коллекции операций
        /// </summary>
        /// <param name="start">Начальная дата</param>
        /// <param name="end">Конечная дата</param>
        /// <param name="operations">Коллекция операций</param>
        /// <returns>Финансовый отчёт</returns>
        public static FinancialReport Generate(DateTime start, DateTime end, IEnumerable<FinancialOperation> operations)
        {
            if (start > end)
                throw new ArgumentException("Start date cannot be later than end date.");

            var report = new FinancialReport
            {
                StartDate = start,
                EndDate = end
            };
            report.SetOperations(operations);
            return report;
        }

        /// <summary>
        /// Вспомогательный метод вызова события PropertyChanged
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
