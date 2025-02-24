using OBEM.Services;
using System.Windows;
using OBEM.Services;

namespace YourWpfProject
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;

        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        // Fetch All Devices
        private async void BtnFetchAllDevices_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();
            txtResult.Text = data; // Prikazivanje rezultata u UI
        }

        // Fetch All Categories
        private async void BtnFetchAllCategories_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllCategoriesAsync();
            txtResult.Text = data;
        }

        // Fetch Device by Name
        private async void BtnFetchDeviceByName_Click(object sender, RoutedEventArgs e)
        {
            string deviceName = txtDeviceName.Text; // Uzima ime uređaja iz textbox-a
            string data = await _apiService.GetDeviceByNameAsync(deviceName);
            txtResult.Text = data;
        }

        // Fetch Device by Category
        private async void BtnFetchDeviceByCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = txtCategoryName.Text; 
            string data = await _apiService.GetDeviceByCategoryAsync(categoryName);
            txtResult.Text = data;
        }
    }
}
