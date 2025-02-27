using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBEM.Views
{
    /// <summary>
    /// Interaction logic for EnergyCost.xaml
    /// </summary>
    public partial class EnergyCost : Page
    {
        private readonly ApiService _apiService;
        public EnergyCost()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }
        // Cost for all units in building
        private async void BtnEnergyCost_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                StringBuilder sb = new StringBuilder();
                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        double energyCost = device.NumericValue * pricePerKw;

                        sb.AppendLine($"Unit: {device.Group1}");
                        sb.AppendLine($"Energy Cost: {energyCost}");
                        sb.AppendLine("==========================");

                    }
                }

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }

        // Energy cost per floor
        private async void BtnEnergyCostPerFloor_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                var floorEnergyConsumption = new Dictionary<string, double>();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels")
                    {
                        double energyCost = device.NumericValue * pricePerKw;

                        if (floorEnergyConsumption.ContainsKey(device.Group3))
                        {
                            floorEnergyConsumption[device.Group3] += energyCost;
                        }
                        else
                        {
                            floorEnergyConsumption[device.Group3] = energyCost;
                        }
                    }
                }
                StringBuilder sb = new StringBuilder();

                foreach (var floor in floorEnergyConsumption)
                {
                    sb.AppendLine($"Floor: {floor.Key}");
                    sb.AppendLine($"Total cost: {floor.Value}");
                    sb.AppendLine("===========================");
                }
                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }


        // Total cost in building

        private async void BtnEnergyCostBuilding_Click(object sender, RoutedEventArgs e)
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
                sb.AppendLine($"Energy Cost For Entire Building: {energyCost}");
                sb.AppendLine("===================================");
                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }
        // Total cost in selected floor

        private async void BtnEntireEnergyCostPerFloor_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();
            string floor = (string)(sender as Button).Content;
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
                        if (device.Group3 == floor)
                        {
                            energyCost += device.NumericValue * pricePerKw;
                        }

                    }
                }
                StringBuilder sb = new StringBuilder();


                sb.AppendLine($"Floor: {floor}");
                sb.AppendLine($"Total cost: {energyCost}");
                sb.AppendLine("===========================");

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }

        // Cost of units in selected floor
        private async void BtnUnitEnergyCostPerFloor_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();
            string floor = (string)(sender as Button).Content;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                var floorEnergyConsumption = new Dictionary<string, double>();
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group3 == floor)
                    {
                        double energyCost = device.NumericValue * pricePerKw;

                        sb.AppendLine($"Unit: {device.Group1}");
                        sb.AppendLine($"Energy Cost: {energyCost}");
                        sb.AppendLine("==========================");

                    }
                }


                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }

        // Energy cost of single unit
        private async void BtnUnitEnergyCost_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();
            string unit = (string)(sender as Button).Content;
            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                const double pricePerKw = 0.30;
                var floorEnergyConsumption = new Dictionary<string, double>();
                StringBuilder sb = new StringBuilder();

                foreach (var device in devices)
                {

                    if (device.Unit == "Power (kW)" && device.Group2 != "Solar Panels" && device.Group1 == unit)
                    {
                        double energyCost = device.NumericValue * pricePerKw;

                        sb.AppendLine($"Unit: {device.Group1}");
                        sb.AppendLine($"Energy Cost: {energyCost}");
                        sb.AppendLine("==========================");
                        break;
                    }
                }


                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Greška prilikom parsiranja podataka: {ex.Message}";
            }
        }
    }
}