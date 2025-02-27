using Newtonsoft.Json;
using OBEM.Services;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{

    class ApiModels
    {

        private readonly ApiService _apiService = new ApiService();


        public async Task<TrendingDataResult> LoadTrendingDataAsync(string deviceId)
        {
            var result = new TrendingDataResult();

            try
            {
                var response = await _apiService.GetTrendingInfoAsync("average", deviceId, "01/01/2024 00:00");
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
                        result.PlotModel = plotModel;

                        if (values.Count > 0)
                        {
                            double minValue = values.Min();
                            double maxValue = values.Max();
                            double avgValue = values.Average();

                            DateTime minTime = timestamps[values.IndexOf(minValue)];
                            DateTime maxTime = timestamps[values.IndexOf(maxValue)];

                            result.MinMaxAvgText = $"Min: {minValue:F2} kWh at {minTime:dd.MM.yyyy HH:mm} ({minTime:HH} h)\n" +
                                                  $"Max: {maxValue:F2} kWh at {maxTime:dd.MM.yyyy HH:mm} ({maxTime:HH} h)\n" +
                                                  $"Average: {avgValue:F2} kWh";
                        }
                        else
                        {
                            result.MinMaxAvgText = "No valid data available.";
                        }
                    }
                }
                else
                {
                    result.ErrorMessage = "No data found.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error fetching data: {ex.Message}";
            }

            return result;
        }

    }
    public class TrendingDataResult
    {
        public PlotModel PlotModel { get; set; }
        public string MinMaxAvgText { get; set; }
        public string ErrorMessage { get; set; }
    }
}


