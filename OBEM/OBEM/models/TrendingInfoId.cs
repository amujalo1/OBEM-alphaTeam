using Newtonsoft.Json;
using OBEM.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class ApiResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<object> Values { get; set; }
        public Dictionary<string, List<TrendingDataApi>> Records { get; set; }
    }

    public class TrendingDataApi
    {
        public double AverageValue { get; set; }
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
