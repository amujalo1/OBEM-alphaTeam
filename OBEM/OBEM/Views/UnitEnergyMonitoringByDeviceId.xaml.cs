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

namespace OBEM
{
    public partial class UnitEnergyMonitoringByDeviceId : Window
    {
        public List<DeviceData> DeviceDataList { get; set; }
        private const string ApiToken = "zC3GtRfOFKY9kKI7CSJo6ZxSW33fT/f1NVQ9Lr0s0gk=";

        public UnitEnergyMonitoringByDeviceId()
        {
            InitializeComponent();
            DeviceDataList = new List<DeviceData>();
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

                            var splineSeries = new LineSeries { Title = "Average Energy Value", MarkerType = MarkerType.Circle };

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

