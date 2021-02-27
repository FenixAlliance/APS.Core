using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace FenixAlliance.APS.Core.Models
{
    internal static class AADB2CTokenConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}