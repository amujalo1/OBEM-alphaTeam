using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    namespace OBEM.Models
    {
        public class Record
        {
            [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
            public string Value { get; set; }

            [JsonProperty("Time", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime Time { get; set; }

            [JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
            public int Status { get; set; }
        }

        public class DeviceRecords
        {
            [JsonProperty("505/37", NullValueHandling = NullValueHandling.Ignore)]
            public List<Record> RecordsList { get; set; }
        }

        public class TrendingInfo
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
}

