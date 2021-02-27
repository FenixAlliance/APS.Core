using Newtonsoft.Json;
using System;

namespace FenixAlliance.APS.Core.Models
{
    public class Value
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}