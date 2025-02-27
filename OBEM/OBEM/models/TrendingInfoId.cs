using Newtonsoft.Json;
using OBEM.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class TrendingInfoId
    {
        [JsonProperty("Result", NullValueHandling = NullValueHandling.Ignore)]
        public int Result { get; set; }

        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("Values", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Values { get; set; }

        [JsonProperty("ColumnNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ColumnNames { get; set; }

        [JsonProperty("Records", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<Record>> Records { get; set; }
    }
}
