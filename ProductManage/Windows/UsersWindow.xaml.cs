// UsersWindow.xaml.cs
using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class UsersWindow : Window
    {
        public UsersWindow()
        {
            InitializeComponent();

            DataContext = new UserManagmentViewModel();
        }
    }
}
