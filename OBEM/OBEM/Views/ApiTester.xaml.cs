using OBEM.Services;
using System.Windows;
using OBEM.Services;
using Newtonsoft.Json;
using OBEM.models;
using System.Collections.Generic;
using System.Text;
using System;
using OBEM.models;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace OBEM
{
    public partial class ApiTester : Page
    {
        private readonly ApiService _apiService;

        public ApiTester()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        // Fetch All Devices
        private async void BtnFetchAllDevices_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();

            // Parsiranje JSON podataka u listu objekata DeviceInfo
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                StringBuilder sb = new StringBuilder();
                foreach (var device in devices)
                {
                    sb.AppendLine($"ID: {device.Id}");
                    sb.AppendLine($"Name: {device.Name}");
                    sb.AppendLine($"Lower Bound: {device.LowerBound}");
                    sb.AppendLine($"Upper Bound: {device.UpperBound}");
                    sb.AppendLine($"Numeric Value: {device.NumericValue}");
                    sb.AppendLine($"String Value: {device.StringValue}");
                    sb.AppendLine($"Unit: {device.Unit}");
                    sb.AppendLine($"Simulation Type: {device.SimulationType}");
                    sb.AppendLine($"Growth Ratio: {device.GrowthRatio}");
                    sb.AppendLine($"Group1: {device.Group1}");
                    sb.AppendLine($"Group2: {device.Group2}");
                    sb.AppendLine($"Group3: {device.Group3}");
                    sb.AppendLine($"Is Active: {device.IsActive}");
                    sb.AppendLine($"Update Interval: {device.UpdateInterval}");
                    sb.AppendLine("===============================================");
                }

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }


        // Fetch All Categories
        private async void BtnFetchAllCategories_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllCategoriesAsync();
            // Parsiranje JSON podataka u listu objekata DeviceInfo
            try
            {
                var kategorije = JsonConvert.DeserializeObject<List<CategoryInfo>>(data);

                StringBuilder sb = new StringBuilder();
                foreach (var kategorija in kategorije)
                {
                    sb.AppendLine($"categoryNumber: {kategorija.CategoryNumber}");
                    foreach (var naziv in kategorija.CategoryNames)
                    {
                        sb.AppendLine($"categoryName: {naziv}");
                    }
                    sb.AppendLine("===============================================");
                }

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
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


        // Fetch Trending Info
        private async void BtnFetchTrendingInfo_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetTrendingInfo();

            txtResult.Text = data;
        }

        // Fetch Trending Info By Id
        private async void BtnFetchTrendingInfoById_Click(object sender, RoutedEventArgs e)
        {
            int id = int.TryParse(txtTrendingId.Text, out int result) ? result : 0;
            if (id == 0)
            {
                txtResult.Text = "Molimo unesite važeći ID.";
                return;
            }

            string data = await _apiService.GetTrendingInfoById(id);
            txtResult.Text = data;
        }

    }
}