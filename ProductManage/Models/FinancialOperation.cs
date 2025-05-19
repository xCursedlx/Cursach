using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс, представляющий финансовую операцию (доход/расход)
    /// </summary>
    public class FinancialOperation : INotifyPropertyChanged
    {
        private int _id;
        private string _type;
        private decimal _amount;
        private string _description;
        private DateTime _operationDate;
        private int _recordedById;

        /// <summary>
        /// Уникальный идентификатор операции (первичный ключ)
        /// </summary>
        [Key]
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Тип операции: income (доход) или expense (расход)
        /// </summary>
        [Required(ErrorMessage = "Тип операции обязателен")]
        [RegularExpression("income|expense", ErrorMessage = "Недопустимый тип операции")]
        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Сумма операции (должна быть больше 0)
        /// </summary>
        [Range(0.01, 1000000000, ErrorMessage = "Сумма должна быть между 0.01 и 1 000 000 000")]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Описание операции (необязательное поле)
        /// </summary>
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Дата совершения операции (по умолчанию текущая дата)
        /// </summary>
        [Required(ErrorMessage = "Дата операции обязательна")]
        public DateTime OperationDate
        {
            get => _operationDate;
            set
            {
                if (_operationDate != value)
                {
                    _operationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Идентификатор пользователя, записавшего операцию
        /// </summary>
        [Required(ErrorMessage = "Идентификатор пользователя обязателен")]
        public int RecordedById
        {
            get => _recordedById;
            set
            {
                if (_recordedById != value)
                {
                    _recordedById = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Конструктор по умолчанию (инициализирует дату текущей датой)
        /// </summary>
        public FinancialOperation()
        {
            OperationDate = DateTime.Today;
        }

        /// <summary>
        /// Основной конструктор для создания новой операции
        /// </summary>
        /// <param name="type">Тип операции (income/expense)</param>
        /// <param name="amount">Сумма операции</param>
        /// <param name="operationDate">Дата операции</param>
        /// <param name="recordedById">ID пользователя</param>
        /// <param name="description">Описание (необязательно)</param>
        public FinancialOperation(string type, decimal amount, DateTime operationDate,
                                int recordedById, string description = null)
        {
            Type = type;
            Amount = amount;
            Description = description;
            OperationDate = operationDate;
            RecordedById = recordedById;
        }

        /// <summary>
        /// Метод для генерации события изменения свойства
        /// </summary>
        /// <param name="propertyName">Название измененного свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Создает объект FinancialOperation из данных MySqlDataReader
        /// </summary>
        /// <param name="reader">Объект чтения данных из БД</param>
        /// <returns>Заполненный объект FinancialOperation</returns>
        public static FinancialOperation FromDataReader(MySqlDataReader reader)
        {
            return new FinancialOperation
            {
                Id = reader.GetInt32("id"),
                Type = reader.GetString("type"),
                Amount = reader.GetDecimal("amount"),
                OperationDate = reader.GetDateTime("operation_date"),
                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString("description"),
                RecordedById = reader.IsDBNull(reader.GetOrdinal("recorded_by"))
                    ? 0 
                    : reader.GetInt32("recorded_by")
            };
        }


        /// <summary>
        /// Преобразует объект в массив параметров для SQL-запроса
        /// </summary>
        /// <returns>Массив MySqlParameter</returns>
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@type", Type),
                new MySqlParameter("@amount", Amount),
                new MySqlParameter("@description", Description ?? (object)DBNull.Value),
                new MySqlParameter("@operation_date", OperationDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@recorded_by", RecordedById)
            };
        }

        /// <summary>
        /// Возвращает строковое представление операции
        /// </summary>
        public override string ToString() =>
            $"{OperationDate:dd.MM.yyyy} - {Type} {Amount:C} ({Description?.Truncate(20)})";

        /// <summary>
        /// Локализованное отображение типа операции для UI
        /// </summary>
        public string TypeDisplay => Type == "income" ? "Доход" : "Расход";

        /// <summary>
        /// Отформатированная сумма для отображения в UI
        /// </summary>
        public string AmountDisplay => $"{Amount:C}";


    }

    /// <summary>
    /// Расширение для обрезания длинных строк
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Обрезает строку до указанной длины, добавляя многоточие при необходимости
        /// </summary>
        /// <param name="value">Исходная строка</param>
        /// <param name="maxLength">Максимальная длина</param>
        /// <returns>Обрезанная строка</returns>
        public static string Truncate(this string value, int maxLength)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : value.Length <= maxLength
                    ? value
                    : value.Substring(0, maxLength) + "...";
        }
    }

}