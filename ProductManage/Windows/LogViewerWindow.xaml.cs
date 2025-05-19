using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class LogViewerWindow : Window
    {
        public LogViewerWindow()
        {
            InitializeComponent();
            DataContext = new LogViewerViewModel();
        }
    }
}
