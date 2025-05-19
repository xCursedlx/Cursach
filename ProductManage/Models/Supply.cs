using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с поставками товаров
    /// </summary>
    public class Supply : INotifyPropertyChanged
    {
        private int _id;
        private int? _supplierId;
        private string _invoiceNumber;

        private DateTime _deliveryDate;
        private string _status;
        private decimal _totalAmount;
        private int _createdBy;

        public List<SupplyItem> Items { get; set; } = new List<SupplyItem>();

        /// <summary>
        /// Уникальный идентификатор поставки
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
        /// Идентификатор поставщика (может быть null)
        /// </summary>
        public int? SupplierId
        {
            get => _supplierId;
            set
            {
                if (_supplierId != value)
                {
                    _supplierId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SupplierName));
                }
            }
        }

        /// <summary>
        /// Уникальный номер накладной (максимум 50 символов)
        /// </summary>
        [StringLength(50, ErrorMessage = "Номер накладной не должен превышать 50 символов")]
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set
            {
                if (_invoiceNumber != value)
                {
                    _invoiceNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Дата поставки (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Дата поставки обязательна")]
        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set
            {
                if (_deliveryDate != value)
                {
                    _deliveryDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedDeliveryDate));
                }
            }
        }

        /// <summary>
        /// Статус поставки: pending, delivered, cancelled
        /// </summary>
        [RegularExpression("pending|delivered|cancelled", ErrorMessage = "Недопустимый статус")]
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        /// <summary>
        /// Общая сумма поставки
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Сумма не может быть отрицательной")]
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalAmountDisplay));
                }
            }
        }

        /// <summary>
        /// Идентификатор пользователя, создавшего поставку
        /// </summary>
        [Required(ErrorMessage = "Идентификатор пользователя обязателен")]
        public int CreatedBy
        {
            get => _createdBy;
            set
            {
                if (_createdBy != value)
                {
                    _createdBy = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreatedByName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Supply()
        {
            DeliveryDate = DateTime.Today;
            Status = "pending";
            TotalAmount = 0;
        }

        /// <summary>
        /// Форматированная дата поставки для отображения
        /// </summary>
        public string FormattedDeliveryDate => DeliveryDate.ToString("dd.MM.yyyy");

        /// <summary>
        /// Локализованное название статуса
        /// </summary>
        public string StatusDisplay => Status switch
        {
            "pending" => "Ожидает",
            "delivered" => "Доставлено",
            "cancelled" => "Отменено",
            _ => "Неизвестно"
        };

        /// <summary>
        /// Название поставщика (загружается отдельно)
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// Имя пользователя (загружается отдельно)
        /// </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Форматированная сумма поставки
        /// </summary>
        public string TotalAmountDisplay => $"{TotalAmount:C}";

        /// <summary>
        /// Создает объект Supply из данных MySqlDataReader
        /// </summary>
        public static Supply FromDataReader(MySqlDataReader reader)
        {
            return new Supply
            {
                Id = reader.GetInt32("id"),
                SupplierId = reader.IsDBNull(reader.GetOrdinal("supplier_id"))
                    ? (int?)null
                    : reader.GetInt32("supplier_id"),
                InvoiceNumber = reader.IsDBNull(reader.GetOrdinal("invoice_number"))
                    ? null
                    : reader.GetString("invoice_number"),
                DeliveryDate = reader.GetDateTime("delivery_date"),
                Status = reader.GetString("status"),
                TotalAmount = reader.GetDecimal("total_amount"),
                CreatedBy = reader.GetInt32("created_by")
            };
        }

        /// <summary>
        /// Преобразует объект в параметры для SQL-запроса
        /// </summary>
        public MySqlParameter[] ToParameters()
        {
            return new MySqlParameter[]
            {
                new MySqlParameter("@supplier_id", SupplierId ?? (object)DBNull.Value),
                new MySqlParameter("@invoice_number", InvoiceNumber ?? (object)DBNull.Value),
                new MySqlParameter("@delivery_date", DeliveryDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@status", Status),
                new MySqlParameter("@total_amount", TotalAmount),
                new MySqlParameter("@created_by", CreatedBy)
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}