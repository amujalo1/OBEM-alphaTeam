using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using OBEM.Services;
using OBEM.models;
using System.Windows.Media;

namespace OBEM.Views
{
    public partial class UnitDetails : Page
    {
        private readonly ApiService _apiService = new ApiService();

        // dictionary because button names cannot have spaces
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
        private string selectedGroup3 = null;  

        public UnitDetails()
        {
            InitializeComponent();
            
        }

        // same event handler for all floor buttons
        private async void FloorButton_Click(object sender, RoutedEventArgs e)
        {
            

            string buttonName = (sender as Button)?.Name;

            
            if (floorMapping.ContainsKey(buttonName))
            {
                
                string floor = floorMapping[buttonName];
                selectedGroup3 = floor;

                MessageBox.Show($"Loading devices for {floor}");
                txtFilteredResults.Text = $"Showing devices for {floor}...";

                
                await LoadDevices();
            }
        }

        private async System.Threading.Tasks.Task LoadDevices()
        {

            txtFilteredResults.Text = "Loading devices...";

            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                StringBuilder sb = new StringBuilder();

                FloorButtonsPanel.Children.Clear();

                foreach (var device in devices)
                {
                    // filtering logic
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

                        var newApartmentButton = new Button
                        {
                            Width = 100,
                            Height = 50,
                            Content = device.Name,
                            Margin = new Thickness(5),
                            Background = new SolidColorBrush(Colors.LightBlue),
                            Tag = device
                        };

                        newApartmentButton.Click += ApartmentButton_Click; // handler on click


                        FloorButtonsPanel.Children.Add(newApartmentButton);
                    }
                }

                // write results to textblock placeholder
                txtFilteredResults.Text = sb.Length > 0 ? sb.ToString() : "No devices found for the selected floor.";
            }
            catch (Exception ex)
            {
                txtFilteredResults.Text = "Error loading devices: " + ex.Message;
            }
        }

            private async void ApartmentButton_Click(object sender, RoutedEventArgs e)
            {
                
                var apartmentButton = sender as Button;
                var device = apartmentButton?.Tag as DeviceInfo;

                if (device != null)
                {
                        
                        string consumptionDetailsForApartment = $"Device ID: {device.Id}\n" +
                                                    $"Name: {device.Name}\n" +
                                                    $"Lower Bound: {device.LowerBound}\n" +
                                                    $"Upper Bound: {device.UpperBound}\n" +
                                                    $"Current Consumption: {device.NumericValue} {device.Unit}";

                        
                        txtFilteredResults.Text = consumptionDetailsForApartment;
                        await LoadDevices();
            }
            }
    }
}