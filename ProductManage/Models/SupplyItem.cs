using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace ProductManage.Models
{
    /// <summary>
    /// Класс-модель для работы с элементами поставки
    /// </summary>
    public class SupplyItem : INotifyPropertyChanged
    {
        private int _id;
        private int _supplyId;
        private int _productId;
        private int _quantity;
        private decimal _unitPrice;

        /// <summary>
        /// Уникальный идентификатор элемента поставки
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
        /// Идентификатор связанной поставки (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Необходимо указать поставку")]
        public int SupplyId
        {
            get => _supplyId;
            set
            {
                if (_supplyId != value)
                {
                    _supplyId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Идентификатор товара (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Необходимо выбрать товар")]
        public int ProductId
        {
            get => _productId;
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProductName));
                }
            }
        }

        /// <summary>
        /// Количество товара (должно быть больше 0)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть не менее 1")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        /// <summary>
        /// Цена за единицу товара (должна быть больше 0)
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        /// <summary>
        /// Общая стоимость элемента поставки
        /// </summary>
        public decimal TotalPrice => Quantity * UnitPrice;

        /// <summary>
        /// Название товара (загружается отдельно)
        /// </summary>
        public string ProductName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SupplyItem() { }

        /// <summary>
        /// Основной конструктор для создания элемента поставки
        /// </summary>
        public SupplyItem(int supplyId, int productId, int quantity, decimal unitPrice)
        {
            SupplyId = supplyId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        /// <summary>
        /// Создает объект SupplyItem из данных MySqlDataReader
        /// </summary>
        public static SupplyItem FromDataReader(MySqlDataReader reader)
        {
            return new SupplyItem(
                supplyId: reader.GetInt32("supply_id"),
                productId: reader.GetInt32("product_id"),
                quantity: reader.GetInt32("quantity"),
                unitPrice: reader.GetDecimal("unit_price"))
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
                new MySqlParameter("@supply_id", SupplyId),
                new MySqlParameter("@product_id", ProductId),
                new MySqlParameter("@quantity", Quantity),
                new MySqlParameter("@unit_price", UnitPrice)
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}