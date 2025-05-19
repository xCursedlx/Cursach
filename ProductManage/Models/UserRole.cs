using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с ролями пользователей
    /// </summary>
    public class UserRole : INotifyPropertyChanged
    {
        private string _code;
        private string _displayName;
        private string _description;

        /// <summary>
        /// Системный код роли (admin, accountant, warehouse)
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Отображаемое название роли
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Подробное описание роли
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

        public UserRole() { }

        public UserRole(string code, string displayName, string description)
        {
            Code = code;
            DisplayName = displayName;
            Description = description;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Статический список доступных ролей системы
        /// </summary>
        public static readonly UserRole[] SystemRoles =
        {
            new UserRole("admin", "Администратор", "Полный доступ ко всем функциям системы"),
            new UserRole("accountant", "Бухгалтер", "Доступ к финансовым операциям и отчетам"),
            new UserRole("warehouse", "Складской работник", "Управление товарами и поставками")
        };

        /// <summary>
        /// Преобразует системный код роли в отображаемое название
        /// </summary>
        public static string GetDisplayName(string roleCode)
        {
            foreach (var role in SystemRoles)
            {
                if (role.Code == roleCode)
                    return role.DisplayName;
            }
            return "Неизвестная роль";
        }

        public override string ToString() => DisplayName;
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@code", Code),
                new MySqlParameter("@displayName", DisplayName),
                new MySqlParameter("@description", Description ?? (object)DBNull.Value)
            };
        }
    }
}