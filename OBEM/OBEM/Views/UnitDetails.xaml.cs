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

        private Dictionary<string, string> floorMapping = new Dictionary<string, string>
        {
            { "Floor0", "Floor 0" },
            { "Floor1", "Floor 1" },
            { "Floor2", "Floor 2" },
            { "FloorNeg1", "Floor -1" },
            { "Outside", "Outside" },
            { "General", "General" }
        };

        private string selectedFloor = null;

        public UnitDetails()
        {
            InitializeComponent();
        }

        private async void FloorButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonName = (sender as RadioButton)?.Name;

            if (floorMapping.ContainsKey(buttonName))
            {
                selectedFloor = floorMapping[buttonName];
                await LoadDevices();
            }
        }

        private async System.Threading.Tasks.Task LoadDevices()
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                DetailsPanel.Children.Clear();

                foreach (var device in devices)
                {
                    if (selectedFloor == null || device.Group3 == selectedFloor)
                    {
                        var devicePanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Margin = new Thickness(0, 0, 0, 10),
                            Background = new SolidColorBrush(Colors.White),
                            //Padding = new Thickness(10),
                            //CornerRadius = new CornerRadius(5)
                        };

                        var group2Text = new TextBlock
                        {
                            Text = $"Group2: {device.Group2}",
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };

                        var name = new TextBlock
                        {
                            Text = $"name: {device.Name}",
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };

                        var isOnText = new TextBlock
                        {
                            Text = $"Is On: {device.IsActive}",
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        
                        var numericValueText = new TextBlock
                        {
                            Text = $"Numeric Value: {device.NumericValue} {device.Unit}",
                            Margin = new Thickness(0, 0, 0, 5)
                        };

                        var toggleButton = new Button
                        {
                            Content = "Toggle",
                            Width = 100,
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        
                        devicePanel.Children.Add(group2Text);
                        devicePanel.Children.Add(name);
                        devicePanel.Children.Add(isOnText);
                        devicePanel.Children.Add(numericValueText);
                        devicePanel.Children.Add(toggleButton);

                        DetailsPanel.Children.Add(devicePanel);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine("Error loading devices: " + ex.Message);
            }
        }
    }
}