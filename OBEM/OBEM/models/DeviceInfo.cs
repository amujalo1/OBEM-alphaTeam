using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace OBEM.models
{

    public class DeviceInfo
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("lowerBound", NullValueHandling = NullValueHandling.Ignore)]
        public int LowerBound { get; set; }

        [JsonProperty("upperBound", NullValueHandling = NullValueHandling.Ignore)]
        public int UpperBound { get; set; }

        [JsonProperty("numericValue", NullValueHandling = NullValueHandling.Ignore)]
        public int NumericValue { get; set; }

        [JsonProperty("stringValue", NullValueHandling = NullValueHandling.Ignore)]
        public string StringValue { get; set; }

        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("simulationType", NullValueHandling = NullValueHandling.Ignore)]
        public int SimulationType { get; set; }

        [JsonProperty("growthRatio", NullValueHandling = NullValueHandling.Ignore)]
        public int GrowthRatio { get; set; }

        [JsonProperty("group1", NullValueHandling = NullValueHandling.Ignore)]
        public string Group1 { get; set; }

        [JsonProperty("group2", NullValueHandling = NullValueHandling.Ignore)]
        public string Group2 { get; set; }

        [JsonProperty("group3", NullValueHandling = NullValueHandling.Ignore)]
        public string Group3 { get; set; }

        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }

        [JsonProperty("updateInterval", NullValueHandling = NullValueHandling.Ignore)]
        public int UpdateInterval { get; set; }
    }


}