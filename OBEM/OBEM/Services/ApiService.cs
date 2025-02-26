using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace OBEM.Services
{
    public class ApiService
    {
        private readonly string _baseUrl = "https://slb-skyline.on.dataminer.services/api/custom/OptimizingBuildingEnergyManagement"; // URL elementa
        private readonly string _apiToken = "zC3GtRfOFKY9kKI7CSJo6ZxSW33fT/f1NVQ9Lr0s0gk="; // API token

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            return client;
        }

        // Get All Devices
        public async Task<string> GetAllDevicesAsync()
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getAllDevices";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get All Categories
        public async Task<string> GetAllCategoriesAsync()
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getAllCategories";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get Device by Name
        public async Task<string> GetDeviceByNameAsync(string deviceName)
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getDevice?name={deviceName}";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get Device by Category Name
        public async Task<string> GetDeviceByCategoryAsync(string categoryName)
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getCategory?name={categoryName}";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get Trending Info
        public async Task<string> GetTrendingInfo()
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getTrendingInfo";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get Trending Info By Id
        public async Task<string> GetTrendingInfoById(int Id)
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getTrendingInfo?id={Id}";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Get trending info by id 2

        public async Task<string> GetTrendingInfoById2(int Id)
        {
            using (var client = CreateHttpClient())
            {
                string url = $"{_baseUrl}/getTrendingInfo?type=average&?id={Id}";
                try
                {
                    var response = await client.GetAsync(url);
                    return await HandleResponse(response);
                }
                catch (Exception ex)
                {
                    return $"Greška: {ex.Message}";
                }
            }
        }

        // Helper method for handling response
        private async Task<string> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData; 
            }
            else
            {
                return $"Greška: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
    }
}
