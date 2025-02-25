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
using System.Windows.Shapes;

namespace OBEM
{
    public partial class CostBreakdownWindow : Window
    {
        private readonly ApiService _apiService;

        public CostBreakdownWindow()
        {
            InitializeComponent();
            _apiService = new ApiService(); // Ako koristiš dependency injection, možeš ovo zamijeniti drugim načinom instanciranja
        }

        private async void BtnFetchDevices_Click(object sender, RoutedEventArgs e)
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                StringBuilder sb = new StringBuilder();
                foreach (var device in devices)
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

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Error parsing data: {ex.Message}";
            }
        }
    }
}