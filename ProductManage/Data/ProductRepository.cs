using MySql.Data.MySqlClient;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с товарами (Product)
    /// </summary>
    public class ProductRepository
    {
        /// <summary>
        /// Получить все товары из базы данных
        /// </summary>
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            const string sql = "SELECT id, name, category_id, price, quantity, description FROM products";

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    products.Add(Product.FromDataReader(reader));
                }
            }

            return products;
        }

        /// <summary>
        /// Получить товар по идентификатору
        /// </summary>
        public async Task<Product> GetProductByIdAsync(int id)
        {
            const string sql = "SELECT id, name, category_id, price, quantity, description FROM products WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                if (await reader.ReadAsync())
                {
                    return Product.FromDataReader(reader);
                }
            }

            return null;
        }

        /// <summary>
        /// Добавить новый товар в базу данных
        /// </summary>
        public async Task<int> AddProductAsync(Product product)
        {
            const string sql = @"
                INSERT INTO products (name, category_id, price, quantity, description)
                VALUES (@name, @category_id, @price, @quantity, @description)";

            await DatabaseManager.ExecuteNonQueryAsync(sql, product.ToParameters());

            const string lastIdSql = "SELECT LAST_INSERT_ID();";
            var result = await DatabaseManager.ExecuteScalarAsync(lastIdSql);

            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Обновить существующий товар
        /// </summary>
        public async Task<int> UpdateProductAsync(Product product)
        {
            const string sql = @"
                UPDATE products SET
                    name = @name,
                    category_id = @category_id,
                    price = @price,
                    quantity = @quantity,
                    description = @description
                WHERE id = @id";

            var parameters = new List<MySqlParameter>(product.ToParameters())
            {
                new MySqlParameter("@id", product.Id)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }

        /// <summary>
        /// Удалить товар по идентификатору
        /// </summary>
        public async Task<int> DeleteProductAsync(int id)
        {
            const string sql = "DELETE FROM products WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Поиск товаров по названию или описанию
        /// </summary>
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            var products = new List<Product>();
            const string sql = @"
                SELECT id, name, category_id, price, quantity, description FROM products
                WHERE name LIKE @search OR description LIKE @search";

            var parameters = new[]
            {
                new MySqlParameter("@search", $"%{searchTerm}%")
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    products.Add(Product.FromDataReader(reader));
                }
            }

            return products;
        }

        /// <summary>
        /// Обновить количество товара на складе (увеличить или уменьшить)
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="quantityChange">Изменение количества (может быть отрицательным)</param>
        public async Task<int> UpdateProductQuantityAsync(int productId, int quantityChange)
        {
            var product = await GetProductByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Товар не найден", nameof(productId));

            if (product.Quantity + quantityChange < 0)
                throw new InvalidOperationException("Недостаточно товара на складе");

            const string sql = @"UPDATE products SET quantity = quantity + @change WHERE id = @productId";

            var parameters = new[]
            {
                new MySqlParameter("@productId", productId),
                new MySqlParameter("@change", quantityChange)
            };

            return await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Получить товары, добавленные за последние 30 дней
        /// </summary>
        public async Task<List<Product>> GetNewProductsAsync()
        {
            var products = new List<Product>();

            const string sql = @"
                SELECT id, name, category_id, price, quantity, description, created_date
                FROM products
                WHERE created_date >= @sinceDate
                ORDER BY created_date DESC";

            var sinceDate = DateTime.Today.AddDays(-30);

            var parameters = new[]
            {
                new MySqlParameter("@sinceDate", sinceDate)
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    products.Add(Product.FromDataReader(reader));
                }
            }

            return products;
        }


    }
}
