using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
