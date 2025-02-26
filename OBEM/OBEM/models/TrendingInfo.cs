using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{

    public class Anomaly
    {
        public string Id { get; set; }
        public double Deviation { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TrendingValue
    {
        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("Time")] // Mapirajte "Time" iz JSON-a na Timestamp
        public DateTime Timestamp { get; set; }

        [JsonProperty("Status")]
        public int Status { get; set; }

        public double NumericValue
        {
            get
            {
                if (double.TryParse(Value, out double result))
                    return result;
                return 0; // Default vrijednost za nevalidne brojeve
            }
        }
    }

    public class TrendingInfo
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<string> Values { get; set; }
        public List<string> ColumnNames { get; set; }
        public Dictionary<string, List<TrendingValue>> Records { get; set; }
    }



}

