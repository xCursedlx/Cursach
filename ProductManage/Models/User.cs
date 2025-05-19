using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с пользователями системы
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        private int _id;
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private bool _isActive;
        private DateTime _createdAt;

        /// <summary>
        /// Уникальный идентификатор пользователя
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
        /// Логин пользователя (обязательное поле, уникальное)
        /// </summary>
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, ErrorMessage = "Логин не должен превышать 50 символов")]
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Пароль пользователя (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(255, ErrorMessage = "Пароль не должен превышать 255 символов")]
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Полное имя пользователя (необязательное поле)
        /// </summary>
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Роль пользователя в системе (admin, accountant, warehouse)
        /// </summary>
        [Required(ErrorMessage = "Роль обязательна")]
        [RegularExpression("admin|accountant|warehouse",
            ErrorMessage = "Недопустимая роль пользователя")]
        public string Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RoleDisplay));
                }
            }
        }

        /// <summary>
        /// Статус активности аккаунта
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        /// <summary>
        /// Дата и время создания аккаунта
        /// </summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreatedAtDisplay));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public User()
        {
            CreatedAt = DateTime.Now;
            IsActive = false;
        }

        /// <summary>
        /// Локализованное отображение роли
        /// </summary>
        public string RoleDisplay => Role switch
        {
            "admin" => "Администратор",
            "accountant" => "Бухгалтер",
            "warehouse" => "Складской работник",
            _ => "Неизвестная роль"
        };

        /// <summary>
        /// Статус аккаунта для отображения
        /// </summary>
        public string StatusDisplay => IsActive ? "Активен" : "Неактивен";

        /// <summary>
        /// Форматированная дата создания
        /// </summary>
        public string CreatedAtDisplay => CreatedAt.ToString("dd.MM.yyyy HH:mm");

        /// <summary>
        /// Создает объект User из данных MySqlDataReader
        /// </summary>
        public static User FromDataReader(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                FullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                    ? null
                    : reader.GetString("full_name"),
                Role = reader.GetString("role"),
                IsActive = reader.GetBoolean("is_active"),
                CreatedAt = reader.GetDateTime("created_at")
            };
        }

        /// <summary>
        /// Преобразует объект в параметры для SQL-запроса
        /// </summary>
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@username", Username),
                new MySqlParameter("@password", Password),
                new MySqlParameter("@full_name", FullName ?? (object)DBNull.Value),
                new MySqlParameter("@role", Role),
                new MySqlParameter("@is_active", IsActive),
                new MySqlParameter("@created_at", CreatedAt)
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Возвращает строковое представление пользователя
        /// </summary>
        public override string ToString() => $"{Username} ({RoleDisplay})";
    }
}