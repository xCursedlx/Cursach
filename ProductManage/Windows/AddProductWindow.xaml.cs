using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class AddProductWindow : Window
    {
        private readonly AddProductViewModel _viewModel;

        public AddProductWindow()
        {
            InitializeComponent();

            _viewModel = new AddProductViewModel();
            _viewModel.OnSaved += ViewModel_OnSaved;
            _viewModel.OnCanceled += ViewModel_OnCanceled;

            DataContext = _viewModel;
        }

        public ProductManage.Models.Product NewProduct => _viewModel.Product;

        private void ViewModel_OnSaved(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ViewModel_OnCanceled(object sender, System.EventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
