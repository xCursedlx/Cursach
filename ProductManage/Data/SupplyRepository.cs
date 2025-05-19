using MySql.Data.MySqlClient;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с поставками товаров
    /// </summary>
    public class SupplyRepository
    {
        private readonly MySqlConnection _connection;

        public SupplyRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }

        private readonly SupplyItemRepository _itemRepository = new SupplyItemRepository();

        /// <summary>
        /// Добавляет новую поставку
        /// </summary>
        public async Task<int> AddSupplyAsync(Supply supply)
        {
            const string sql = @"INSERT INTO supplies 
                       (supplier_id, invoice_number, delivery_date, status, total_amount, created_by)
                       VALUES (@supplier_id, @invoice_number, @delivery_date, @status, @total_amount, @created_by);
                       SELECT LAST_INSERT_ID();";

            var result = await DatabaseManager.ExecuteScalarAsync(sql, ToParameters(supply));
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Получает поставку по ID
        /// </summary>
        public async Task<Supply> GetSupplyByIdAsync(int id)
        {
            const string sql = "SELECT s.*, sup.name AS supplier_name, u.full_name AS created_by_name " +
                "FROM supplies s " +
                "LEFT JOIN suppliers sup ON s.supplier_id = sup.id " +
                "LEFT JOIN users u ON s.created_by = u.id " +
                "WHERE s.id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                if (!await reader.ReadAsync()) return null;

                var supply = Supply.FromDataReader(reader);
                supply.SupplierName = reader.IsDBNull(reader.GetOrdinal("supplier_name")) ? null : reader.GetString("supplier_name");
                supply.CreatedByName = reader.IsDBNull(reader.GetOrdinal("created_by_name")) ? null : reader.GetString("created_by_name");
                supply.Items = await _itemRepository.GetItemsBySupplyAsync(id);

                return supply;
            }
        }


        /// <summary>
        /// Обновляет данные поставки
        /// </summary>
        public async Task<int> UpdateSupplyAsync(Supply supply)
        {
            const string sql = @"UPDATE supplies SET
                                supplier_id = @supplier_id,
                                invoice_number = @invoice_number,
                                delivery_date = @delivery_date,
                                status = @status
                                WHERE id = @id";

            return await DatabaseManager.ExecuteNonQueryAsync(sql, ToParameters(supply, true));
        }

        /// <summary>
        /// Удаляет поставку по ID
        /// </summary>
        public async Task<int> DeleteSupplyAsync(int id)
        {
            const string sql = "DELETE FROM supplies WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Получает все поставки с фильтрацией по статусу
        /// </summary>
        public async Task<List<Supply>> GetSuppliesByStatusAsync(string status)
        {
            var supplies = new List<Supply>();
            const string sql = "SELECT * FROM supplies WHERE status = @status ORDER BY delivery_date DESC";

            var parameters = new[]
            {
                new MySqlParameter("@status", status)
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    supplies.Add(Supply.FromDataReader(reader));
                }
            }
            return supplies;
        }

        /// <summary>
        /// Обновляет статус поставки
        /// </summary>
        public async Task<int> UpdateSupplyStatusAsync(int supplyId, string newStatus)
        {
            const string sql = "UPDATE supplies SET status = @status WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@status", newStatus),
                new MySqlParameter("@id", supplyId)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Получает поставки за указанный период
        /// </summary>
        public async Task<List<Supply>> GetSuppliesByDateRangeAsync(DateTime start, DateTime end)
        {
            var supplies = new List<Supply>();
            const string sql = @"SELECT * FROM supplies 
                               WHERE delivery_date BETWEEN @start AND @end
                               ORDER BY delivery_date DESC";

            var parameters = new[]
            {
                new MySqlParameter("@start", start.ToString("yyyy-MM-dd")),
                new MySqlParameter("@end", end.ToString("yyyy-MM-dd"))
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    supplies.Add(Supply.FromDataReader(reader));
                }
            }
            return supplies;
        }

        /// <summary>
        /// Проверяет уникальность номера накладной
        /// </summary>
        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
        {
            const string sql = "SELECT COUNT(1) FROM supplies WHERE invoice_number = @invoice_number";
            var parameters = new[] { new MySqlParameter("@invoice_number", invoiceNumber) };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }

        private async Task<string> GetSupplierNameAsync(int? supplierId)
        {
            if (!supplierId.HasValue) return null;

            var supplierRepo = new SupplierRepository();
            var supplier = await supplierRepo.GetSupplierByIdAsync(supplierId.Value);
            return supplier?.Name;
        }

        private async Task<string> GetUserNameAsync(int userId)
        {
            var userRepo = new UserRepository();
            var user = await userRepo.GetUserByIdAsync(userId);
            return user?.FullName;
        }

        private MySqlParameter[] ToParameters(Supply supply, bool includeId = false)
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@supplier_id", supply.SupplierId ?? (object)DBNull.Value),
                new MySqlParameter("@invoice_number", supply.InvoiceNumber ?? (object)DBNull.Value),
                new MySqlParameter("@delivery_date", supply.DeliveryDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@status", supply.Status),
                new MySqlParameter("@total_amount", supply.TotalAmount),
                new MySqlParameter("@created_by", supply.CreatedBy)
            };

            if (includeId)
            {
                parameters.Add(new MySqlParameter("@id", supply.Id));
            }

            return parameters.ToArray();
        }

        /// <summary>
        /// Получает общую сумму поставки через её элементы
        /// </summary>
        public async Task<decimal> CalculateTotalAmountAsync(int supplyId)
        {
            const string sql = @"SELECT SUM(quantity * unit_price) 
                               FROM supply_items 
                               WHERE supply_id = @supplyId";

            var parameters = new[]
            {
                new MySqlParameter("@supplyId", supplyId)
            };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }

        public async Task<List<Supply>> GetAllSuppliesAsync()
        {
            var supplies = new List<Supply>();
            const string sql = "SELECT * FROM supplies ORDER BY delivery_date DESC";

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    supplies.Add(Supply.FromDataReader(reader));
                }
            }
            return supplies;
        }
    }
}