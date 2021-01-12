using System.Collections.Generic;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Models
{
    public class KeysLookupResponse
    {
        [JsonProperty("keys")]
        public List<Key> Keys { get; set; }
    }
}