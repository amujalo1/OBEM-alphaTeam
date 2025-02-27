using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using OBEM.Services;
using OBEM.models;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using System.Windows.Input;
using System.Windows.Media.Effects;

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
        private string selectedGroup3 = null;
        private string selectedFloor = null;
        public UnitDetails()
        {
            
            InitializeComponent();
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await BuildingStats(sender, null);
        }

        private async void FloorButton_Click(object sender, RoutedEventArgs e)
        {

            arrowLine.Visibility = Visibility.Hidden;
            txtCurrentOptionApartment.Visibility = Visibility.Hidden;
            HideGraphFrame();
            string buttonName = (sender as RadioButton)?.Name;

            if (floorMapping.ContainsKey(buttonName))
            {
                string floor = floorMapping[buttonName];
                selectedGroup3 = floor;
                selectedGroup1 = null; // Reset selectedGroup1 when a new floor is selected
                selectedFloor = floorMapping[buttonName];

                int floorNumber = GetFloorNumber(floor);
                MoveStackPanelBasedOnFloor(floorNumber);

                await LoadDevices();
            }

            // Energy cost logic

            string data = await _apiService.GetAllDevicesAsync();
            string floorLevel = (sender as RadioButton)?.Content.ToString();
            txtCurrentOption.Content = floorLevel;
            double currentEnergyCost = 0;
            double currentCO2 = 0;
            const double pricePerKw = 0.30;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                var floorEnergyConsumption = new Dictionary<string, double>();

                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group3 == floorLevel)
                    {
                        Console.WriteLine($"FloorLevel: {floorLevel}, Device Group3: {device.Group3}");

                        currentEnergyCost += device.NumericValue;
                        
                    }
                }
                double currentPower = currentEnergyCost;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{currentPower}");
                txtTotalRealTimeUsage.Content = sb.ToString();
                sb.Clear();

                currentCO2 = currentEnergyCost * 1.2;
                sb.AppendLine($"{currentCO2} kg CO2");
                txtCurentCarbonFootprint.Content = sb.ToString();
                sb.Clear();

                currentEnergyCost *= pricePerKw;

                sb.AppendLine($"{currentEnergyCost}€");
                txtCurrentEnergyCost.Content = sb.ToString();


            }
            catch (Exception ex)
            {
                txtCurrentEnergyCost.Content = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }

            // Last hour

            double totalEnergyCost = 0;
            double totalCO2 = 0;

            DateTime sevenDaysAgo = DateTime.Now.AddMinutes(-60);
            double sumOfAverages = 0;
            int count = 0;

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group3 == floorLevel)
                    {
                        string trendingData = await _apiService.GetTrendingInfoAsync("average", device.Id, sevenDaysAgo.ToString("MM/dd/yyyy HH:mm"));
                        var trendingResponse = JsonConvert.DeserializeObject<TrendingInfo2>(trendingData);
                        var records = trendingResponse.Records;

                        foreach (var recordEntry in records)
                        {
                            foreach (var record in recordEntry.Value)
                            {
                                sumOfAverages += record.AverageValue;
                                count++;
                                Console.WriteLine($"suma{count}--{sumOfAverages}");
                            }
                        }
                    }
                }

                if (count > 0)
                {
                    double averageValue = sumOfAverages / count;
                    Console.WriteLine($"averageValue - {averageValue}");
                    totalEnergyCost = averageValue * pricePerKw;
                    Console.WriteLine($"totalEnergyCost - {totalEnergyCost}");
                    totalCO2 = averageValue * 1.2;
                    Console.WriteLine($"totalCO2 - {totalCO2}");
                }
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Floor: {floorLevel}");
                sb.AppendLine($"{Math.Round(totalEnergyCost)}€");
                txtEnergyConsumptionLastHour.Content = sb.ToString();
                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCO2)} kg CO2");
                txtCarbonFootprintLastHour.Content = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyConsumptionLastHour.Content = $"Error calculating energy cost: {ex.Message}";
            }
        }


        private async System.Threading.Tasks.Task LoadDevices()
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                HashSet<string> uniqueGroup1Values = new HashSet<string>();
                HashSet<string> uniqueGroup2Values = new HashSet<string>();

                // Clear the panels at the start
                ApartmentButtonsPanel.Children.Clear();
                DetailsPanel.Children.Clear();

                // First pass to collect the unique values for group1 and group2
                foreach (var device in devices)
                {
                    if ((selectedGroup3 == null || device.Group3 == selectedGroup3) &&
                        (selectedGroup1 == null || device.Group1 == selectedGroup1))
                    {
                        uniqueGroup1Values.Add(device.Group1);
                        uniqueGroup2Values.Add(device.Group2);

                        // StackPanel kartica
                        var devicePanel = new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(163, 200, 243)),
                            CornerRadius = new CornerRadius(5),
                            Padding = new Thickness(5), // unutrašnji padding
                            Margin = new Thickness(5), // prostor između kartica
                            Effect = new DropShadowEffect
                            {
                                Color = Colors.Gray,
                                Direction = 320,
                                ShadowDepth = 4,
                                Opacity = 1
                            }
                        };

                        
                        var innerGrid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); 
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); 

                        var deviceNamePanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(0, 0, 0, 0)
                        };

                        var powerIcon = new TextBlock
                        {
                            Text = device.IsActive ? "\u2714" : "\u274C", // Unicode for check or X
                            FontSize = 14,
                            Foreground = device.IsActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red),
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 0) // margin between name and icon
                        };

                        var nameText = new TextBlock
                        {
                            Text = device.Group2,
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(15, 0, 0, 0)
                        };

                        deviceNamePanel.Children.Add(powerIcon);
                        deviceNamePanel.Children.Add(nameText);

                        var valueText = new TextBlock
                        {
                            Text = $"{device.NumericValue} {device.Unit}",
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            Margin = new Thickness(200, 6, 0, 0) // Add some margin to the right for spacing
                        };

                        // Create toggle button
                        var toggleButton = new Button
                        {
                            Content = $"Graph:{device.Id}",
                            Width = 60,
                            Height = 25,
                            Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                            Foreground = new SolidColorBrush(Colors.White),
                            FontSize = 10,
                            Padding = new Thickness(0, 0, 0, 0),
                            Cursor = Cursors.Hand,
                            Margin = new Thickness(-50, 0, 0, 0) // Add margin to the right side for spacing
                        };

                        toggleButton.Click += GraphButton_Click;

                        
                        Grid.SetColumn(deviceNamePanel, 0); 
                        Grid.SetColumn(valueText, 0); 
                        Grid.SetColumn(toggleButton, 1); 

                        innerGrid.Children.Add(deviceNamePanel);
                        innerGrid.Children.Add(valueText);
                        innerGrid.Children.Add(toggleButton);

                        devicePanel.Child = innerGrid;

                        DetailsPanel.Children.Add(devicePanel);
                    }
                }


                // After processing devices, generate the apartment buttons once
                foreach (var group1 in uniqueGroup1Values)
                {
                    var newApartmentButton = new RadioButton
                    {
                        Width = 100,
                        Padding = new Thickness(10, 5, 10, 5),
                        Height = 25,
                        FontSize = 14,
                        Content = group1,
                        Tag = group1
                    };

                    var menuButtonStyle = Application.Current.FindResource("SelectionButtonTheme") as Style;
                    if (menuButtonStyle != null)
                    {
                        newApartmentButton.Style = menuButtonStyle;
                        newApartmentButton.ToolTip = $"Apartment: {group1}";
                    }

                    newApartmentButton.Click += ApartmentButton_Click;
                    ApartmentButtonsPanel.Children.Add(newApartmentButton);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine("Error loading devices: " + ex.Message);
            }
        }

        private void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var deviceId = button.Content.ToString().Replace("Graph:", "");

            // Prikaži Frame
            GraphFrame.Visibility = Visibility.Visible;

            // Navigacija do novog Page-a
            GraphFrame.Navigate(new UnitEnergyMonitoringByDeviceId(deviceId));
        }

        private void HideGraphFrame()
        {
            // Sakrij Frame
            GraphFrame.Visibility = Visibility.Collapsed;
        }
        private async void ApartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HideGraphFrame();
            var apartmentButton = sender as RadioButton;
            selectedGroup1 = apartmentButton?.Tag as string;
            Console.WriteLine(selectedGroup1);
            if (selectedGroup1 != null)
            {
                selectedGroup2 = null;
                await LoadDevices();
            }

            //Energy cost for single unit
            selectedGroup2 = apartmentButton?.Content as string;
            txtCurrentOptionApartment.Visibility = Visibility.Visible;
            txtCurrentOptionApartment.Content = selectedGroup2;
            arrowLine.Visibility = Visibility.Visible;

            string data = await _apiService.GetAllDevicesAsync();
            try
            {
                const double pricePerKw = 0.30;
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == selectedGroup2)
                    {
                        sb.AppendLine($"{device.NumericValue}");
                        txtTotalRealTimeUsage.Content = sb.ToString();
                        sb.Clear();

                        double currentEnergyCost = device.NumericValue * pricePerKw;
                        double currentCO2 = device.NumericValue * 1.2;
                        sb.AppendLine($"{currentEnergyCost}€");
                        txtCurrentEnergyCost.Content = sb.ToString();
                        sb.Clear();
                        sb.AppendLine($"{currentCO2} kg CO2");
                        txtCurentCarbonFootprint.Content = sb.ToString();

                        break;
                    }
                }

                // Last 7 days
                double totalCost = 0;
                double totalCO2 = 0;
                DateTime sevenDaysAgo = DateTime.Now.AddMinutes(-60);
                int count = 0;
                double sumOfAverages = 0;
                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == selectedGroup2)
                    {

                        string trendingData = await _apiService.GetTrendingInfoAsync("average", device.Id, sevenDaysAgo.ToString("MM/dd/yyyy HH:mm"));
                        var trendingResponse = JsonConvert.DeserializeObject<TrendingInfo2>(trendingData);

                        var records = trendingResponse.Records;

                        foreach (var recordEntry in records)
                        {
                            foreach (var record in recordEntry.Value)
                            {
                                DateTime recordTime = DateTime.Parse(record.Time);

                                sumOfAverages += record.AverageValue;
                                Console.WriteLine($"suma{count}--{sumOfAverages}");
                                count++;
                            }
                        }
                    }

                }

                if (count > 0)
                {
                    double averageValue = sumOfAverages / count;
                    Console.WriteLine($"averageValue - {averageValue}");
                    totalCost = averageValue * pricePerKw;
                    Console.WriteLine($"totalCost - {totalCost}");
                    totalCO2 = averageValue * 1.2;
                    Console.WriteLine($"totalCO2 - {totalCO2} kg CO2");
                }

                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCost)}€");
                txtEnergyConsumptionLastHour.Content = sb.ToString();
                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCO2)} kg CO2");
                txtCarbonFootprintLastHour.Content = sb.ToString();

            }
            catch (Exception ex)
            {
                txtEnergyConsumptionLastHour.Content = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }

        }

        // Carbon footprint calculation for entire building
        private async Task BuildingStats(object sender, SelectionChangedEventArgs e)
        {
            txtCurrentOption.Content = "Building";
            arrowLine.Visibility = Visibility.Hidden;
            txtCurrentOptionApartment.Visibility = Visibility.Hidden;

            string data = await _apiService.GetAllDevicesAsync();
            double currentCO2 = 0;
            double currentEnergyCost = 0;
            const double pricePerKw = 0.30;
            // Current values
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        currentEnergyCost += device.NumericValue;
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{currentEnergyCost}");
                txtTotalRealTimeUsage.Content = sb.ToString();
                sb.Clear();


                currentCO2 = currentEnergyCost * 1.2;
                sb.AppendLine($"{currentCO2} kg CO2");
                txtCurentCarbonFootprint.Content = sb.ToString();
                sb.Clear();

                currentEnergyCost *= pricePerKw;
                sb.AppendLine($"{currentEnergyCost}€");
                txtCurrentEnergyCost.Content = sb.ToString();

            }
            catch (Exception ex)
            {
                txtCurentCarbonFootprint.Content = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
            //Works fine
            //Last 7 days
            double totalEnergyCost = 0;
            double totalCO2 = 0;

            DateTime sevenDaysAgo = DateTime.Now.AddMinutes(-60);
            double sumOfAverages = 0;
            int count = 0;


            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        string trendingData = await _apiService.GetTrendingInfoAsync("average", device.Id, sevenDaysAgo.ToString("MM/dd/yyyy HH:mm"));
                        var trendingResponse = JsonConvert.DeserializeObject<TrendingInfo2>(trendingData);
                        var records = trendingResponse.Records;

                        foreach (var recordEntry in records)
                        {
                            foreach (var record in recordEntry.Value)
                            {

                                sumOfAverages += record.AverageValue;
                                count++;
                                Console.WriteLine($"suma{count}--{sumOfAverages}");
                            }
                        }
                    }

                }

                if (count > 0)
                {
                    double averageValue = sumOfAverages / count;
                    Console.WriteLine($"averageValue - {averageValue}");
                    totalEnergyCost = averageValue * pricePerKw;
                    Console.WriteLine($"totalEnergyCost - {totalEnergyCost}");
                    totalCO2 = averageValue * 1.2;
                    Console.WriteLine($"totalCO2 - {totalCO2}");
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{Math.Round(totalEnergyCost)}€");
                txtEnergyConsumptionLastHour.Content = sb.ToString();
                sb.Clear();

                sb.AppendLine($"{Math.Round(totalCO2)} kg CO2");
                txtCarbonFootprintLastHour.Content = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyConsumptionLastHour.Content = $"Error calculating energy cost: {ex.Message}";
            }

        }



        public void MoveStackPanelBasedOnFloor(int floorNumber)
        {
            // pomjeranje margine od početka u zavisnosti od izabira
            // ovo je lakše nego da pravimo više panela
            double marginTop = 0;

            switch (floorNumber)
            {
                case 2:
                    marginTop = 20;
                    break;
                case 1:
                    marginTop = 70;
                    break;
                case 0:
                    marginTop = 120;
                    break;
                case -1:
                    marginTop = 170;
                    break;
                case -4:
                    marginTop = 220;
                    break;
                default:
                    marginTop = 1000; // default
                    break;
            }

            ApartmentButtonsPanel.Margin = new Thickness(0, marginTop, 200, 0);
        }

        private int GetFloorNumber(string floor)
        {
            switch (floor)
            {
                case "Floor 2": return 2;
                case "Floor 1": return 1;
                case "Floor 0": return 0;
                case "Floor -1": return -1;
                case "Outside": return -4; 
                case "General": return -5; 
                default: return 0; 
            }
        }
    }
}