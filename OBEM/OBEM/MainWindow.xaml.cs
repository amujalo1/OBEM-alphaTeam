using OBEM.Views;
using System.Windows;
using System.Windows.Controls;

namespace OBEM
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenApiTester_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApiTester());
        }
        private void OpenUnitEnergyMonitoring_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UnitDetails());
        }

        private void OpenEnergyCost_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EnergyCost());
        }

    }
}