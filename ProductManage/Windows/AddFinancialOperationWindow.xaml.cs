using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class AddFinancialOperationWindow : Window
    {
        public AddFinancialOperationWindow()
        {
            InitializeComponent();
            this.DataContext = new AddFinancialOperationViewModel();
        }
    }
}
