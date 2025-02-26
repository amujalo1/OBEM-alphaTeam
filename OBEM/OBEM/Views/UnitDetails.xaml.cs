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

        public UnitDetails()
        {
            InitializeComponent();

        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await BuildingCarbonFootprint(sender, null);
            await EnergyCostBuilding(sender, null);
        }

        private async void FloorButton_Click(object sender, RoutedEventArgs e)
        {

            //Current 
            string buttonName = (sender as RadioButton)?.Name;

            if (floorMapping.ContainsKey(buttonName))
            {
                string floor = floorMapping[buttonName];
                selectedGroup3 = floor;
                selectedGroup1 = null; // Reset selectedGroup1 when a new floor is selected
                txtDetails.Text = $"Showing devices for {floor}...";

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
                            currentEnergyCost += device.NumericValue;
                    }
                }
                currentCO2 = currentEnergyCost * 1.2;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{currentCO2} kg CO2");
                txtCurentCarbonFootprint.Text = sb.ToString();
                sb.Clear();

                currentEnergyCost *= pricePerKw;

                sb.AppendLine($"Floor: {floorLevel}");
                sb.AppendLine($"{currentEnergyCost}$");
                txtCurrentEnergyCost.Text = sb.ToString();
                

            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }

            // Last 7 days

            double totalEnergyCost = 0;
            double totalCO2 = 0;

            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            double sumOfAverages = 0;
            int count = 0;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                foreach(var device in devices)
                {
                    if(device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group3 == floorLevel)
                    {
                        string trendingData = await _apiService.GetTrendingInfoById2(int.Parse(device.Id));
                        var trendingResponse = JsonConvert.DeserializeObject<TrendingInfo2>(trendingData);
                        var records = trendingResponse.Records;

                        foreach(var recordEntry in records)
                        {
                            foreach(var record in recordEntry.Value)
                            {
                                DateTime recordTime = DateTime.Parse(record.Time);


                                if(recordTime > sevenDaysAgo)
                                {
                                    sumOfAverages += record.AverageValue;
                                    count++;
                                }
                            }
                        }
                        if(count > 0)
                        {
                            double averageValue = sumOfAverages / count;
                            totalEnergyCost = averageValue + pricePerKw;
                            totalCO2 += averageValue * 1.2;
                        }
                    }
                }
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Floor: {floorLevel}");
                sb.AppendLine($"{Math.Round(totalEnergyCost)}$");
                txtEnergyCost.Text = sb.ToString();
                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCO2)} kg CO2");
                txtCarbonFootprint.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Error calculating energy cost: {ex.Message}";
                txtCarbonFootprint.Text = $"Error calculating carbon footprint: {ex.Message}";
            }


        }

        private async System.Threading.Tasks.Task LoadDevices()
        {
            txtDetails.Text = "Loading devices...";

            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                HashSet<string> uniqueGroup1Values = new HashSet<string>();
                HashSet<string> uniqueGroup2Values = new HashSet<string>();

                ApartmentButtonsPanel.Children.Clear();
                lstGroup2.Items.Clear();

                foreach (var device in devices)
                {
                    if ((selectedGroup3 == null || device.Group3 == selectedGroup3) &&
                        (selectedGroup1 == null || device.Group1 == selectedGroup1))
                    {
                        uniqueGroup1Values.Add(device.Group1);
                        uniqueGroup2Values.Add(device.Group2);
                    }
                }

                foreach (var group1 in uniqueGroup1Values)
                {
                    var newApartmentButton = new RadioButton
                    {
                        Width = 75,
                        Height = 25,
                        Content = group1,
                        Margin = new Thickness(),
                        Background = new SolidColorBrush(Colors.LightBlue),
                        Tag = group1,
                    };

                    var menuButtonStyle = Application.Current.FindResource("MenuButtonTheme") as Style;
                    if (menuButtonStyle != null)
                    {
                        newApartmentButton.Style = menuButtonStyle;
                    }
                    newApartmentButton.Click += ApartmentButton_Click;
                    ApartmentButtonsPanel.Children.Add(newApartmentButton);

                    
                }

                foreach (var group2 in uniqueGroup2Values)
                {
                    lstGroup2.Items.Add(group2);
                }

                txtDetails.Text = uniqueGroup2Values.Count > 0 ? "Select a Group2 to see details." : "No devices found for the selected floor and group.";
            }
            catch (Exception ex)
            {
                txtDetails.Text = "Error loading devices: " + ex.Message;
            }
        }

        private async void ApartmentButton_Click(object sender, RoutedEventArgs e)
        {
            var apartmentButton = sender as RadioButton;
            selectedGroup1 = apartmentButton?.Tag as string;

            if (selectedGroup1 != null)
            {
                txtDetails.Text = $"Showing devices for {selectedGroup1}...";
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
                        sb.AppendLine($"Energy Cost: {currentEnergyCost}");
                        txtCurrentEnergyCost.Text = sb.ToString();
                        sb.Clear();
                        sb.AppendLine($"{currentCO2} kg CO2");
                        txtCurentCarbonFootprint.Text = sb.ToString();

                        break;
                    }
                }
                
                // Last 7 days
                double totalCost = 0;
                double totalCO2 = 0;
                var floorEnergyConsumption = new Dictionary<string, double>();
                DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == selectedGroup2)
                    {

                        string trendingData = await _apiService.GetTrendingInfoById2(int.Parse(device.Id));
                        var trendingResponse = JsonConvert.DeserializeObject<TrendingInfo2>(trendingData);


                        var records = trendingResponse.Records;

                        double sumOfAverages = 0;
                        int count = 0;

                        foreach(var recordEntry in records)
                        {
                            foreach(var record in recordEntry.Value)
                            {
                                DateTime recordTime = DateTime.Parse(record.Time);
                                
                                if(recordTime > sevenDaysAgo)
                                {
                                    sumOfAverages += record.AverageValue;
                                    count++;
                                }
                            }
                        }

                        if(count > 0)
                        {
                            double average = sumOfAverages / count;
                            double energyCost = average * pricePerKw;
                            double CO2 = average * 1.2;

                            totalCost += energyCost;
                            totalCO2 += CO2;
                        }
                    }

                }

                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCost)}$");
                txtEnergyCost.Text = sb.ToString();
                sb.Clear();
                sb.AppendLine($"{Math.Round(totalCO2)} kg CO2");
                txtCarbonFootprint.Text = sb.ToString();

            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }

        }

        private async void Group2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedGroup2 = lstGroup2.SelectedItem as string;

            if (selectedGroup2 != null)
            {
                txtDetails.Text = $"Loading details for {selectedGroup2}...";

                string data = await _apiService.GetAllDevicesAsync();
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {
                    if (device.Group2 == selectedGroup2 &&
                        (selectedGroup1 == null || device.Group1 == selectedGroup1) &&
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

                txtDetails.Text = sb.Length > 0 ? sb.ToString() : "No details found for the selected Group2.";
            }
        }

        // Carbon footprint calculation for entire building
        private async Task BuildingCarbonFootprint(object sender, SelectionChangedEventArgs e)
        {

            string data = await _apiService.GetAllDevicesAsync();
            double CO2 = 0;

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                StringBuilder sb = new StringBuilder();
                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        CO2 += device.NumericValue * 1.2;

                    }
                }
                sb.AppendLine($"{CO2} kg CO2");
                txtCarbonFootprint.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtCarbonFootprint.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }

        }


        // Cost calculation for entire building

        private async Task EnergyCostBuilding(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                StringBuilder sb = new StringBuilder();
                double energyCost = 0;
                foreach (var device in devices)
                {
                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        energyCost += device.NumericValue * pricePerKw;
                    }
                }
                sb.AppendLine($"Energy Cost For Entire Building: {energyCost}$");
                txtEnergyCost.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }

        
        public void MoveStackPanelBasedOnFloor(int floorNumber)
        {
            // Define the margin positions based on the selected floor
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
                case -2:
                    marginTop = 220;
                    break;
                case -3:
                    marginTop = 270;
                    break;
                default:
                    marginTop = 0; // Default margin in case of invalid floor
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
                case "Floor -2": return -2;
                case "Floor -3": return -3;
                case "Outside": return -4; 
                case "General": return -5; 
                default: return 0; 
            }
        }
    }
}