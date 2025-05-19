using System.Windows;
using System.Windows.Controls;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            var vm = new RegisterViewModel();
            DataContext = vm;

            vm.RequestClose += success =>
            {
                DialogResult = success;
                Close();
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && sender is PasswordBox pb)
            {
                vm.ConfirmPassword = pb.Password;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.CloseRequest(false);
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
