using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductManage.Helpers
{
    /// <summary>
    /// Асинхронная команда, реализующая ICommand, для MVVM.
    /// Позволяет выполнять асинхронные операции с блокировкой повторных вызовов и уведомлением UI.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;          // Асинхронное действие
        private readonly Func<bool> _canExecute;       // Условие выполнения команды
        private bool _isExecuting;                      // Флаг выполнения

        /// <summary>
        /// Создаёт новую команду.
        /// </summary>
        /// <param name="execute">Асинхронный метод для выполнения.</param>
        /// <param name="canExecute">Метод, определяющий, может ли команда выполняться.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, вызываемое при изменении возможности выполнения команды.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Проверяет, может ли команда выполняться.
        /// </summary>
        /// <param name="parameter">Параметр (не используется).</param>
        /// <returns>True, если команда может выполняться.</returns>
        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// Выполняет команду асинхронно.
        /// </summary>
        /// <param name="parameter">Параметр (не используется).</param>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Вызывает событие CanExecuteChanged для обновления состояния UI.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
