using ProductManage.Models;
using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(User currentUser)
        {
            InitializeComponent();
            DataContext = new ProfileViewModel(currentUser);
        }
    }
}
