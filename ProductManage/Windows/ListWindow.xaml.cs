using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class ListWindow : Window
    {
        public ListWindow()
        {
            InitializeComponent();
            DataContext = new ListViewModel();
        }
    }
}
