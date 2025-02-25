using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class Record
    {
        [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
        public double Value { get; set; }

        [JsonProperty("Time", NullValueHandling = NullValueHandling.Ignore)]
        public string Time { get; set; }
        [JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
        public int Satus { get; set; }
    }
}
