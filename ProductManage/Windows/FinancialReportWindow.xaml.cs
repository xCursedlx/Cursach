// FinancialReportWindow.xaml.cs
using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class FinancialReportWindow : Window
    {
        public FinancialReportWindow(int cur)
        {
            InitializeComponent();
            DataContext = new FinancialReportViewModel(cur);
        }
    }
}
