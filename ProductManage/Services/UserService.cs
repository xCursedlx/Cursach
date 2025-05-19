using MySql.Data.MySqlClient;
using ProductManage.Data;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Services
{
    public static class UserService
    {
        /// <summary>
        /// Получить пользователя по логину
        /// </summary>
        public static async Task<User> GetUserByUsernameAsync(string username)
        {
            const string sql = @"SELECT id, username, password, full_name, role, is_active, created_at 
                                 FROM users WHERE username = @username LIMIT 1";

            var param = new MySqlParameter("@username", username);

            using var reader = await DatabaseManager.ExecuteReaderAsync(sql, param);
            if (await reader.ReadAsync())
            {
                return User.FromDataReader(reader);
            }
            return null;
        }

        /// <summary>
        /// Обновить профиль пользователя
        /// </summary>
        public static async Task UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            const string sql = @"UPDATE users SET 
                                    full_name = @full_name,
                                    password = @password,
                                    role = @role,
                                    is_active = @is_active
                                 WHERE id = @id";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@full_name", user.FullName ?? (object)DBNull.Value),
                new MySqlParameter("@password", user.Password),
                new MySqlParameter("@role", user.Role),
                new MySqlParameter("@is_active", user.IsActive),
                new MySqlParameter("@id", user.Id)
            };

            int affected = await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
            if (affected == 0)
                throw new Exception("Пользователь не найден для обновления");
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        public static async Task CreateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            const string sql = @"INSERT INTO users (username, password, full_name, role, is_active, created_at)
                                 VALUES (@username, @password, @full_name, @role, @is_active, @created_at)";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@username", user.Username),
                new MySqlParameter("@password", user.Password),
                new MySqlParameter("@full_name", user.FullName ?? (object)DBNull.Value),
                new MySqlParameter("@role", user.Role),
                new MySqlParameter("@is_active", user.IsActive),
                new MySqlParameter("@created_at", user.CreatedAt)
            };

            await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);

            // Получение последнего вставленного ID
            var idObj = await DatabaseManager.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
            if (idObj != null && int.TryParse(idObj.ToString(), out int id))
            {
                user.Id = id;
            }
        }

        /// <summary>
        /// Удалить пользователя по ID
        /// </summary>
        public static async Task DeleteUserAsync(int userId)
        {
            const string sql = @"DELETE FROM users WHERE id = @id";

            var param = new MySqlParameter("@id", userId);

            int affected = await DatabaseManager.ExecuteNonQueryAsync(sql, param);
            if (affected == 0)
                throw new Exception("Пользователь не найден для удаления");
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        public static async Task<List<User>> GetAllUsersAsync()
        {
            const string sql = @"SELECT id, username, password, full_name, role, is_active, created_at FROM users";

            var users = new List<User>();

            using var reader = await DatabaseManager.ExecuteReaderAsync(sql);
            while (await reader.ReadAsync())
            {
                users.Add(User.FromDataReader(reader));
            }

            return users;
        }
    }
}
