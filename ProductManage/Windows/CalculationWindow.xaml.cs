using ProductManage.ViewModels;
using System.Windows;

namespace ProductManage.Windows
{
    /// <summary>
    /// Логика взаимодействия для CalculationWindow.xaml
    /// </summary>
    public partial class CalculationWindow : Window
    {
        public CalculationWindow()
        {
            InitializeComponent();
            DataContext = new CalculationViewModel();
        }
    }
}
