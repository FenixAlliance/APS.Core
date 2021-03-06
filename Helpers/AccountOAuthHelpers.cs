using FenixAlliance.ABM.Data.Interfaces.Helpers;
using FenixAlliance.ABM.Models.DTOs.Auth;
using FenixAlliance.ABM.Models.DTOs.Auth.AADB2C;
using FenixAlliance.ABM.Models.DTOs.Auth.OAuth2;
using FenixAlliance.ABM.Models.DTOs.Auth.OpenID;
using FenixAlliance.ABM.Models.DTOs.Components.Holders;
using FenixAlliance.ABM.Models.DTOs.Responses;
using FenixAlliance.ACL.Configuration.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Helpers
{
    public class AccountOAuthHelpers
    {

        private ISuiteOptions SuiteOptions { get; set; }
        private IConfiguration Configuration { get; set; }
        private IWebHostEnvironment Environment { get; set; }
        private IHttpClientFactory HttpClientFactory { get; set; }
        private IAccountUsersHelpers AccountUsersHelpers { get; set; }
        private HttpClient HttpClient { get; set; } = new HttpClient();

        private string MeEndpoint { get; set; } = "https://fenixalliance.com.co/api/V2/me";
        private string BusinessEndpoint { get; set; }  = "https://fenixalliance.com.co/api/V2/Tenants/{0}/AppAuthorization";



        public AccountOAuthHelpers(ISuiteOptions SuiteOptions, IConfiguration Configuration, IWebHostEnvironment Environment, IAccountUsersHelpers AccountUsersHelpers, IHttpClientFactory HttpClientFactory)
        {
            this.HttpClient = HttpClientFactory.CreateClient();
            this.AccountUsersHelpers = AccountUsersHelpers;
            this.HttpClientFactory = HttpClientFactory;
            this.Configuration = Configuration;
            this.SuiteOptions = SuiteOptions;
            this.Environment = Environment;
        }



        public async Task<string> GetAndRegisterGUIDByRequestAsync(HttpRequest request, AccountUsersHelpers _accountUsersHelpers)
        {
            var Token = ExtractAuthToken(request);
            var DecodedToken = await DecodeAndValidateOAuthTokenAsync(Token);
            return _accountUsersHelpers.GetAndRegisterGUIDByToken(DecodedToken);
        }

        public async Task<Holder> GetAccountHolderByTokenAsync(string Token)
        {
            var DecodedToken = await DecodeAndValidateOAuthTokenAsync(Token);

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var Holder = JsonConvert.DeserializeObject<Holder>(await HttpClient.GetStringAsync(MeEndpoint));

            // Check 
            if (DecodedToken.Oid.ToString() != Holder.ID)
            {
                return null;
            }

            return Holder;
        }

        public async Task<ClientApplication> GetBusinessTenantApplicationByApiKeyAsync(string ApiKey)
        {

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", ApiKey);
            return JsonConvert.DeserializeObject<ClientApplication>(await HttpClient.GetStringAsync(BusinessEndpoint));
        }


        private async Task<B2CDecodedToken> DecodeAndValidateOAuthTokenAsync(string Token)
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
                string _OpenIdConfigurationResponse = await HttpClient.GetStringAsync(Endpoint);
                var ConfigurationResponse = JsonConvert.DeserializeObject<OpenIdConfigurationResponse>(_OpenIdConfigurationResponse);
                // Lookup Keys
                string _KeysLookupResponse = await HttpClient.GetStringAsync(ConfigurationResponse.JwksUri);
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

        private string ExtractAuthToken(HttpRequest request)
        {
            string Token = null;
            try
            {
                var AuthHeader = request.Headers.FirstOrDefault(x => x.Key.ToLowerInvariant() == "authorization").Value.FirstOrDefault();
                if (AuthHeader.Split(' ')[0].ToLowerInvariant() == "bearer")
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

        public string ExtractAuthType(HttpRequest Request)
        {
            try
            {
                var AuthHeader = Request.Headers.FirstOrDefault(x => x.Key.ToLowerInvariant() == "authorization");
                var AuthTypeKey = AuthHeader.Key;
                var AuthTypeValue = AuthHeader.Value;
                var AuthTypeValueString = AuthTypeValue.ToString();
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
