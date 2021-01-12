using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Models
{
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