using System.Windows;
using ProductManage.Models;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class NewSupplyWindow : Window
    {
        public NewSupplyWindow(User user)
        {
            InitializeComponent();

            var vm = new NewSupplyViewModel(user);
            DataContext = vm;

            vm.RequestClose += success =>
            {
                DialogResult = success;
                Close();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewSupplyViewModel vm)
            {
                vm.CloseRequest(false);
            }
        }

    }
}
