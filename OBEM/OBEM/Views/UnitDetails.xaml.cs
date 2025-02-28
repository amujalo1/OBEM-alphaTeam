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
using OxyPlot;
using System.Linq;
using OxyPlot.Series;
using System.Windows.Markup;
using System.Windows.Threading;

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
        private DispatcherTimer timer;

        public UnitDetails()
        {
            InitializeComponent();
            StartTimer();
        }
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20);
            Console.WriteLine("Thread zavrsen//////");
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await LoadDevices();
        }
        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await BuildingStats(sender, null);
            
            string data = await _apiService.GetAllDevicesAsync();
            var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

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
                    }
                }

                if (selectedGroup1 == null && selectedGroup3 != null)
                {
                    var groupedDevices = devices
                        .Where(d => d.Group3 == selectedGroup3)
                        .GroupBy(d => d.Group2)
                        .Select(g => new
                        {
                            Id = g.Key,
                            Group2 = g.Key,
                            NumericValue = g.Key == "HVAC System" ? g.Average(d => d.NumericValue) : g.Sum(d => d.NumericValue),
                            Unit = g.First().Unit,
                            IsActive = g.Any(d => d.IsActive)
                        });

                    foreach (var group in groupedDevices)
                    {
                        var devicePanel = CreateDevicePanel(group.Id, group.Group2, group.NumericValue, group.Unit, group.IsActive, showToggleButton: false);
                        DetailsPanel.Children.Add(devicePanel);
                    }

                    var powerUsageData = devices
                        .Where(d => d.Unit == "Power (kW)" && d.Group3 == selectedGroup3)
                        .GroupBy(d => new { d.Group1, d.Group3 })
                        .Select(g => new { Apartment = g.Key.Group1, Category = g.Key.Group3, TotalPower = g.Sum(d => d.NumericValue) })
                        .OrderByDescending(g => g.TotalPower)
                        .ToList();

                    var pieSeries = new PieSeries
                    {
                        InsideLabelPosition = 0.5,
                        StrokeThickness = 2,
                        Stroke = OxyColors.White,
                        AngleSpan = 360,
                        StartAngle = 0,
                        Background = OxyColors.Transparent,
                        OutsideLabelFormat = "{1}: {0}%"
                    };

                    foreach (var item in powerUsageData)
                    {
                        pieSeries.Slices.Add(new PieSlice($"{item.Apartment} ({item.Category})", item.TotalPower)
                        {
                            Fill = OxyColor.FromAColor(180, OxyPalettes.Jet(powerUsageData.Count).Colors[powerUsageData.IndexOf(item)])
                        });
                    }

                    var plotModel = new PlotModel
                    {
                        Title = "Power Consumption by Apartment and Category",
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent
                    };
                    plotModel.Series.Add(pieSeries);

                    var plotView = new OxyPlot.Wpf.PlotView
                    {
                        Model = plotModel,
                        Height = 300,
                        Width = 400,
                        Margin = new System.Windows.Thickness(50),
                        Background = System.Windows.Media.Brushes.Transparent
                    };

                    // Dodavanje PieChart-a u GraphContentControl
                    GraphContentControl.Content = plotView;
                }

                else
                {
                    // If a specific apartment is selected, show individual devices
                    foreach (var device in devices)
                    {
                        if ((selectedGroup3 == null || device.Group3 == selectedGroup3) &&
                            (selectedGroup1 == null || device.Group1 == selectedGroup1))
                        {
                            var devicePanel = CreateDevicePanel(device.Id,device.Group2, device.NumericValue, device.Unit, device.IsActive, showToggleButton: true);
                            DetailsPanel.Children.Add(devicePanel);
                        }
                    }
                }

                // Generate apartment buttons
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

        private Border CreateDevicePanel(string Id,string group2, double numericValue, string unit, bool isActive, bool showToggleButton)
        {
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
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };

            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            var deviceNamePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var powerIcon = new TextBlock
            {
                Text = isActive ? "\u2714" : "\u274C", // Unicode for check or X
                FontSize = 14,
                Foreground = isActive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0) // margin between name and icon
            };

            var nameText = new TextBlock
            {
                Text = group2,
                FontWeight = System.Windows.FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            deviceNamePanel.Children.Add(powerIcon);
            deviceNamePanel.Children.Add(nameText);

            var valueText = new TextBlock
            {
                Text = $"{numericValue} {unit}",
                FontWeight = System.Windows.FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(200, 6, 0, 0) // Add some margin to the right for spacing
            };

            innerGrid.Children.Add(deviceNamePanel);
            innerGrid.Children.Add(valueText);

            // Create toggle button only if showToggleButton is true
            if (showToggleButton)
            {
                var toggleButton = new Button
                {
                    Content = $"Graph:{Id}",
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
                Grid.SetColumn(toggleButton, 1);
                innerGrid.Children.Add(toggleButton);
            }

            devicePanel.Child = innerGrid;

            return devicePanel;
        }

        private async void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var deviceId = button.Content.ToString().Replace("Graph:", "");

            var trendingDataService = new ApiModels();
            var result = await trendingDataService.LoadTrendingDataAsync(deviceId);
            
            if (result.PlotModel != null)
            {
                result.PlotModel.PlotAreaBorderColor = OxyColors.White; 
                result.PlotModel.TextColor = OxyColors.White; 
                result.PlotModel.TitleColor = OxyColors.White; 

                foreach (var axis in result.PlotModel.Axes)
                {
                    axis.AxislineColor = OxyColors.White; 
                    axis.TextColor = OxyColors.White; 
                    axis.TicklineColor = OxyColors.White;
                    axis.ExtraGridlineColor = OxyColors.White;
                }

                var plotView = new OxyPlot.Wpf.PlotView
                {
                    Model = result.PlotModel,
                    Width = 500,
                    Height = 300,
                    Background = Brushes.Transparent, 

                };

                GraphContentControl.Content = plotView;
            }

            

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                MessageBox.Show(result.ErrorMessage);
            }
        }


        private void HideGraphFrame()
        {
            GraphFrame.Visibility = Visibility.Collapsed;
        }
        private async void ApartmentButton_Click(object sender, RoutedEventArgs e)
        {
            txtCurrentOption.Content = "";
            HideGraphFrame();
            var apartmentButton = sender as RadioButton;
            selectedGroup1 = apartmentButton?.Tag as string;
            txtCurrentOption.Content = $"{selectedGroup2} {selectedGroup1}";
            ;
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