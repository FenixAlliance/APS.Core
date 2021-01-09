using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.APS.Core.Models;
using FenixAlliance.Models.DTOs.Authorization;
using FenixAlliance.Models.DTOs.Components.ID;
using FenixAlliance.Models.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.DataHelpers
{
    public class AccountOAuthHelpers
    {
        public AccountUsersHelpers UserHelpers;
        private static readonly HttpClient client = new HttpClient();

        private static readonly string MeEndpoint = "https://fenixalliance.com.co/api/V2/me";
        private static readonly string BusinessEndpoint = "https://fenixalliance.com.co/api/V2/Tenants/{0}/AppAuthorization";

        public static async Task<string> GetAndRegisterGUIDByRequestAsync(HttpRequest request, AccountUsersHelpers _accountUsersHelpers)
        {
            var Token = AccountOAuthHelpers.ExtractAuthToken(request);
            var DecodedToken = await AccountOAuthHelpers.DecodeAndValidateOAuthTokenAsync(Token);
            return _accountUsersHelpers.GetAndRegisterGUIDByToken(DecodedToken);
        }

        public static async Task<Holder> GetAllianceIDHolderByTokenAsync(string Token)
        {
            var DecodedToken = await AccountOAuthHelpers.DecodeAndValidateOAuthTokenAsync(Token);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var Holder = JsonConvert.DeserializeObject<Holder>(await client.GetStringAsync(MeEndpoint));

            // Check 
            if (DecodedToken.Oid.ToString() != Holder.ID)
            {
                return null;
            }

            return Holder;
        }

        public static async Task<ClientApplication> GetBusinessTenantApplicationByApiKeyAsync(string ApiKey)
        {

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", ApiKey);
            return JsonConvert.DeserializeObject<ClientApplication>(await client.GetStringAsync(BusinessEndpoint));
        }


        private static async Task<B2CDecodedToken> DecodeAndValidateOAuthTokenAsync(string Token)
        {
            var TokenComponents = Token.Split('.');
            var TokenHeader = TokenComponents[0];
            var TokenPayload = TokenComponents[1];
            var TokenSignature = TokenComponents[2];

            // Decode and return Token.
            var SerializedJSONTokeHeader = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(TokenHeader));
            var SerializedJSONTokenPayload = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(TokenPayload));

            var THeader = JsonConvert.DeserializeObject<JsonWebTokenHeader>(SerializedJSONTokeHeader);
            var SerializedJSONToken = B2CDecodedToken.FromJson(SerializedJSONTokenPayload);
            var TPayload = JsonConvert.DeserializeObject<JsonWebTokenPayload>(SerializedJSONTokenPayload);
            var IsValidSignature = false;

            var OpenIDEndpoints = new List<string>
            {
                "https://fenixallianceb2c.b2clogin.com/tfp/fenixallianceb2c.onmicrosoft.com/B2C_1_AllianceID/v2.0/.well-known/openid-configuration",
                "https://fenixallianceb2c.b2clogin.com/tfp/fenixallianceb2c.onmicrosoft.com/B2C_1_AndySignIn/v2.0/.well-known/openid-configuration"
            };

            foreach (var Endpoint in OpenIDEndpoints)
            {
                // Lookup Configuration
                string _OpenIdConfigurationResponse = await client.GetStringAsync(Endpoint);
                var ConfigurationResponse = JsonConvert.DeserializeObject<OpenIdConfigurationResponse>(_OpenIdConfigurationResponse);
                // Lookup Keys
                string _KeysLookupResponse = await client.GetStringAsync(ConfigurationResponse.JwksUri);
                var KeysRequestResponse = JsonConvert.DeserializeObject<KeysLookupResponse>(_KeysLookupResponse);

                foreach (var Key in KeysRequestResponse.Keys)
                {
                    // Match key and verify signature.
                    if (THeader.kid == Key.Kid)
                    {
                        // TODO: Validate Signature.
                        IsValidSignature = true;
                    }
                }
            }

            // If Signature is invalid, return null.
            if (!IsValidSignature)
            {
                return null;
            }

            // Decode and return Token.
            return SerializedJSONToken;
        }

        private static string ExtractAuthToken(HttpRequest request)
        {
            string Token = null;
            try
            {
                var AuthHeader = request.Headers.FirstOrDefault(x => x.Key.ToLowerInvariant() == "authorization").Value.FirstOrDefault();
                if(AuthHeader.Split(' ')[0].ToLowerInvariant() == "bearer")
                {
                    Token = AuthHeader.Split(' ')[1];
                }
            }
            catch
            {
                return null;
            }
            return Token;
        }

        public static string ExtractAuthType(HttpRequest Request)
        {
            try
            {
                var AuthHeader = Request.Headers.FirstOrDefault(x => x.Key.ToLowerInvariant() == "authorization");
                var AuthTypeKey = AuthHeader.Key;
                var AuthTypeValue = AuthHeader.Value;
               var AuthTypeValueString =  AuthTypeValue.ToString();
                var ToReturn = AuthTypeValueString.Split(' ')[0].ToLowerInvariant();
                return ToReturn;
            }
            catch
            {
                return null;
            }
        }
    }
}
