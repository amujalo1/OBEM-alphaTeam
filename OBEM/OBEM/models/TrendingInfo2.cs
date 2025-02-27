using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    class TrendingInfo2
    {
        [JsonProperty("Result", NullValueHandling = NullValueHandling.Ignore)]
        public int Result { get; set; }
        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        [JsonProperty("Records", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<TrendingRecord>> Records { get; set; }
    }
}
