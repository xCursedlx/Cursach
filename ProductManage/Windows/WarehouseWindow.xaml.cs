// WarehouseWindow.xaml.cs
using System.Linq;
using System.Windows;
using ProductManage.ViewModels;
using ProductManage.Models;

namespace ProductManage.Windows
{
    public partial class WarehouseWindow : Window
    {
        public WarehouseWindow(User user)
        {
            InitializeComponent();

            DataContext = new WarehouseViewModel(user);
        }
    }
}
