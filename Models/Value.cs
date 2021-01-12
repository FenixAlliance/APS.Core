using System;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Models
{
    public class Value
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}