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

namespace OBEM
{
    public partial class UnitEnergyMonitoringByDeviceId : Page
    {
        public List<DeviceData> DeviceDataList { get; set; }
        private const string ApiToken = "zC3GtRfOFKY9kKI7CSJo6ZxSW33fT/f1NVQ9Lr0s0gk=";

        public UnitEnergyMonitoringByDeviceId()
        {
            InitializeComponent();
            DeviceDataList = new List<DeviceData>();
        }
        public UnitEnergyMonitoringByDeviceId(string id)
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
            string apiUrl = $"https://slb-skyline.on.dataminer.services/api/custom/OptimizingBuildingEnergyManagement/getTrendingInfo?type=average&id={deviceId}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiToken);
                    var response = await client.GetStringAsync(apiUrl);
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response);

                    if (apiResponse.Result == 1 && apiResponse.Records != null)
                    {
                        foreach (var record in apiResponse.Records)
                        {
                            var plotModel = new PlotModel { Title = $"Device: {record.Key}" };

                            var splineSeries = new LineSeries { Title = "Average Energy Value", MarkerType = MarkerType.Circle, Color = OxyColors.DeepSkyBlue };

                            List<double> values = new List<double>();
                            List<DateTime> timestamps = new List<DateTime>();

                            plotModel.Axes.Add(new DateTimeAxis
                            {
                                Position = AxisPosition.Bottom,
                                StringFormat = "dd.MM.yyyy",
                                Title = "Time"
                            });

                            foreach (var dataPoint in record.Value)
                            {
                                if (DateTimeOffset.TryParseExact(dataPoint.Time, "yyyy-MM-ddTHH:mm:sszzz",
                                    null, System.Globalization.DateTimeStyles.None, out DateTimeOffset time) &&
                                    double.TryParse(dataPoint.AverageValue.ToString(), out double averageValue))
                                {
                                    splineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time.DateTime), averageValue));
                                    values.Add(averageValue);
                                    timestamps.Add(time.DateTime);
                                }
                            }

                            plotModel.Series.Add(splineSeries);
                            EnergyPlot.Model = plotModel;

                            if (values.Count > 0)
                            {
                                double minValue = values.Min();
                                double maxValue = values.Max();
                                double avgValue = values.Average();

                                DateTime minTime = timestamps[values.IndexOf(minValue)];
                                DateTime maxTime = timestamps[values.IndexOf(maxValue)];

                                MinMaxAvgTextBlock.Text = $"Min: {minValue:F2} kWh at {minTime:dd.MM.yyyy HH:mm} ({minTime:HH} h)\n" +
                                                          $"Max: {maxValue:F2} kWh at {maxTime:dd.MM.yyyy HH:mm} ({maxTime:HH} h)\n" +
                                                          $"Average: {avgValue:F2} kWh";
                            }
                            else
                            {
                                MinMaxAvgTextBlock.Text = "No valid data available.";
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No data found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching data: {ex.Message}");
                }
            }
        }

        private async Task LoadDeviceInfoAsync(string deviceId)
        {
            string apiUrl = $"https://slb-skyline.on.dataminer.services/api/custom/OptimizingBuildingEnergyManagement/getAllDevices";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiToken);
                    var response = await client.GetStringAsync(apiUrl);
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

    public class ApiResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<object> Values { get; set; }
        public Dictionary<string, List<TrendingDataApi>> Records { get; set; }
    }

    public class TrendingDataApi
    {
        public double AverageValue { get; set; }  // Updated to reflect AverageValue
        public string Time { get; set; }
        public int Status { get; set; }
    }

    public class DeviceData
    {
        public string DeviceId { get; set; }
        public List<TrendingData> TrendingDataList { get; set; }

        public DeviceData(string deviceId)
        {
            DeviceId = deviceId;
            TrendingDataList = new List<TrendingData>();
        }
    }

    public class TrendingData
    {
        public string Value { get; set; }
        public DateTime Time { get; set; }
        public int Status { get; set; }
    }
}