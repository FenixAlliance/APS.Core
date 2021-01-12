using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Models
{
    public static class Serialize
    {
        public static string ToJson(this B2CDecodedToken self) => JsonConvert.SerializeObject(self, AADB2CTokenConverter.Settings);
    }
}