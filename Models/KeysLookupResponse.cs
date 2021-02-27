using Newtonsoft.Json;
using System.Collections.Generic;

namespace FenixAlliance.APS.Core.Models
{
    public class KeysLookupResponse
    {
        [JsonProperty("keys")]
        public List<Key> Keys { get; set; }
    }
}