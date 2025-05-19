using System;

namespace ProductManage.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }      // Например: INFO, ERROR, WARN
        public string Message { get; set; }
        public string Source { get; set; }     // Источник лога (класс, модуль)
    }
}
