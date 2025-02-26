using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;
using OBEM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace OBEM
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();

        public MainWindow()
        {
            InitializeComponent();
            LoadHighConsumptionData();
            LoadAnomaliesData();
            StartClock();
        }
        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => UpdateDateTime();
            timer.Start();
        }

        private void UpdateDateTime()
        {
            DateTime currentDateTime = DateTime.Now;
            DateTimeText.Content = currentDateTime.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Notifikacije());
        }
        private async void LoadHighConsumptionData()
        {
            string data = await _apiService.GetAllDevicesAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);

                // Filtriranje uređaja s Unit == "Power (kW)"
                var powerDevices = devices.Where(d => d.Unit == "Power (kW)").ToList();

                // Grupiranje po Group1 i Group3 i zbrajanje NumericValue
                var groupedData = powerDevices
                    .GroupBy(d => new { d.Group1, d.Group3 })
                    .Select(g => new
                    {
                        Group1 = g.Key.Group1,
                        Group3 = g.Key.Group3,
                        TotalConsumption = g.Sum(d => d.NumericValue),
                        DeviceNames = string.Join(", ", g.Select(d => d.Group2))
                    })
                    .OrderByDescending(g => g.TotalConsumption) // Sortiranje od najveće do najmanje potrošnje
                    .ToList();

                // Postavljanje podataka u DataGrid
                dgHighConsumption.ItemsSource = groupedData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju podataka: " + ex.Message);
            }
        }

        private async void LoadAnomaliesData()
        {
            string data = await _apiService.GetTrendingInfo();

            try
            {
                var trendingData = JsonConvert.DeserializeObject<TrendingInfo>(data);

                if (trendingData == null || trendingData.Records == null)
                {
                    MessageBox.Show("Nema podataka o trendovima ili je Records null.");
                    return;
                }

                var anomalies = new List<Anomaly>();

                foreach (var record in trendingData.Records)
                {
                    var id = record.Key;

                    if (record.Value == null)
                    {
                        Console.WriteLine($"Nema podataka za ID: {id}");
                        continue;
                    }

                    var validValues = record.Value
                        .Where(v => v != null && !string.IsNullOrEmpty(v.Value) && double.TryParse(v.Value, out _))
                        .ToList();

                    Console.WriteLine($"ID: {id}, Broj validnih vrijednosti: {validValues.Count}");

                    if (validValues == null || !validValues.Any())
                    {
                        Console.WriteLine($"Nema validnih podataka za ID: {id}");
                        continue;
                    }

                    var averageValue = validValues.Average(v => v.NumericValue);
                    var maxDeviation = validValues.Max(v => Math.Abs(v.NumericValue - averageValue));
                    var anomaly = validValues.FirstOrDefault(v => Math.Abs(v.NumericValue - averageValue) == maxDeviation);

                    Console.WriteLine($"ID: {id}, Prosjek: {averageValue}, Maksimalno odstupanje: {maxDeviation}");

                    if (anomaly != null)
                    {
                        anomalies.Add(new Anomaly
                        {
                            Id = id,
                            Deviation = maxDeviation,
                            Value = anomaly.NumericValue,
                            Timestamp = anomaly.Timestamp
                        });
                    }
                }

                Console.WriteLine($"Broj pronađenih anomalija: {anomalies.Count}");

                dgAnomalies.ItemsSource = anomalies;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju podataka o anomalijama: " + ex.Message);
            }
        }

        private void OpenApiTester_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApiTester());
        }

        private void OpenUnitEnergyMonitoring_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UnitDetails());
        }

        private void OpenEnergyCost_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EnergyCost());
        }
    }

   
}