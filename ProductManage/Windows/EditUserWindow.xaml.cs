using ProductManage.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProductManage.Windows
{
    public partial class EditUserWindow : Window
    {
        private bool _isUpdatingPassword;

        public EditUserWindow(int userId)
        {
            InitializeComponent();
            DataContext = new EditUserViewModel(userId);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;

            if (DataContext is EditUserViewModel vm)
            {
                vm.Password = PasswordBox.Password;
                PasswordTextBox.Text = PasswordBox.Password;
            }

            _isUpdatingPassword = false;
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;

            if (!_isUpdatingPassword)
            {
                _isUpdatingPassword = true;
                PasswordBox.Password = PasswordTextBox.Text;
                _isUpdatingPassword = false;
            }
        }
    }
}
