// SelectProductWindow.xaml.cs
using System.Windows;
using ProductManage.Models;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class SelectProductWindow : Window
    {
        public SelectProductWindow(bool multiSelect = false)
        {
            InitializeComponent();

            var vm = new SelectProductViewModel(multiSelect);
            DataContext = vm;

            vm.ProductSelected += product =>
            {
                SelectedProduct = product;
                DialogResult = true;
                Close();
            };

            vm.SelectionCompleted += () =>
            {
                DialogResult = true;
                Close();
            };
        }

        public Product SelectedProduct { get; private set; }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
