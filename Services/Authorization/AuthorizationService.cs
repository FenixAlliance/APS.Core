using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Models.DTOs.Authorization;
using FenixAlliance.ACL.Configuration.Interfaces;
using FenixAlliance.APS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Services.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthorizationService : IAuthorizationService, IDisposable
    {
        public IConfiguration Configuration { get; set; }
        public IHostEnvironment Environment { get; set; }
        public HttpClient HttpClient { get; set; }
        public ABMContext DataContext { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string Scopes { get; set; }
        public string BaseEndpoint { get; set; } = "rest.absuite.net";
        public string AuthEndpoint { get; set; }
        public bool IsAuthorized { get; set; }

        public AuthorizationService(ABMContext DataContext, IConfiguration configuration, IHostEnvironment hostingEnvironment, ISuiteOptions Options)
        {
            this.DataContext = DataContext;
            Environment = hostingEnvironment;
            Configuration = configuration;
            Scopes = Options?.ABS?.Portal?.Scopes;
            PublicKey = Options?.ABS?.Portal?.PublicKey;
            PrivateKey = Options?.ABS?.Portal?.PrivateKey;
            BaseEndpoint = Options?.ABS?.Portal?.BaseEndpoint;
            AuthEndpoint = $"https://{BaseEndpoint}/api/v2/OAuth2/Token?client_id={PublicKey}&client_secret={PrivateKey}&grant_type=client_credentials&requested_scopes={Scopes}";
            HttpClient = new HttpClient() { BaseAddress = new Uri($"https://{BaseEndpoint ?? "rest.absuite.net" }/api/v2/") };
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// This method retuirns true if the current user is authenticated, has a selected BusinessID (Whih means a valid Business Profile Record) 
        /// AND it has either the Business Owner Property set to true or the business_owner role.
        /// </summary>
        /// <param name="User">The user to inspect.</param>
        /// <returns></returns>
        public async Task<bool> IsAdmin(ClaimsPrincipal User)
        {

            return false;
        }

        public async Task<bool> IsAdmin(string BusinessTenantID)
        {
            return false;
        }

        public async Task<bool> IsAdmin(ClaimsPrincipal User, string BusinessTenantID, List<string> RequiredRoles, List<string> RequiredPermissions)
        {
            return false;
        }

        public async Task<bool> IsHolderAuthorized(string BusinessTenantID)
        {
            return false;
        }

        public async Task AuthorizeClient()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    var TokenRequest = await client.GetAsync(AuthEndpoint);
                    TokenRequest.EnsureSuccessStatusCode();
                    var TokenString = await TokenRequest.Content.ReadAsStringAsync();
                    var Token = JsonSerializer.Deserialize<JsonWebToken>(TokenString);
                    HttpClient.DefaultRequestHeaders.Add("Authorization", $"{Token.TokenType} {Token.AccessToken}");
                    IsAuthorized = true;
                }
            }
            catch (Exception e)
            {
                IsAuthorized = false;
                Console.WriteLine(e.ToString());
            }
        }

        public async Task<JsonWebToken> GetToken()
        {
            JsonWebToken Response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    var TokenRequest = await client.GetAsync(AuthEndpoint);
                    TokenRequest.EnsureSuccessStatusCode();
                    var TokenString = await TokenRequest.Content.ReadAsStringAsync();
                    Response = JsonSerializer.Deserialize<JsonWebToken>(TokenString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return Response;
        }

        public async Task<JsonWebToken> RefreshToken(JsonWebToken JsonWebToken)
        {
            try
            {
                if (JsonWebToken == null || string.IsNullOrEmpty(JsonWebToken.AccessToken) || String.IsNullOrEmpty(JsonWebToken.TokenType))
                {
                    JsonWebToken = await GetToken();
                }
                else
                {
                    //TODO:_ Check Token Header for Expiration and renew if expired.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return JsonWebToken;
        }

        public async Task AuthorizeClient(List<string> RequestedScopes)
        {
            try
            {
                var Token = await GetToken();

                this.IsAuthorized = true;

                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token.TokenType, Token.AccessToken);
            }
            catch (Exception e)
            {
                this.IsAuthorized = false;
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
