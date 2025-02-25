using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;

namespace OBEM
{
    public partial class UnitEnergyMonitoring : Page
    {
        private readonly ApiService _apiService = new ApiService();
        private string selectedGroup1 = null;
        private string selectedGroup2 = null;
        private string selectedGroup3 = null;

        public UnitEnergyMonitoring()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            // Kategorije
            ListBoxCategory1.ItemsSource = new List<string>
            {
                "Apartment 1", "Office B", "Office A", "Apartment 2", "Apartment 3",
                "Apartment 4", "Apartment 5", "Apartment 6", "Apartment 7", "Apartment 8",
                "Apartment 9", "Apartment 10", "Office C", "Office D", "Office E",
                "Elevator A", "Elevator B", "General", "Car Park"
            };

            ListBoxCategory2.ItemsSource = new List<string>
            {
                "HVAC System", "Lighting System", "Appliances", "Solar Panels",
                "Water Consumption", "Electricity", "Generic"
            };

            ListBoxCategory3.ItemsSource = new List<string>
            {
                "Floor 0", "Floor 1", "Floor 2", "Outside",
                "Monitoring", "Floor -1"
            };
        }

        private async void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == ListBoxCategory1)
                selectedGroup1 = ListBoxCategory1.SelectedItem as string;

            if (sender == ListBoxCategory2)
                selectedGroup2 = ListBoxCategory2.SelectedItem as string;

            if (sender == ListBoxCategory3)
                selectedGroup3 = ListBoxCategory3.SelectedItem as string;

            await LoadDevices();
        }

        private async System.Threading.Tasks.Task LoadDevices()
        {
            txtDevicesInfo.Text = "Loading devices...";
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {
                    if ((selectedGroup1 == null || device.Group1 == selectedGroup1) &&
                        (selectedGroup2 == null || device.Group2 == selectedGroup2) &&
                        (selectedGroup3 == null || device.Group3 == selectedGroup3))
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
                }

                txtDevicesInfo.Text = sb.Length > 0 ? sb.ToString() : "No devices found for the selected categories.";
            }
            catch (Exception ex)
            {
                txtDevicesInfo.Text = "Error loading devices: " + ex.Message;
            }
        }
    }
}