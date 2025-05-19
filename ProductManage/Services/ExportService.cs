using K4os.Compression.LZ4;
using ProductManage.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProductManage.Services
{
    public class ExportService
    {
        /// <summary>
        /// Экспортирует данные в CSV-файл
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="data">Коллекция данных</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>True - успех, False - ошибка</returns>
        public async Task<bool> ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, char separator = ',')
        {
            if (data == null || !data.Any())
            {
                await Logger.LogAsync("Нет данных для экспорта.", LogLevel.Warning);
                return false;
            }

            try
            {
                CreateDirectoryIfNotExists(filePath);

                var properties = typeof(T).GetProperties();

                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                // Запись заголовка
                var header = string.Join(separator, properties.Select(p => EscapeCsvValue(p.Name, separator)));
                await writer.WriteLineAsync(header);

                // Запись данных
                foreach (var item in data)
                {
                    var values = properties.Select(p => EscapeCsvValue(p.GetValue(item)?.ToString() ?? "", separator));
                    await writer.WriteLineAsync(string.Join(separator, values));
                }

                return true;
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка при экспорте CSV: {ex}", LogLevel.Error);
                return false;
            }
        }

        private string EscapeCsvValue(string value, char separator)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool mustQuote = value.Contains(separator) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            if (mustQuote)
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }


        private void CreateDirectoryIfNotExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void WriteCsvContent<T>(IEnumerable<T> data, StreamWriter writer)
        {
            var properties = typeof(T).GetProperties();
            WriteHeader(writer, properties);
            WriteDataRows(data, writer, properties);
        }

        private void WriteHeader(StreamWriter writer, System.Reflection.PropertyInfo[] properties)
        {
            var header = string.Join(",", properties.Select(p => EscapeCsvValue(p.Name)));
            writer.WriteLine(header);
        }

        private void WriteDataRows<T>(IEnumerable<T> data, StreamWriter writer, System.Reflection.PropertyInfo[] properties)
        {
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                    EscapeCsvValue(p.GetValue(item)?.ToString() ?? ""));
                writer.WriteLine(string.Join(",", values));
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            return value.Contains(",") || value.Contains("\"") || value.Contains("\n")
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }

        public async Task<string> ExportFinancialReportToCsvAsync(FinancialReport report)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = $"FinancialReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(folder, fileName);

            var sb = new StringBuilder();

            // Заголовки
            sb.AppendLine("Дата;Тип;Сумма;Описание;Записал");

            // Данные операций
            foreach (var op in report.Operations)
            {
                string line = $"{op.OperationDate:dd.MM.yyyy};{op.Type};{op.Amount};{EscapeCsv(op.Description)};{op.RecordedById}";
                sb.AppendLine(line);
            }

            // Запись в файл
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

            return filePath;
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
    }
}