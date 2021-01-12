using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Models
{
    public class MemberOf
    {
        [JsonProperty("odata.metadata")]
        public Uri OdataMetadata { get; set; }

        [JsonProperty("value")]
        public List<Value> Value { get; set; }
    }
}
