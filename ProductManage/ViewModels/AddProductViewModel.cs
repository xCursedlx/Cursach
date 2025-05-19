using ProductManage.Data;
using ProductManage.Helpers;
using ProductManage.Models;
using ProductManage.Services;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProductManage.ViewModels
{
    public class AddProductViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly ProductRepository _repository = new ProductRepository();

        private Product _product = new Product();
        private string _statusMessage;
        private bool _isSaving;

        public Product Product
        {
            get => _product;
            set
            {
                if (_product != value)
                {
                    _product = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddProductViewModel()
        {
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private bool CanSave()
        {
            return !IsSaving && string.IsNullOrEmpty(Error);
        }

        private async Task SaveAsync()
        {
            try
            {
                IsSaving = true;
                StatusMessage = "Сохранение...";

                // Валидация через DataAnnotations
                var context = new ValidationContext(Product);
                Validator.ValidateObject(Product, context, validateAllProperties: true);

                int newId = await _repository.AddProductAsync(Product);
                Product.Id = newId;

                StatusMessage = "Товар успешно добавлен";
                OnSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (ValidationException vex)
            {
                StatusMessage = vex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при сохранении: {ex.Message}";
                await Logger.LogAsync($"Ошибка сохранения товара: {ex}", LogLevel.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void Cancel()
        {
            OnCanceled?.Invoke(this, EventArgs.Empty);
        }

        // События для уведомления View о завершении
        public event EventHandler OnSaved;
        public event EventHandler OnCanceled;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region IDataErrorInfo для валидации

        public string Error
        {
            get
            {
                // Проверяем все свойства модели
                var context = new ValidationContext(Product);
                try
                {
                    Validator.ValidateObject(Product, context, true);
                    return null;
                }
                catch (ValidationException ex)
                {
                    return ex.Message;
                }
            }
        }

        public string this[string columnName]
        {
            get
            {
                var prop = Product.GetType().GetProperty(columnName);
                if (prop == null) return null;

                var value = prop.GetValue(Product);
                var context = new ValidationContext(Product) { MemberName = columnName };
                try
                {
                    Validator.ValidateProperty(value, context);
                    return null;
                }
                catch (ValidationException ex)
                {
                    return ex.Message;
                }
            }
        }

        #endregion
    }
}
