using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FenixAlliance.Data.Access.Helpers.AADB2C
{
    public partial class B2CDecodedToken
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }

        [JsonProperty("exp")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Exp { get; set; }

        [JsonProperty("nbf")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Nbf { get; set; }

        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("idp_access_token")]
        public string IdpAccessToken { get; set; }

        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("idp")]
        public string Idp { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("emails")]
        public List<string> Emails { get; set; }

        [JsonProperty("tfp")]
        public string Tfp { get; set; }

        [JsonProperty("scp")]
        public string Scp { get; set; }

        [JsonProperty("azp")]
        public string Azp { get; set; }

        [JsonProperty("ver")]
        public string Ver { get; set; }

        [JsonProperty("iat")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Iat { get; set; }
    }

    public partial class B2CDecodedToken
    {
        public static B2CDecodedToken FromJson(string json) => JsonConvert.DeserializeObject<B2CDecodedToken>(json, AADB2CTokenConverter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this B2CDecodedToken self) => JsonConvert.SerializeObject(self, AADB2CTokenConverter.Settings);
    }

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

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}