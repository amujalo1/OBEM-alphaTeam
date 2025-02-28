using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;
using OBEM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace OBEM
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            LoadHighConsumptionData();
            LoadAnomaliesData();
            StartClock();
            StartThreadTimer();
        }
        private void StartThreadTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            Console.WriteLine("Thread zavrsen//////");
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            InitializeComponent();
            LoadHighConsumptionData();
            LoadAnomaliesData();
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
            var anomalies = new List<Anomaly>();
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddDays(-1);

            try
            {
                var response = await _apiService.GetAllDevicesAsync();
                var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(response);

                if (devices == null || !devices.Any())
                {
                    Console.WriteLine("Nema dostupnih uređaja.");
                    return;
                }

                foreach (var device in devices)
                {
                    string formattedStartDate = startDate.ToString("MM/dd/yyyy HH:mm");

                    var deviceResponse = await _apiService.GetTrendingInfoAsync("average", device.Id.ToString(), formattedStartDate);

                    if (string.IsNullOrEmpty(deviceResponse))
                    {
                        Console.WriteLine($"API odgovor je prazan za ID: {device.Id}");
                        continue;
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(deviceResponse);

                    if (apiResponse == null || apiResponse.Records == null || !apiResponse.Records.Any())
                    {
                        Console.WriteLine($"Nema podataka za ID: {device.Id}");
                        continue;
                    }

                    foreach (var record in apiResponse.Records)
                    {
                        if (record.Value == null)
                        {
                            Console.WriteLine($"Record.Value je null za ID: {device.Id}");
                            continue;
                        }

                        var validValues = record.Value
                            .Where(v => v != null && double.TryParse(v.AverageValue.ToString(), out _))
                            .ToList();

                        if (validValues.Any())
                        {
                            var averageValue = validValues.Average(v => double.Parse(v.AverageValue.ToString()));
                            var maxDeviation = validValues.Max(v => Math.Abs(double.Parse(v.AverageValue.ToString()) - averageValue));
                            var anomaly = validValues.FirstOrDefault(v => Math.Abs(double.Parse(v.AverageValue.ToString()) - averageValue) == maxDeviation);

                            if (anomaly != null)
                            {
                                string severity = "Normal";
                                if (maxDeviation > 10) severity = "Alert";
                                else if (maxDeviation > 5) severity = "Warning";
                                if (device.Name == "Water Consumption")
                                    continue;
                                anomalies.Add(new Anomaly
                                {
                                    Id = $"505/{device.Id}",
                                    Name = device.Name,
                                    Group1 = device.Group1,
                                    Group2 = device.Group2,
                                    Group3 = device.Group3,
                                    Deviation = maxDeviation,
                                    Value = double.Parse(anomaly.AverageValue.ToString()),
                                    Timestamp = DateTimeOffset.ParseExact(anomaly.Time, "yyyy-MM-ddTHH:mm:sszzz", null, System.Globalization.DateTimeStyles.None).DateTime,
                                    Severity = severity
                                });
                            }
                        }
                    }
                }

                dgAnomalies.ItemsSource = anomalies;

                // Pretplata na LoadingRow događaj
                dgAnomalies.LoadingRow += DgAnomalies_LoadingRow;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju podataka o anomalijama: " + ex.Message);
            }
        }


        private void DgAnomalies_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var anomaly = e.Row.DataContext as Anomaly;
            if (anomaly != null)
            {
                switch (anomaly.Severity)
                {
                    case "Warning":
                        e.Row.Background = new SolidColorBrush(Colors.Yellow);
                        break;
                    case "Alert":
                        e.Row.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case "Normal":
                        e.Row.Background = new SolidColorBrush(Colors.Green);
                        break;
                }
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
        private void OpenDeviceGraphPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UnitEnergyMonitoringByDeviceId());
        }
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow log = new LoginWindow();
            this.Close();
            log.Show();
        }
        private void ShowGraph_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button.DataContext;

            var idProperty = selectedItem.GetType().GetProperty("Id");
            if (idProperty == null)
            {
                MessageBox.Show("Odabrani red nema ID svojstvo.");
                return;
            }

            var fullId = idProperty.GetValue(selectedItem) as string;
            if (string.IsNullOrEmpty(fullId))
            {
                MessageBox.Show("ID nije pronađen.");
                return;
            }

            var idParts = fullId.Split('/');
            if (idParts.Length < 2)
            {
                MessageBox.Show("ID nije u očekivanom formatu (npr. '505/37').");
                return;
            }

            var id = idParts[1];

            MainFrame.Navigate(new UnitEnergyMonitoringByDeviceId(id));
        }

    }

   
}