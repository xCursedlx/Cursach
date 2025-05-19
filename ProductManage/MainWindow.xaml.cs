using System.Windows;
using ProductManage.Windows;

namespace ProductManage
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            this.Hide();
            loginWindow.Owner = this;
            bool? result = loginWindow.ShowDialog();

            
        }


        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            this.Hide();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }

    }
}
