
using ProductManage.Models;
using ProductManage.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ProductManage.Windows
{
    public partial class LoginWindow : Window
    {
        public User AuthenticatedUser { get; private set; }
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            DataContext = vm;

            vm.RequestClose += success =>
            {
                DialogResult = success;

                if (success)
                {
                    AuthenticatedUser = vm.AuthenticatedUser;
                    var menuWindow = new MenuWindow(AuthenticatedUser);
                    menuWindow.Show();
                }

                this.Close();
            };

        }


        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
    }
}
