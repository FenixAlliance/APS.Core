using System.Collections.Generic;
using Newtonsoft.Json;

namespace FenixAlliance.Data.Access.Helpers.AADB2C
{
    public class KeysLookupResponse
    {
        [JsonProperty("keys")]
        public List<Key> Keys { get; set; }
    }

    public class Key
    {
        [JsonProperty("kid")]
        public string Kid { get; set; }

        [JsonProperty("nbf")]
        public long Nbf { get; set; }

        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("kty")]
        public string Kty { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("n")]
        public string N { get; set; }
    }

    
}