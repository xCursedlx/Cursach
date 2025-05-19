using ProductManage.Helpers;
using ProductManage.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class LogViewerViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LogEntry> _logs;
        private string _filterText;
        private bool _isBusy;
        private string _statusMessage;

        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set { _logs = value; OnPropertyChanged(); }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged();
                    FilterLogs();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    StatusMessage = _isBusy ? "Загрузка..." : string.Empty;
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        private ObservableCollection<LogEntry> _allLogs;

        public LogViewerViewModel()
        {
            Logs = new ObservableCollection<LogEntry>();
            _allLogs = new ObservableCollection<LogEntry>();
            RefreshCommand = new RelayCommand(async _ => await LoadLogsAsync());

            _ = LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Delay(500); // имитация загрузки

                var loadedLogs = new ObservableCollection<LogEntry>
                {
                    new LogEntry { Timestamp = DateTime.Now.AddMinutes(-5), Level = "INFO", Message = "Приложение запущено", Source = "System" },
                    new LogEntry { Timestamp = DateTime.Now.AddMinutes(-3), Level = "ERROR", Message = "Ошибка подключения к БД", Source = "DatabaseService" },
                    new LogEntry { Timestamp = DateTime.Now.AddMinutes(-1), Level = "WARN", Message = "Низкий уровень памяти", Source = "MemoryMonitor" },
                };

                _allLogs = loadedLogs;
                FilterLogs();
            }
            catch (Exception)
            {
                StatusMessage = "Ошибка при загрузке логов";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterLogs()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                Logs = new ObservableCollection<LogEntry>(_allLogs);
            }
            else
            {
                var filtered = new ObservableCollection<LogEntry>();
                foreach (var log in _allLogs)
                {
                    if (log.Message.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        log.Level.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        log.Source.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filtered.Add(log);
                    }
                }
                Logs = filtered;
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
