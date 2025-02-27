using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using OBEM.models;
using OBEM.Services;

namespace OBEM
{
    public partial class Last24HoursEnergy : Page
    {
        public List<DeviceData> DeviceDataList { get; set; }

        private readonly ApiService _apiService = new ApiService();

        public Last24HoursEnergy()
        {
            InitializeComponent();
            DeviceDataList = new List<DeviceData>();
        }




        public Last24HoursEnergy(string id)
        {
            InitializeComponent();
            DeviceDataList = new List<DeviceData>();
            DeviceIdTextBox.Text = id;
            string deviceId = DeviceIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(deviceId))
            {
                MessageBox.Show("Please enter a valid device ID.");
                return;
            }
            _ = LoadTrendingDataAsync(deviceId);
            _ = LoadDeviceInfoAsync(deviceId);
        }




        private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = DeviceIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(deviceId))
            {
                MessageBox.Show("Please enter a valid device ID.");
                return;
            }

            await LoadTrendingDataAsync(deviceId);
            await LoadDeviceInfoAsync(deviceId);
        }




        private async Task LoadTrendingDataAsync(string deviceId)
        {
            var trendingDataService = new ApiModels();
            var result = await trendingDataService.LoadTrendingDataAsync(deviceId);

            if (result.PlotModel != null)
            {
                result.PlotModel = FilterDataForLast24Hours(result.PlotModel);
                EnergyPlot.Model = result.PlotModel;
            }

            if (!string.IsNullOrEmpty(result.MinMaxAvgText))
            {
                MinMaxAvgTextBlock.Text = result.MinMaxAvgText;
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                MessageBox.Show(result.ErrorMessage);
            }
        }




        private PlotModel FilterDataForLast24Hours(PlotModel plotModel)
        {
            var filteredPlotModel = new PlotModel { Title = "Energy Monitoring (Last 24 Hours)" };

            var dateTimeAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = DateTime.Now.AddHours(-24).ToOADate(), 
                Maximum = DateTime.Now.ToOADate(),
                StringFormat = "HH:mm", 
            };
            filteredPlotModel.Axes.Add(dateTimeAxis);

            var filteredSeries = new LineSeries { Title = "Energy Data" };

            foreach (var series in plotModel.Series.OfType<LineSeries>())
            {
                foreach (var point in series.Points)
                {
                    DateTime pointDateTime = DateTime.FromOADate(point.X); 

                    if (pointDateTime > DateTime.Now.AddHours(-24))
                    {
                        filteredSeries.Points.Add(point);
                    }
                }
            }

            filteredPlotModel.Series.Add(filteredSeries);

            return filteredPlotModel;
        }





        private async Task LoadDeviceInfoAsync(string deviceId)
        {
            try
            {
                var response = await _apiService.GetAllDevicesAsync();
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(response);

                var device = devices.FirstOrDefault(d => d.Id == deviceId);

                if (device != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Name: {device.Name}");
                    sb.AppendLine($"Unit: {device.Unit}");
                    sb.AppendLine($"Is On: {device.IsActive}");
                    sb.AppendLine($"Update Interval: {device.UpdateInterval}");

                    DeviceInfoTextBlock.Text = sb.ToString();
                }
                else
                {
                    DeviceInfoTextBlock.Text = "Device not found.";
                }
            }
            catch (Exception ex)
            {
                DeviceInfoTextBlock.Text = $"Error fetching device info: {ex.Message}";
            }
        }
    }
}