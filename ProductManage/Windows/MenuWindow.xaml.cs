using ProductManage.Models;
using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class MenuWindow : Window
    {
        public MenuWindow(User currentUser)
        {
            InitializeComponent();

            var vm = new MenuViewModel(currentUser);
            DataContext = vm;
        }
    }
}
