// Data/SupplyItemRepository.cs
using MySql.Data.MySqlClient;
using ProductManage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с элементами поставок
    /// </summary>
    public class SupplyItemRepository
    {
        private readonly MySqlConnection _connection;

        public SupplyItemRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }

        /// <summary>
        /// Добавляет новый элемент поставки
        /// </summary>
        public async Task<int> AddSupplyItemAsync(SupplyItem item)
        {
            const string sql = @"INSERT INTO supply_items (supply_id, product_id, quantity, unit_price)
                         VALUES (@supply_id, @product_id, @quantity, @unit_price)";
            await DatabaseManager.ExecuteNonQueryAsync(sql, item.ToParameters());

            const string lastIdSql = "SELECT LAST_INSERT_ID();";
            var result = await DatabaseManager.ExecuteScalarAsync(lastIdSql);
            return Convert.ToInt32(result);
        }


        /// <summary>
        /// Получает все элементы поставки для указанной поставки
        /// </summary>
        public async Task<List<SupplyItem>> GetItemsBySupplyAsync(int supplyId)
        {
            var items = new List<SupplyItem>();
            const string sql = "SELECT * FROM supply_items WHERE supply_id = @supplyId";

            var parameters = new[]
            {
                new MySqlParameter("@supplyId", supplyId)
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    items.Add(SupplyItem.FromDataReader(reader));
                }
            }
            return items;
        }

        /// <summary>
        /// Обновляет элемент поставки
        /// </summary>
        public async Task<int> UpdateSupplyItemAsync(SupplyItem item)
        {
            const string sql = @"UPDATE supply_items SET
                                quantity = @quantity,
                                unit_price = @unit_price
                                WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@quantity", item.Quantity),
                new MySqlParameter("@unit_price", item.UnitPrice),
                new MySqlParameter("@id", item.Id)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Удаляет элемент поставки по ID
        /// </summary>
        public async Task<int> DeleteSupplyItemAsync(int itemId)
        {
            const string sql = "DELETE FROM supply_items WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", itemId) };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Получает элемент поставки по ID
        /// </summary>
        public async Task<SupplyItem> GetSupplyItemByIdAsync(int itemId)
        {
            const string sql = "SELECT * FROM supply_items WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", itemId) };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? SupplyItem.FromDataReader(reader) : null;
            }
        }

        /// <summary>
        /// Проверяет наличие товара в поставке
        /// </summary>
        public async Task<bool> ProductExistsInSupplyAsync(int supplyId, int productId)
        {
            const string sql = @"SELECT COUNT(1) FROM supply_items 
                       WHERE supply_id = @supplyId 
                       AND product_id = @productId";

            var parameters = new[]
            {
        new MySqlParameter("@supplyId", supplyId),
        new MySqlParameter("@productId", productId)
    };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Получает общее количество товаров в поставке
        /// </summary>
        public async Task<int> GetTotalItemsCountAsync(int supplyId)
        {
            const string sql = "SELECT SUM(quantity) FROM supply_items WHERE supply_id = @supplyId";
            var parameters = new[] { new MySqlParameter("@supplyId", supplyId) };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
        }
    }
}