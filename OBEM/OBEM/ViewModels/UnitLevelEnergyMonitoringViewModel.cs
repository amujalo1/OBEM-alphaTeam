using Newtonsoft.Json;
using OBEM.models;
using OBEM.Services;
using OBEM.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OBEM.ViewModels
{
    public class UnitLevelEnergyMonitoringViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;

        private ObservableCollection<Device> Devices { get; set; } = new ObservableCollection<Device>();
        private ObservableCollection<Device> _filteredDevices = new ObservableCollection<Device>();
        public ObservableCollection<Device> FilteredDevices
        {
            get => _filteredDevices;
            set
            {
                _filteredDevices = value;
                OnPropertyChanged(nameof(FilteredDevices));
            }
        }
        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Floors { get; set; } = new ObservableCollection<string>();

        private double _totalPowerConsumption;
        public double TotalPowerConsumption
        {
            get => _totalPowerConsumption;
            set
            {
                _totalPowerConsumption = value;
                OnPropertyChanged(nameof(TotalPowerConsumption));
            }
        }

        private string _selectedUnit;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                OnPropertyChanged(nameof(SelectedUnit));
                FilterDevices();
            }
        }

        private string _selectedFloor;
        public string SelectedFloor
        {
            get => _selectedFloor;
            set
            {
                _selectedFloor = value;
                OnPropertyChanged(nameof(SelectedFloor));
                FilterDevices();
            }
        }

        public ICommand LoadDataCommand { get; }
        public UnitLevelEnergyMonitoringViewModel()
        {
            _apiService = new ApiService();
            LoadDataCommand = new RelayCommand(async _ => await LoadDevices());
            _ = LoadDevices(); // auto load on init
        }

        private async Task LoadDevices()
        {
            var devices = await _apiService.GetAllDevicesAsync(); // Fetch devices from API service
            var deviceInfoList = JsonConvert.DeserializeObject<List<DeviceInfo>>(devices) ?? new List<DeviceInfo>();

            Devices.Clear();
            FilteredDevices.Clear();
            Units.Clear();
            Floors.Clear();

            foreach (var deviceInfo in deviceInfoList)
            {
                var device = Device.FromDeviceInfo(deviceInfo);
                Devices.Add(device);

                if (!Units.Contains(device.ApartmentName) && !string.IsNullOrEmpty(device.ApartmentName))
                    Units.Add(device.ApartmentName);

                if (!Floors.Contains(device.Floor) && !string.IsNullOrEmpty(device.Floor))
                    Floors.Add(device.Floor);
            }
        }

            private void FilterDevices()
            {
                var filtered = Devices.Where(d =>
                    (string.IsNullOrEmpty(SelectedUnit) || d.Unit == SelectedUnit) &&
                    (string.IsNullOrEmpty(SelectedFloor) || d.Floor == SelectedFloor)).ToList();

                FilteredDevices.Clear();
                foreach (var device in filtered)
                {
                    FilteredDevices.Add(device);
                }

                TotalPowerConsumption = FilteredDevices.Sum(d => d.NumericValue);
            }
    
    }
}
