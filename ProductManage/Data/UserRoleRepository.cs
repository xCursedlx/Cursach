using MySql.Data.MySqlClient;
using ProductManage.Models;
using ProductManage.Services;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Репозиторий для управления ролями пользователей
    /// </summary>
    public class UserRoleRepository
    {
        private readonly MySqlConnection _connection;

        public UserRoleRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }

        /// <summary>
        /// Получает все роли из базы данных
        /// </summary>
        public async Task<List<UserRole>> GetAllRolesAsync()
        {
            var roles = new List<UserRole>();
            const string sql = "SELECT code, display_name, description FROM roles";

            if (_connection.State != System.Data.ConnectionState.Open)
        await _connection.OpenAsync();

            using (var cmd = new MySqlCommand(sql, _connection))
            {
                try
                {
                    using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(MapFromReader(reader));
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logger.LogAsync($"Ошибка загрузки ролей: {ex.Message}", LogLevel.Error);
                    throw; // или обработайте ошибку
                }

            }
            return roles;
        }

        /// <summary>
        /// Получает роль по коду
        /// </summary>
        public async Task<UserRole> GetRoleByCodeAsync(string roleCode)
        {
            const string sql = "SELECT * FROM roles WHERE code = @code";
            var parameters = new[] { new MySqlParameter("@code", roleCode) };

            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);
                var dbReader = await cmd.ExecuteReaderAsync();

                if (dbReader is MySqlDataReader mysqlReader)
                {
                    using (mysqlReader)
                    {
                        return await mysqlReader.ReadAsync() ? MapFromReader(mysqlReader) : null;
                    }
                }
                throw new InvalidOperationException("Неожиданный тип DataReader");
            }
        }

        /// <summary>
        /// Добавляет новую роль
        /// </summary>
        public async Task<int> AddRoleAsync(UserRole role)
        {
            const string sql = @"INSERT INTO roles 
                               (code, display_name, description)
                               VALUES (@code, @displayName, @description)";

            return await ExecuteNonQueryAsync(sql, role.ToParameters());
        }

        /// <summary>
        /// Обновляет существующую роль
        /// </summary>
        public async Task<int> UpdateRoleAsync(UserRole role)
        {
            const string sql = @"UPDATE roles SET
                                display_name = @displayName,
                                description = @description
                                WHERE code = @code";

            return await ExecuteNonQueryAsync(sql, role.ToParameters());
        }

        /// <summary>
        /// Удаляет роль по коду
        /// </summary>
        public async Task<int> DeleteRoleAsync(string roleCode)
        {
            const string sql = "DELETE FROM roles WHERE code = @code";
            var parameters = new[] { new MySqlParameter("@code", roleCode) };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Проверяет существование роли по коду
        /// </summary>
        public async Task<bool> RoleExistsAsync(string roleCode)
        {
            const string sql = "SELECT COUNT(1) FROM roles WHERE code = @code";
            var parameters = new[] { new MySqlParameter("@code", roleCode) };

            var result = await ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result) > 0;
        }

        private UserRole MapFromReader(MySqlDataReader reader)
        {
            return new UserRole(
                reader.GetString("code"),
                reader.GetString("display_name"),
                reader.GetString("description")
            );
        }



        #region Helpers
        private async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteNonQueryAsync();
            }
        }


        private async Task<object> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteScalarAsync();
            }
        }
        #endregion
    }
}