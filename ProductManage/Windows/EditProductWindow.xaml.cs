using ProductManage.Models;
using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class EditProductWindow : Window
    {
        private readonly EditProductViewModel _viewModel;

        public EditProductWindow(Product product)
        {
            InitializeComponent();

            _viewModel = new EditProductViewModel(product);
            _viewModel.OnSaved += ViewModel_OnSaved;
            _viewModel.OnCanceled += ViewModel_OnCanceled;

            DataContext = _viewModel;
        }

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
