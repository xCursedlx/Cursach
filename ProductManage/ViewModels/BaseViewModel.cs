using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProductManage.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel, реализующий механизм уведомлений об изменениях свойств
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие для уведомления об изменении значения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод для вызова события PropertyChanged
        /// </summary>
        /// <param name="propertyName">Название изменившегося свойства (определяется автоматически)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и вызывает событие изменения свойства, если значение изменилось
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <param name="field">Ссылка на поле</param>
        /// <param name="value">Новое значение</param>
        /// <param name="propertyName">Название свойства (определяется автоматически)</param>
        /// <returns>True если значение изменилось, иначе False</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}