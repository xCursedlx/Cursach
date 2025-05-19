using System.Threading.Tasks;
using System.Windows;
using ProductManage.ViewModels;

namespace ProductManage.Windows
{
    public partial class StatusUpdateWindow : Window
    {
        private readonly StatusUpdateViewModel _viewModel;

        public StatusUpdateWindow(int supplyId)
        {
            InitializeComponent();

            _viewModel = new StatusUpdateViewModel(supplyId);
            DataContext = _viewModel;

            Loaded += StatusUpdateWindow_Loaded;
        }

        private async void StatusUpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LoadSupplyCommand.CanExecute(null))
            {
                await Task.Run(() => _viewModel.LoadSupplyCommand.Execute(null));
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
