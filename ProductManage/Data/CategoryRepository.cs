using MySql.Data.MySqlClient;
using ProductManage.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с категориями товаров
    /// </summary>
    public class CategoryRepository
    {
        private readonly MySqlConnection _connection;
        public CategoryRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }
        /// <summary>
        /// Получает все категории из базы данных
        /// </summary>
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();
            const string sql = "SELECT id, name, description FROM categories";

            using (var cmd = new MySqlCommand(sql, _connection))
            {
                using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(Category.FromDataReader(reader));
                    }
                }
            }
            return categories;
        }

        /// <summary>
        /// Получает категорию по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор категории</param>
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            const string sql = "SELECT id, name, description FROM categories WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? Category.FromDataReader(reader) : null;
            }
        }

        /// <summary>
        /// Добавляет новую категорию в базу данных
        /// </summary>
        /// <param name="category">Объект категории для добавления</param>
        public async Task<int> AddCategoryAsync(Category category)
        {
            const string sql = @"INSERT INTO categories (name, description) 
                               VALUES (@name, @description);
                               SELECT LAST_INSERT_ID();";
            var result = await DatabaseManager.ExecuteScalarAsync(sql, category.ToParameters());
            return Convert.ToInt32(result);
        }

            /// <summary>
            /// Обновляет существующую категорию
            /// </summary>
            /// <param name="category">Объект категории с обновленными данными</param>
            public async Task<int> UpdateCategoryAsync(Category category)
        {
            const string sql = @"UPDATE categories SET 
                                name = @name,
                                description = @description
                                WHERE id = @id";

            var parameters = new List<MySqlParameter>(category.ToParameters())
            {
                new MySqlParameter("@id", category.Id)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }

        /// <summary>
        /// Удаляет категорию по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор категории для удаления</param>
        public async Task<int> DeleteCategoryAsync(int id)
        {
            const string sql = "DELETE FROM categories WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Проверяет существование категории по имени
        /// </summary>
        /// <param name="name">Название категории</param>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            const string sql = "SELECT COUNT(1) FROM categories WHERE name = @name";
            var parameters = new[] { new MySqlParameter("@name", name) };

            var result = await DatabaseManager.ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }
       
    }
}