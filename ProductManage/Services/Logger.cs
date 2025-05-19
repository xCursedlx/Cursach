using System;
using System.IO;
using System.Threading.Tasks;
namespace ProductManage.Services
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath = "logs/app.log";

        public static void Initialize(string logDirectory = "logs")
        {
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.log");
        }

        public static async Task LogAsync(string message, LogLevel level = LogLevel.Info)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

                // Используем FileStream с асинхронной записью и блокировку для потокобезопасности
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, logEntry);
                }
                await Task.CompletedTask; 
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка логирования: {ex.Message}");
            }
        }

    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}