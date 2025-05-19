using MySql.Data.MySqlClient;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManage.Data
{
    public class FinancialRepository
    {
        private readonly MySqlConnection _connection;

        public FinancialRepository(MySqlConnection connection = null)
        {
            _connection = connection ?? DatabaseManager.GetConnection();
        }

        public async Task<FinancialReport> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new FinancialReport
            {
                StartDate = startDate,
                EndDate = endDate
            };

            decimal incomeTotal = 0m;
            decimal expenseTotal = 0m;

            const string totalsSql = @"
                SELECT type, SUM(amount) AS total
                FROM financial_operations
                WHERE operation_date BETWEEN @start AND @end
                GROUP BY type";

            var totalsParams = new[]
            {
                new MySqlParameter("@start", startDate),
                new MySqlParameter("@end", endDate)
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(totalsSql, totalsParams))
            {
                while (await reader.ReadAsync())
                {
                    var type = reader.GetString("type");
                    var total = reader.GetDecimal("total");

                    switch (type.ToLowerInvariant())
                    {
                        case "income":
                            incomeTotal = total;
                            break;
                        case "expense":
                            expenseTotal = total;
                            break;
                    }
                }
            }

            report.SetTotals(incomeTotal, expenseTotal);

            var operations = await GetOperationsByDateRangeAsync(startDate, endDate);
            report.SetOperations(operations);

            return report;
        }

        public async Task<IEnumerable<FinancialOperation>> GetOperationsByDateRangeAsync(DateTime start, DateTime end)
        {
            var operations = new List<FinancialOperation>();

            // Добавлен столбец recorded_by в запрос
            const string sql = @"
                SELECT id, operation_date, type, amount, description, recorded_by
                FROM financial_operations
                WHERE operation_date BETWEEN @start AND @end
                ORDER BY operation_date DESC";

            var parameters = new[]
            {
                new MySqlParameter("@start", start),
                new MySqlParameter("@end", end)
            };

            using (var reader = await DatabaseManager.ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    var operation = FinancialOperation.FromDataReader(reader);
                    operations.Add(operation);
                }
            }

            return operations;
        }

        public async Task AddAsync(FinancialOperation operation)
        {
            const string sql = @"
                INSERT INTO financial_operations 
                    (operation_date, type, amount, description, recorded_by)
                VALUES 
                    (@operation_date, @type, @amount, @description, @recorded_by)";

            var parameters = new[]
            {
                new MySqlParameter("@operation_date", operation.OperationDate),
                new MySqlParameter("@type", operation.Type),
                new MySqlParameter("@amount", operation.Amount),
                new MySqlParameter("@description", operation.Description ?? (object)DBNull.Value),
                new MySqlParameter("@recorded_by", operation.RecordedById)
            };

            await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task UpdateAsync(FinancialOperation operation)
        {
            const string sql = @"
                UPDATE financial_operations
                SET 
                    operation_date = @operation_date,
                    type = @type,
                    amount = @amount,
                    description = @description,
                    recorded_by = @recorded_by
                WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@operation_date", operation.OperationDate),
                new MySqlParameter("@type", operation.Type),
                new MySqlParameter("@amount", operation.Amount),
                new MySqlParameter("@description", operation.Description ?? (object)DBNull.Value),
                new MySqlParameter("@recorded_by", operation.RecordedById),
                new MySqlParameter("@id", operation.Id)
            };

            await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM financial_operations WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };
            await DatabaseManager.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
