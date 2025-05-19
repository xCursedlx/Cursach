using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Модель товара с поддержкой уведомления об изменениях
    /// </summary>
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int? _categoryId;
        private decimal _price;
        private int _quantity;
        private string _description;
        private bool _isSelected;

        /// <summary>Уникальный идентификатор товара</summary>
        [Key]
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        /// <summary>Наименование товара</summary>
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Максимум 200 символов")]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>Идентификатор категории</summary>
        public int? CategoryId
        {
            get => _categoryId;
            set => SetField(ref _categoryId, value);
        }

        /// <summary>Цена товара</summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetField(ref _price, value))
                {
                    OnPropertyChanged(nameof(PriceDisplay));
                }
            }
        }

        /// <summary>Количество на складе</summary>
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(QuantityDisplay));
                }
            }
        }

        /// <summary>Описание товара</summary>
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        /// <summary>Флаг выбора товара (для UI)</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        /// <summary>Форматированная цена для отображения</summary>
        public string PriceDisplay => Price.ToString("C");

        /// <summary>Форматированное количество для отображения</summary>
        public string QuantityDisplay => $"{Quantity} шт.";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Создает объект Product из MySqlDataReader
        /// </summary>
        /// <param name="reader">Читатель данных</param>
        /// <returns>Объект Product</returns>
        public static Product FromDataReader(MySqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? (int?)null : reader.GetInt32("category_id"),
                Price = reader.GetDecimal("price"),
                Quantity = reader.GetInt32("quantity"),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description")
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
                new MySqlParameter("@name", Name),
                new MySqlParameter("@category_id", CategoryId ?? (object)DBNull.Value),
                new MySqlParameter("@price", Price),
                new MySqlParameter("@quantity", Quantity),
                new MySqlParameter("@description", Description ?? (object)DBNull.Value)
            };
        }

        /// <summary>
        /// Вызывает событие PropertyChanged
        /// </summary>
        /// <param name="propertyName">Имя изменённого свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и уведомляет об изменении
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <param name="field">Ссылка на поле</param>
        /// <param name="value">Новое значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>True, если значение изменилось</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public override string ToString() => $"{Name} ({Price:C})";
    }
}
