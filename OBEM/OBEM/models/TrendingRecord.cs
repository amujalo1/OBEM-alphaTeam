using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    class TrendingRecord
    {
        [JsonProperty("AverageValue", NullValueHandling = NullValueHandling.Ignore)]
        public double AverageValue { get; set; }
        [JsonProperty("MinimumValue", NullValueHandling = NullValueHandling.Ignore)]
        public double MinimumValue { get; set; }
        [JsonProperty("MaximumValue", NullValueHandling = NullValueHandling.Ignore)]
        public double MaximumValue { get; set; }
        [JsonProperty("Time", NullValueHandling = NullValueHandling.Ignore)]
        public string Time { get; set; }
        [JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }
    }
}
