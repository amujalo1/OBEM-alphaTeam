using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;
using OBEM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OBEM
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();

        public MainWindow()
        {
            InitializeComponent();
            LoadHighConsumptionData();
        }

        private async void LoadHighConsumptionData()
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                // Filtriranje uređaja s Unit == "Power (kW)"
                var powerDevices = devices.Where(d => d.Unit == "Power (kW)").ToList();

                // Grupiranje po Group1 i Group3 i zbrajanje NumericValue
                var groupedData = powerDevices
                    .GroupBy(d => new { d.Group1, d.Group3 })
                    .Select(g => new
                    {
                        Group1 = g.Key.Group1,
                        Group3 = g.Key.Group3,
                        TotalConsumption = g.Sum(d => d.NumericValue),
                        DeviceNames = string.Join(", ", g.Select(d => d.Group2)) 
                    })
                    .OrderByDescending(g => g.TotalConsumption) // Sortiranje od najveće do najmanje potrošnje
                    .ToList();

                // Postavljanje podataka u DataGrid
                dgHighConsumption.ItemsSource = groupedData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju podataka: " + ex.Message);
            }
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