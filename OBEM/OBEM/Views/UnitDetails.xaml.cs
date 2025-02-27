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
            await BuildingCarbonFootprint(sender, null);
            await EnergyCostBuilding(sender, null);
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
            double energyCost = 0;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                var floorEnergyConsumption = new Dictionary<string, double>();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        if (device.Group3 == floorLevel)
                        {
                            energyCost += device.NumericValue * pricePerKw;
                        }

                    }
                }
                StringBuilder sb = new StringBuilder();


                sb.AppendLine($"Floor: {floorLevel}");
                sb.AppendLine($"Total cost: {energyCost}$");

                txtEnergyCost.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }


            // Carbon footprint calculation
            double CO2 = 0;

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                StringBuilder sb = new StringBuilder();
                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group3==floorLevel)
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
                            Content = $"Graph:{device.Id}",
                            Width = 100,
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 5)
                        };

                        toggleButton.Click += GraphButton_Click; // Dodajte ovu liniju

                        devicePanel.Children.Add(group2Text);
                        devicePanel.Children.Add(name);
                        devicePanel.Children.Add(isOnText);
                        devicePanel.Children.Add(numericValueText);
                        devicePanel.Children.Add(toggleButton);

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
            //string unit = (string)(sender as Button).Content;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                var floorEnergyConsumption = new Dictionary<string, double>();
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == selectedGroup2)
                    {
                        double energyCost = device.NumericValue * pricePerKw;

                        sb.AppendLine($"Unit: {device.Group1}");
                        sb.AppendLine($"Energy Cost: {energyCost}");
                        break;
                    }
                }


                txtEnergyCost.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtEnergyCost.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }



            // CO2 footprint 

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == selectedGroup2)
                    {
                        double CO2 = device.NumericValue * 1.2;

                        sb.AppendLine($"{CO2} kg CO2");
                        break;
                    }
                }


                txtCarbonFootprint.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtCarbonFootprint.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
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