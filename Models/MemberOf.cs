using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FenixAlliance.Data.Access.Helpers.AADB2C
{
    public class MemberOf
    {
        [JsonProperty("odata.metadata")]
        public Uri OdataMetadata { get; set; }

        [JsonProperty("value")]
        public List<Value> Value { get; set; }
    }

    public class Value
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
