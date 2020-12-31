using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FenixAlliance.Data.Access.Helpers.AADB2C
{
    public partial class KeysLookupResponse
    {
        [JsonProperty("keys")]
        public List<Key> Keys { get; set; }
    }

    public partial class Key
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