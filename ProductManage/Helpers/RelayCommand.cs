using System;
using System.Windows.Input;

namespace ProductManage.Helpers
{
    /// <summary>
    /// Реализация ICommand для делегирования вызовов
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Конструктор команды с возможностью выполнения
        /// </summary>
        /// <param name="execute">Метод выполнения</param>
        /// <param name="canExecute">Метод проверки возможности выполнения</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Конструктор команды без проверки выполнения (всегда доступна)
        /// </summary>
        public RelayCommand(Action execute) : this(_ => execute()) { }

        /// <summary>
        /// Проверка возможности выполнения команды
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Метод для принудительного обновления состояния команды
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}