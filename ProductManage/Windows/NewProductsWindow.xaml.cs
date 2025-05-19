// NewProductsWindow.xaml.cs
using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class NewProductsWindow : Window
    {
        public NewProductsWindow()
        {
            InitializeComponent();

            var vm = new NewProductsViewModel();
            DataContext = vm;

            vm.RequestClose += result =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
