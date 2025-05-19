using ProductManage.Models;
using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class EditFinancialOperationWindow : Window
    {
        public EditFinancialOperationWindow(FinancialOperation operation, int currentUserId)
        {
            InitializeComponent();
            var viewModel = new EditFinancialOperationViewModel(operation, currentUserId);
            DataContext = viewModel;
            viewModel.RequestClose += (success) => { DialogResult = success; Close(); };
        }
    }
}
