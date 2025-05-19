using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с поставщиками
    /// </summary>
    public class Supplier : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _contactPerson;
        private string _phone;
        private string _email;

        /// <summary>
        /// Уникальный идентификатор поставщика
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
        /// Наименование поставщика (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Название поставщика обязательно")]
        [StringLength(200, ErrorMessage = "Максимальная длина названия - 200 символов")]
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
        /// Контактное лицо (необязательное поле)
        /// </summary>
        [StringLength(100, ErrorMessage = "Максимальная длина имени контакта - 100 символов")]
        public string ContactPerson
        {
            get => _contactPerson;
            set
            {
                if (_contactPerson != value)
                {
                    _contactPerson = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Номер телефона (необязательное поле)
        /// </summary>
        [Phone(ErrorMessage = "Некорректный формат номера телефона")]
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Электронная почта (необязательное поле)
        /// </summary>
        [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Supplier() { }

        /// <summary>
        /// Основной конструктор для создания поставщика
        /// </summary>
        public Supplier(string name, string contactPerson = null,
                      string phone = null, string email = null)
        {
            Name = name;
            ContactPerson = contactPerson;
            Phone = phone;
            Email = email;
        }

        /// <summary>
        /// Создает объект Supplier из данных MySqlDataReader
        /// </summary>
        public static Supplier FromDataReader(MySqlDataReader reader)
        {
            return new Supplier(
                name: reader.GetString("name"),
                contactPerson: reader.IsDBNull(reader.GetOrdinal("contact_person"))
                    ? null
                    : reader.GetString("contact_person"),
                phone: reader.IsDBNull(reader.GetOrdinal("phone"))
                    ? null
                    : reader.GetString("phone"),
                email: reader.IsDBNull(reader.GetOrdinal("email"))
                    ? null
                    : reader.GetString("email")
            )
            {
                Id = reader.GetInt32("id")
            };
        }

        /// <summary>
        /// Преобразует объект в параметры для SQL-запроса
        /// </summary>
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@name", Name),
                new MySqlParameter("@contact_person", ContactPerson ?? (object)DBNull.Value),
                new MySqlParameter("@phone", Phone ?? (object)DBNull.Value),
                new MySqlParameter("@email", Email ?? (object)DBNull.Value)
            };
        }

        /// <summary>
        /// Возвращает строковое представление поставщика
        /// </summary>
        public override string ToString() => Name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Форматированная контактная информация для отображения
        /// </summary>
        public string ContactInfo
        {
            get
            {
                var info = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(ContactPerson)) info.AppendLine(ContactPerson);
                if (!string.IsNullOrEmpty(Phone)) info.AppendLine($"Тел: {Phone}");
                if (!string.IsNullOrEmpty(Email)) info.AppendLine($"Email: {Email}");
                return info.ToString().Trim();
            }
        }
    }
}