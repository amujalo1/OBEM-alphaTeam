using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OBEM.models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class CategoryInfo
    {
        [JsonProperty("categoryNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int CategoryNumber { get; set; }

        [JsonProperty("categoryNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> CategoryNames { get; set; }
    }
}
