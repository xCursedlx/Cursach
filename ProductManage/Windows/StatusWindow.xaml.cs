// StatusWindow.xaml.cs
using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class StatusWindow : Window
    {
        public StatusWindow()
        {
            InitializeComponent();

            DataContext = new StatusViewModel();
        }
    }
}
