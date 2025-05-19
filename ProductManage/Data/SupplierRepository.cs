using MySql.Data.MySqlClient;
using ProductManage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с поставщиками
    /// </summary>
    public class SupplierRepository
    {
        private readonly MySqlConnection _connection;

        public SupplierRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }

        /// <summary>
        /// Получает всех поставщиков из базы данных
        /// </summary>
        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            var suppliers = new List<Supplier>();
            const string sql = "SELECT id, name, contact_person, phone, email FROM suppliers";

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    suppliers.Add(Supplier.FromDataReader(reader));
                }
            }
            return suppliers;
        }

        /// <summary>
        /// Получает поставщика по идентификатору
        /// </summary>
        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            const string sql = "SELECT * FROM suppliers WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? Supplier.FromDataReader(reader) : null;
            }
        }

        /// <summary>
        /// Добавляет нового поставщика
        /// </summary>
        public async Task<int> AddSupplierAsync(Supplier supplier)
        {
            const string sql = @"INSERT INTO suppliers (name, contact_person, phone, email)
                         VALUES (@name, @contact_person, @phone, @email)";
            await DatabaseManager.ExecuteNonQueryAsync(sql, supplier.ToParameters());

            const string lastIdSql = "SELECT LAST_INSERT_ID();";
            var result = await DatabaseManager.ExecuteScalarAsync(lastIdSql);
            return Convert.ToInt32(result);
        }


        /// <summary>
        /// Обновляет существующего поставщика
        /// </summary>
        public async Task<int> UpdateSupplierAsync(Supplier supplier)
        {
            const string sql = @"UPDATE suppliers SET
                                name = @name,
                                contact_person = @contact_person,
                                phone = @phone,
                                email = @email
                                WHERE id = @id";

            var parameters = new List<MySqlParameter>(supplier.ToParameters())
            {
                new MySqlParameter("@id", supplier.Id)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }

        /// <summary>
        /// Удаляет поставщика по идентификатору
        /// </summary>
        public async Task<int> DeleteSupplierAsync(int id)
        {
            const string sql = "DELETE FROM suppliers WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Проверяет существование поставщика по имени
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            const string sql = "SELECT COUNT(1) FROM suppliers WHERE name = @name";
            var parameters = new[] { new MySqlParameter("@name", name) };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Ищет поставщиков по названию или контактному лицу
        /// </summary>
        public async Task<List<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            var suppliers = new List<Supplier>();
            const string sql = @"SELECT * FROM suppliers 
                               WHERE name LIKE @search 
                               OR contact_person LIKE @search";

            var parameters = new[]
            {
                new MySqlParameter("@search", $"%{searchTerm}%")
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    suppliers.Add(Supplier.FromDataReader(reader));
                }
            }
            return suppliers;
        }
    }
}