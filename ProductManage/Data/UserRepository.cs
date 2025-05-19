using MySql.Data.MySqlClient;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для работы с пользователями системы
    /// </summary>
    public class UserRepository
    {
        private readonly MySqlConnection _connection;

        public UserRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }
        public async Task<User> GetByUsernameAsync(string username)
        {
            const string sql = "SELECT * FROM users WHERE username = @username";
            var parameters = new[] { new MySqlParameter("@username", username) };

            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);

                await _connection.OpenAsync();
                using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    return await reader.ReadAsync() ? MapUserFromReader(reader) : null;
                }
            }
        }

        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        public async Task<int> AddUserAsync(User user)
        {
            const string sql = @"INSERT INTO users 
                               (username, password, full_name, role, is_active, created_at)
                               VALUES (@username, @password, @fullName, @role, @isActive, @createdAt);
                               SELECT LAST_INSERT_ID();";

            var parameters = new[]
            {
                new MySqlParameter("@username", user.Username),
                new MySqlParameter("@password", user.Password),
                new MySqlParameter("@fullName", user.FullName ?? (object)DBNull.Value),
                new MySqlParameter("@role", user.Role),
                new MySqlParameter("@isActive", user.IsActive),
                new MySqlParameter("@createdAt", user.CreatedAt)
            };

            var result = await ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Получение пользователя по ID
        /// </summary>
        public async Task<User> GetUserByIdAsync(int id)
        {
            const string sql = "SELECT * FROM users WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = await ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? MapUserFromReader(reader) : null;
            }
        }

        /// <summary>
        /// Получение пользователя по логину
        /// </summary>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            const string sql = "SELECT * FROM users WHERE username = @username";
            var parameters = new[] { new MySqlParameter("@username", username) };

            using (var reader = await ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? MapUserFromReader(reader) : null;
            }
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        public async Task<int> UpdateUserAsync(User user)
        {
            const string sql = @"UPDATE users SET
                                username = @username,
                                password = @password,
                                full_name = @fullName,
                                role = @role,
                                is_active = @isActive
                                WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@username", user.Username),
                new MySqlParameter("@password", user.Password),
                new MySqlParameter("@fullName", user.FullName ?? (object)DBNull.Value),
                new MySqlParameter("@role", user.Role),
                new MySqlParameter("@isActive", user.IsActive),
                new MySqlParameter("@id", user.Id)
            };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Удаление пользователя по ID
        /// </summary>
        public async Task<int> DeleteUserAsync(int id)
        {
            const string sql = "DELETE FROM users WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Получение всех пользователей
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            const string sql = "SELECT * FROM users";

            using (var reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    users.Add(MapUserFromReader(reader));
                }
            }
            return users;
        }

        /// <summary>
        /// Проверка существования пользователя по логину
        /// </summary>
        public async Task<bool> UserExistsAsync(string username)
        {
            const string sql = "SELECT COUNT(1) FROM users WHERE username = @username";
            var parameters = new[] { new MySqlParameter("@username", username) };

            var result = await ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Обновление статуса активности пользователя
        /// </summary>
        public async Task<int> UpdateUserStatusAsync(int userId, bool isActive)
        {
            const string sql = "UPDATE users SET is_active = @isActive WHERE id = @userId";
            var parameters = new[]
            {
                new MySqlParameter("@isActive", isActive),
                new MySqlParameter("@userId", userId)
            };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            const string sql = "SELECT * FROM users WHERE username = @username AND password = @password";
            var parameters = new[]
            {
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password)
            };

            using (var reader = await ExecuteReaderAsync(sql, parameters))
            {
                return await reader.ReadAsync() ? MapUserFromReader(reader) : null;
            }
        }

        private User MapUserFromReader(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ?
                    null : reader.GetString("full_name"),
                Role = reader.GetString("role"),
                IsActive = reader.GetBoolean("is_active"),
                CreatedAt = reader.GetDateTime("created_at")
            };
        }

        #region Database Operations Helpers
        private async Task<MySqlDataReader> ExecuteReaderAsync(string sql, params MySqlParameter[] parameters)
        {
            var cmd = new MySqlCommand(sql, _connection);
            cmd.Parameters.AddRange(parameters);

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            return (MySqlDataReader)await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }


        private async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<object> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                return await cmd.ExecuteScalarAsync();
            }
        }
        #endregion
    }
}