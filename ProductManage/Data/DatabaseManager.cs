using K4os.Compression.LZ4;
using MySql.Data.MySqlClient;
using ProductManage.Services;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    /// <summary>
    /// Централизованный менеджер для работы с базой данных MySQL
    /// </summary>
    public static class DatabaseManager
    {
        private static readonly string connectionString;

        /// <summary>
        /// Статический конструктор инициализирует подключение
        /// </summary>
        static DatabaseManager()
        {
            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        }

        /// <summary>
        /// Создает и возвращает новое подключение к БД
        /// </summary>
        /// <returns>Объект подключения MySqlConnection</returns>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static Task InitializeAsync()
        {
            return TestConnection();
        }
        /// <summary>
        /// Тестирование подключения к базе данных
        /// </summary>
        /// <exception cref="InvalidOperationException">Выбрасывается при неудачном подключении</exception>
        private static async Task TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    await Logger.LogAsync("Успешное подключение к базе данных", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка подключения: {ex.Message}", LogLevel.Error);
                throw new InvalidOperationException("Не удалось подключиться к БД", ex);
            }
        }

        public static async Task<object> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(parameters);
                        return await cmd.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка выполнения скалярного запроса: {ex.Message}\nSQL: {sql}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Выполняет SQL-запрос без возвращаемых данных
        /// </summary>
        /// <param name="sql">SQL-команда</param>
        /// <param name="parameters">Параметры команды</param>
        /// <returns>Количество затронутых строк</returns>
        public static async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                            command.Parameters.AddRange(parameters);
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка выполнения запроса: {ex.Message}\nSQL: {sql}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Выполняет SQL-запрос с возвратом данных
        /// </summary>
        /// <param name="sql">SQL-запрос SELECT</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Объект для чтения результатов</returns>
        public static async Task<MySqlDataReader> ExecuteReaderAsync(string sql, params MySqlParameter[] parameters)
        {
            var connection = GetConnection();
            try
            {
                await connection.OpenAsync();
                var command = new MySqlCommand(sql, connection);
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);
                return await command.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                Logger.LogAsync($"Ошибка чтения данных: {ex.Message}\nSQL: {sql}", LogLevel.Error);
                throw;
            }
        }
    }
}