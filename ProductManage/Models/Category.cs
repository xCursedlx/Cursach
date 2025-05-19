using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с категориями товаров
    /// </summary>
    public class Category : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _description;

        /// <summary>
        /// Уникальный идентификатор категории
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
        /// Название категории (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Название категории обязательно")]
        [StringLength(100, ErrorMessage = "Максимальная длина названия - 100 символов")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Дополнительное описание категории
        /// </summary>
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

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Category() { }

        /// <summary>
        /// Основной конструктор для создания категории
        /// </summary>
        /// <param name="id">Идентификатор категории</param>
        /// <param name="name">Название категории</param>
        /// <param name="description">Описание (необязательно)</param>
        public Category(int id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Обработчик изменения свойств для INotifyPropertyChanged
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Преобразует результат запроса в объект Category
        /// </summary>
        /// <param name="reader">Объект данных из БД</param>
        /// <returns>Заполненный объект Category</returns>
        public static Category FromDataReader(MySqlDataReader reader)
        {
            return new Category(
                id: reader.GetInt32("id"),
                name: reader.GetString("name"),
                description: reader.IsDBNull(reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString("description")
            );
        }

        /// <summary>
        /// Преобразует объект в параметры для SQL-запроса
        /// </summary>
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@name", Name),
                new MySqlParameter("@description", Description ?? (object)DBNull.Value)
            };
        }

        /// <summary>
        /// Возвращает строковое представление категории
        /// </summary>
        public override string ToString() => Name;
    }
}