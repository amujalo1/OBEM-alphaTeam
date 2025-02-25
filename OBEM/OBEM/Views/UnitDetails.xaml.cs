using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using OBEM.Services;
using OBEM.models;

namespace OBEM.Views
{
    public partial class UnitDetails : Page
    {
        private readonly ApiService _apiService = new ApiService();

        private Dictionary<string, string> floorMapping = new Dictionary<string, string>
        {
            { "Floor0", "Floor 0" },
            { "Floor1", "Floor 1" },
            { "Floor2", "Floor 2" },
            { "FloorNeg1", "Floor -1" },
            { "Outside", "Outside" },
            { "General", "General" }
        }; 

        private string selectedGroup1 = null;
        private string selectedGroup2 = null;
        private string selectedGroup3 = null;  // Will hold the selected floor (e.g., "Floor 0")

        public UnitDetails()
        {
            InitializeComponent(); 
        }

        // Single event handler for all floor buttons
        private async void FloorButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the button's name
            string buttonName = (sender as Button)?.Name;

            // Look up the floor name in the dictionary
            if (floorMapping.ContainsKey(buttonName))
            {
                // Get the corresponding floor name with a space
                string floor = floorMapping[buttonName];
                selectedGroup3 = floor;

                // Update the filtered results UI
                txtFilteredResults.Text = $"Showing devices for {floor}...";

                // Load and display filtered devices based on the selected floor
                await LoadDevices();
            }
        }

        private async System.Threading.Tasks.Task LoadDevices()
        {
            // Temporary display message while loading devices
            txtFilteredResults.Text = "Loading devices...";

            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {
                    // Only filter based on the selected floor (Group3)
                    if ((selectedGroup3 == null || device.Group3 == selectedGroup3))
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

                // Display filtered results in the TextBlock
                txtFilteredResults.Text = sb.Length > 0 ? sb.ToString() : "No devices found for the selected floor.";
            }
            catch (Exception ex)
            {
                txtFilteredResults.Text = "Error loading devices: " + ex.Message;
            }
        }
    }
}