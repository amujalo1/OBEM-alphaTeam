using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace OBEM
{
    public partial class Notifikacije : Page
    {
        public class Device
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int NumericValue { get; set; }
            public int UpperBound { get; set; }
            public string Group2 { get; set; }
            public string Unit { get; set; }
            public bool IsActive { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        public Notifikacije()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private async void LoadNotifications()
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer zC3GtRfOFKY9kKI7CSJo6ZxSW33fT/f1NVQ9Lr0s0gk=");

                string apiUrl = "https://slb-skyline.on.dataminer.services/api/custom/OptimizingBuildingEnergyManagement/getAllDevices";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    var devices = JsonConvert.DeserializeObject<List<Device>>(jsonResponse);

                    var solarPanelDevices = devices.Where(device => device.Group2 == "Solar Panels").ToList();
                    var turnedOffDevices = devices.Where(device => !device.IsActive).ToList();
                    var peakConsumptionDevices = devices.Where(device => device.NumericValue >= 0.8 * device.UpperBound).ToList();
                    var exceededThresholdDevices = devices.Where(device => device.NumericValue > device.UpperBound).ToList();

                    NotificationsStackPanel.Children.Clear();

                    DisplaySolarPanelDevices(solarPanelDevices);

                    DisplayTurnedOffDevices(turnedOffDevices);

                    DisplayPeakConsumptionDevices(peakConsumptionDevices);

                    DisplayExceededThresholdDevices(exceededThresholdDevices);
                }
                else
                {
                    MessageBox.Show("Failed to retrieve data from the API.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching data: {ex.Message}");
            }
        }



        private void DisplaySolarPanelDevices(List<Device> solarPanelDevices)
        {
            var titleTextBlock = new TextBlock
            {
                Text = "Solar Panel Devices",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(10)
            };
            NotificationsStackPanel.Children.Add(titleTextBlock);

            if (solarPanelDevices.Count == 0)
            {
                var noSolarPanelsTextBlock = new TextBlock
                {
                    Text = "No Solar Panels found.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new System.Windows.Thickness(10)
                };
                NotificationsStackPanel.Children.Add(noSolarPanelsTextBlock);
            }
            else
            {
                foreach (var solarPanel in solarPanelDevices)
                {
                    int numericValue = solarPanel.NumericValue;

                    Console.WriteLine($"Device ID: {solarPanel.Id}, Numeric Value: {numericValue}");

                    var solarPanelBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.Green, 
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new System.Windows.Thickness(1),
                        Padding = new System.Windows.Thickness(10),
                        Margin = new System.Windows.Thickness(5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"Device ID: {solarPanel.Id}\nNumeric Value: {numericValue} {solarPanel.Unit}",
                        Foreground = System.Windows.Media.Brushes.White, 
                        FontSize = 14
                    };

                    solarPanelBorder.Child = textBlock;

                    NotificationsStackPanel.Children.Add(solarPanelBorder);
                }
            }
        }





        private void DisplayTurnedOffDevices(List<Device> turnedOffDevices)
        {
            var titleTextBlock = new TextBlock
            {
                Text = "Devices Turned Off",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(10)
            };
            NotificationsStackPanel.Children.Add(titleTextBlock);

            if (turnedOffDevices.Count == 0)
            {
                var noTurnedOffDevicesTextBlock = new TextBlock
                {
                    Text = "No devices found that are turned off currently.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new System.Windows.Thickness(10)
                };
                NotificationsStackPanel.Children.Add(noTurnedOffDevicesTextBlock);
            }
            else
            {
                foreach (var device in turnedOffDevices)
                {
                    var deviceBorder = CreateNotificationBorder(
                        device.Name, 
                        $"Device ID: {device.Id}\nNumeric Value: {device.NumericValue} {device.Unit}",
                        string.Empty);

                    NotificationsStackPanel.Children.Add(deviceBorder);
                }
            }
        }



        private void DisplayPeakConsumptionDevices(List<Device> peakConsumptionDevices)
        {
            var titleTextBlock = new TextBlock
            {
                Text = "Devices with Peak Consumption",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(10)
            };
            NotificationsStackPanel.Children.Add(titleTextBlock);

            if (peakConsumptionDevices.Count == 0)
            {
                var noPeakConsumptionDevicesTextBlock = new TextBlock
                {
                    Text = "No devices with peak consumption detected.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new System.Windows.Thickness(10)
                };
                NotificationsStackPanel.Children.Add(noPeakConsumptionDevicesTextBlock);
            }
            else
            {
                foreach (var device in peakConsumptionDevices)
                {
                    bool isPeakConsumption = device.NumericValue >= (device.UpperBound * 0.8);

                    var borderColor = isPeakConsumption
                        ? System.Windows.Media.Brushes.Orange
                        : System.Windows.Media.Brushes.Transparent;

                    var deviceBorder = new Border
                    {
                        Background = borderColor,
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new System.Windows.Thickness(1),
                        Padding = new System.Windows.Thickness(10),
                        Margin = new System.Windows.Thickness(5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"Device ID: {device.Id}\nNumeric Value: {device.NumericValue} {device.Unit}\nUpper Bound: {device.UpperBound}",
                        Foreground = System.Windows.Media.Brushes.White, 
                        FontSize = 14
                    };

                    deviceBorder.Child = textBlock;

                    NotificationsStackPanel.Children.Add(deviceBorder);
                }
            }
        }





        private void DisplayExceededThresholdDevices(List<Device> exceededThresholdDevices)
        {
            var titleTextBlock = new TextBlock
            {
                Text = "Devices Exceeding Threshold",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(10)
            };
            NotificationsStackPanel.Children.Add(titleTextBlock);

            if (exceededThresholdDevices.Count == 0)
            {
                var noExceededThresholdDevicesTextBlock = new TextBlock
                {
                    Text = "No devices with exceeded threshold detected.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new System.Windows.Thickness(10)
                };
                NotificationsStackPanel.Children.Add(noExceededThresholdDevicesTextBlock);
            }
            else
            {
                foreach (var device in exceededThresholdDevices)
                {
                    var deviceBorder = new Border
                    {
                        Background = System.Windows.Media.Brushes.Red,
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new System.Windows.Thickness(1),
                        Padding = new System.Windows.Thickness(10),
                        Margin = new System.Windows.Thickness(5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"Device ID: {device.Id}\nNumeric Value: {device.NumericValue} {device.Unit}\nUpper Bound: {device.UpperBound}",
                        Foreground = System.Windows.Media.Brushes.White, 
                        FontSize = 14
                    };

                    deviceBorder.Child = textBlock;

                    NotificationsStackPanel.Children.Add(deviceBorder);
                }
            }
        }




        private Border CreateNotificationBorder(string title, string details, string timestamp)
        {
            var notificationBorder = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new System.Windows.CornerRadius(5),
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new System.Windows.Thickness(1),
                Margin = new System.Windows.Thickness(0, 0, 0, 10),
                Padding = new System.Windows.Thickness(10)
            };

            var stackPanel = new StackPanel();
            notificationBorder.Child = stackPanel;

            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.Green
            };
            stackPanel.Children.Add(titleTextBlock);

            var detailsTextBlock = new TextBlock
            {
                Text = details,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new System.Windows.Thickness(0, 5, 0, 0)
            };
            stackPanel.Children.Add(detailsTextBlock);

            if (!string.IsNullOrEmpty(timestamp))
            {
                var timestampTextBlock = new TextBlock
                {
                    Text = timestamp,
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    Margin = new System.Windows.Thickness(0, 5, 0, 0)
                };
                stackPanel.Children.Add(timestampTextBlock);
            }

            return notificationBorder;
        }
    }
}
