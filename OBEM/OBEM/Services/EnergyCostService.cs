using Newtonsoft.Json;
using OBEM.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OBEM.Services
{
    public class EnergyCostService
    {
        private readonly ApiService _apiService;
        private readonly double _pricePerKwh = 0.15;

        public EnergyCostService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<string> CalculateEnergyCostForApartments()
         {
             try
             {
                 Console.WriteLine("Pozivanje API-a za uređaje...");

                 string data = await _apiService.GetAllDevicesAsync();

                 Console.WriteLine($"Podaci sa API-a: {data}");

                 var devices = JsonConvert.DeserializeObject<List<DeviceInfo>>(data);
                 Console.WriteLine($"Broj uređaja nakon deserijalizacije: {devices.Count}");

                 var filteredDevices = devices.Where(d => d.Unit == "Power (kW)" && d.NumericValue > 0).ToList();

                 Console.WriteLine($"Broj uređaja nakon filtriranja: {filteredDevices.Count}");

                 double totalEnergyCost = 0;
                 StringBuilder result = new StringBuilder();

                 foreach (var device in filteredDevices)
                 {
                     double energyConsumption = device.NumericValue * device.UpdateInterval;
                     double energyCost = energyConsumption * _pricePerKwh;
                     totalEnergyCost += energyCost;

                     result.AppendLine($"Uređaj: {device.Name}, Potrošnja: {energyConsumption} kWh, Trošak: ${energyCost:F2}");

                     Console.WriteLine($"Uređaj: {device.Name}, Potrošnja: {energyConsumption}, Trošak: {energyCost}");
                 }

                 result.AppendLine($"Ukupni trošak energije za sve uređaje sa Power (kWh): ${totalEnergyCost:F2}");

                 Console.WriteLine($"Ukupni trošak energije: ${totalEnergyCost:F2}");

                 return result.ToString();
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Došlo je do greške: {ex.Message}");
                 return $"Greška pri izračunu troška: {ex.Message}";
             }
         }
    }
}
