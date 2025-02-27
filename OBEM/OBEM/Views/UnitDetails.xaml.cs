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
                currentCO2 = currentEnergyCost * 1.2;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{currentCO2} kg CO2");
                txtCurentCarbonFootprint.Content = sb.ToString();
                sb.Clear();

                currentEnergyCost *= pricePerKw;

                sb.AppendLine($"Floor: {floorLevel}");
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

                ApartmentButtonsPanel.Children.Clear();
                DetailsPanel.Children.Clear();

                foreach (var device in devices)
                {
                    if ((selectedGroup3 == null || device.Group3 == selectedGroup3) &&
                        (selectedGroup1 == null || device.Group1 == selectedGroup1))
                    {
                        uniqueGroup1Values.Add(device.Group1);
                        uniqueGroup2Values.Add(device.Group2);

                        // Kreirajte StackPanel za karticu
                        var devicePanel = new Border // Koristimo Border za zaobljene uglove
                        {
                            Background = new SolidColorBrush(Colors.White), // Početna boja pozadine
                            CornerRadius = new CornerRadius(10), // Zaobljeni uglovi
                            Padding = new Thickness(10), // Unutarnji razmak (padding)
                            Margin = new Thickness(5), // Vanjski razmak (margin) između kartica
                            Effect = new DropShadowEffect // Dodajte sjenu za moderniji izgled
                            {
                                Color = Colors.Gray,
                                Direction = 320,
                                ShadowDepth = 5,
                                Opacity = 0.5
                            }
                        };

                        // Unutarnji StackPanel za organizaciju elemenata
                        var innerStackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical
                        };

                        // Dodajte TextBlock za Group2
                        var group2Text = new TextBlock
                        {
                            Text = $"Group2: {device.Group2}",
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 5), // Razmak ispod teksta
                            FontSize = 14 // Veličina fonta
                        };

                        // Dodajte TextBlock za Name
                        var name = new TextBlock
                        {
                            Text = $"Name: {device.Name}",
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 5), // Razmak ispod teksta
                            FontSize = 14
                        };

                        // Dodajte TextBlock za IsActive status
                        var isOnText = new TextBlock
                        {
                            Text = device.IsActive ? "Status: Activated" : "Status: Deactivated", // Promijenite tekst ovisno o statusu
                            Margin = new Thickness(0, 0, 0, 5), // Razmak ispod teksta
                            FontSize = 14,
                            Foreground = device.IsActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red) // Boja teksta ovisno o statusu
                        };

                        // Dodajte TextBlock za Numeric Value
                        var numericValueText = new TextBlock
                        {
                            Text = $"Numeric Value: {device.NumericValue} {device.Unit}",
                            Margin = new Thickness(0, 0, 0, 5), // Razmak ispod teksta
                            FontSize = 14
                        };

                        // Dodajte Button za prikaz grafa
                        var toggleButton = new Button
                        {
                            Content = $"Graph: {device.Id}",
                            Width = 100,
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 5), // Razmak ispod dugmeta
                            Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Plava boja pozadine
                            Foreground = new SolidColorBrush(Colors.White), // Bijeli tekst
                            FontSize = 12,
                            Padding = new Thickness(5),
                            Cursor = Cursors.Hand // Promijeni kursor u ruku
                        };

                        toggleButton.Click += GraphButton_Click; // Dodajte event handler

                        // Ako je uređaj aktivan, promijenite boju pozadine kartice
                        if (device.IsActive)
                        {
                            devicePanel.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // Svijetlo plava boja pozadine
                        }

                        // Dodajte sve elemente u unutarnji StackPanel
                        innerStackPanel.Children.Add(group2Text);
                        innerStackPanel.Children.Add(name);
                        innerStackPanel.Children.Add(isOnText);
                        innerStackPanel.Children.Add(numericValueText);
                        innerStackPanel.Children.Add(toggleButton);

                        // Dodajte unutarnji StackPanel u Border (karticu)
                        devicePanel.Child = innerStackPanel;



                        DetailsPanel.Children.Add(devicePanel);
                    }
                }

                foreach (var group1 in uniqueGroup1Values)
                {
                    var newApartmentButton = new RadioButton
                    {
                        Width = 75,
                        Padding = new Thickness(10, 5, 10, 5),
                        Height = 25,
                        FontSize = 10,
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
                        double currentEnergyCost = device.NumericValue * pricePerKw;
                        double currentCO2 = device.NumericValue * 1.2;
                        sb.AppendLine($"Unit: {device.Group1}");
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

                currentCO2 = currentEnergyCost * 1.2;
                StringBuilder sb = new StringBuilder();
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