using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    public partial class SelectUserWindow : Window
    {
        public SelectUserWindow()
        {
            InitializeComponent();
            DataContext = new SelectUserViewModel();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
