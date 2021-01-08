using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Models.Global.Integrations.Applications;
using FenixAlliance.ABM.Models.Tenants.BusinessProfileRecords;
using FenixAlliance.ABM.SDK.Helpers;
using FenixAlliance.Data.Access.DataAccess;
using FenixAlliance.Data.Access.Helpers;
using FenixAlliance.Models.DTOs.Authorization;
using FenixAlliance.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.APS.Core.Controllers.Auth
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiExplorerSettings(GroupName = "IAM")]
    [Produces("application/json", "application/xml")]
    [Consumes("application/json", "application/xml")]
    public class OAuth2Controller : ControllerBase
    {
        public ABMContext DataContext { get; }
        public StoreHelpers StoreHelpers { get; }
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }
        public BusinessHelpers BusinessHelpers { get; }
        public AccountUsersHelpers AccountUsersHelpers { get; }
        public AccountGraphHelpers AccountGraphHelpers { get; }
        public BusinessDataAccessClient BusinessDataAccess { get; }
        public BlobStorageDataAccessClient StorageDataAccessClient { get; }

        public OAuth2Controller(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            DataContext = context;
            Configuration = configuration;
            HostEnvironment = hostingEnvironment;
            StoreHelpers = new StoreHelpers(DataContext);
            BusinessHelpers = new BusinessHelpers(context);
            AccountUsersHelpers = new AccountUsersHelpers(context);
            AccountGraphHelpers = new AccountGraphHelpers(DataContext, Configuration);
            BusinessDataAccess = new BusinessDataAccessClient(DataContext, Configuration, HostEnvironment);
            StorageDataAccessClient = new BlobStorageDataAccessClient();
        }

        /// <summary>
        /// Gets a token for a particular application.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Token")]
        public async Task<ActionResult<JsonWebToken>> GetToken(string client_id, string client_secret, string grant_type, string requested_scopes, string requested_enrollment)
        {
            var JWT = new JsonWebToken();
            try
            {

                /* TODO: Assert the grant type and act based on it.
                 */
                var Enrollment = new BusinessProfileRecord();
                if (string.IsNullOrEmpty(grant_type))
                {
                    return BadRequest("grant_type not present.");
                }

                switch (grant_type)
                {
                    case "refresh_token":
                        break;
                    case "client_credentials":

                        /* 1.  Check for required properties.
                         * 2.  Try to retrieve the current secret using client_id (Public Key) and client_secret (Private Key).
                         * 3.  Assert that the secret exists.
                         * 4.  Assert the existence of the requested enrollment or fallback to application owner.
                         * 5.  Assert the existence of each and every requested scope within the context of a particular tenant. (A.K.A Granted for the entire organization)
                         * 6.  Assert the existence of each and every requested scope within the context of a particular enrollment. (A.K.A Granted granular for a particular BPR)
                         * 7.  Build the JWT Header.
                         * 8.  Build the JWT Payload.
                         * 9.  Get both public and private signing keys as RSAParameters.
                         * 10. Sign the JWT Payload and compose the token APIResponse.
                         */

                        // 1. Check for required properties
                        if (String.IsNullOrEmpty(client_id) || String.IsNullOrEmpty(client_secret))
                        {
                            return BadRequest();
                        }

                        if (requested_scopes == null || !requested_scopes.ToList().Any())
                        {
                            return BadRequest();
                        }


                        // 2. Try to retrieve the current secret using client_id (Public Key) and client_secret (Private Key).
                        var SecretsSet = await DataContext.BusinessApplicationSecret
                            .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationPermissionGrants).ThenInclude(c => c.BusinessPermission)
                            .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationRequestedPermissions).ThenInclude(c => c.BusinessPermission)
                            .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                            .FirstAsync(c => c.PublicKey == client_id && c.PrivateKey == client_secret);


                        // 3. Assert that the secret exists.
                        if (SecretsSet == null)
                        {
                            return Unauthorized();
                        }


                        // 4. Assert the existence of the requested enrollment or fallback to application owner. Bad Request if not found.
                        var EnrollmentID = string.Empty;

                        if (String.IsNullOrEmpty(requested_enrollment) || !await DataContext.BusinessProfileRecord.AnyAsync(c => c.ID == requested_enrollment))
                        {
                            if (!await DataContext.BusinessProfileRecord.AnyAsync(c => c.ID == Enrollment.ID))
                            {
                                EnrollmentID = SecretsSet.BusinessApplication.BusinessProfileRecordID;
                            }
                        }
                        else
                        {
                            EnrollmentID = requested_enrollment;
                        }

                        if (String.IsNullOrEmpty(EnrollmentID) || !await DataContext.BusinessProfileRecord.AnyAsync(c => c.ID == EnrollmentID))
                        {
                            return BadRequest("Invalid Enrollment.");
                        }

                        Enrollment = await DataContext.BusinessProfileRecord
                            .Include(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                            .Include(c => c.BusinessProfileSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                            .FirstAsync(c => c.ID == EnrollmentID);

                        var RequiredAppPermissions = SecretsSet.BusinessApplication.BusinessApplicationRequestedPermissions.Where(c => c.IsOptional == false).Select(c => c.BusinessPermission).Select(c => c.ID).ToList();
                        
                        // 5. Assert the existence of each and every requested scope within the context of a particular tenant. (A.K.A Granted for the entire organization)
                        var GrantedAppPermissionsThroughRoles = SecretsSet.BusinessApplication.BusinessApplicationSecurityRoleGrants.Where(c => c.BusinessSecurityRole.BusinessID == Enrollment.BusinessID).Select(c => c.BusinessSecurityRole).SelectMany(c => c.BusinessRolePermissionGrants).Select(c => c.BusinessPermission);
                        
                        var GrantedAppDirectPermissions = SecretsSet.BusinessApplication
                            .BusinessApplicationPermissionGrants.Where(c => c.BusinessID == Enrollment.BusinessID)
                            .Select(c => c.BusinessPermission);
                        
                        var ApplicationPermissions = GrantedAppPermissionsThroughRoles.Union(GrantedAppDirectPermissions).Select(c => c.ID);

                        var applicationPermissions = (string[]) ApplicationPermissions;
                        var PermissionsRequestedButNotGranted = RequiredAppPermissions.Except(applicationPermissions.ToList());

                        if (PermissionsRequestedButNotGranted.Any())
                        {
                            return Unauthorized("There are permissions that are required for this application, but have not been granted.");
                        }

                        // 6. Assert the existence of each and every requested scope within the context of a particular enrollment. (A.K.A Granted granular for a particular BPR)
                        var GrantedEnrollmentPermissionsThroughRoles = Enrollment.BusinessProfileSecurityRoleGrants.Where(c => c.BusinessSecurityRole.BusinessID == Enrollment.BusinessID || c.BusinessSecurityRole.IsSystemSecurityRole).Select(c => c.BusinessSecurityRole).SelectMany(c => c.BusinessRolePermissionGrants).Select(c => c.BusinessPermission);
                        var GrantedEnrollmentDirectPermissions = Enrollment.BusinessProfileDirectPermissionGrants.Where(c => c.BusinessID == Enrollment.BusinessID).Select(c => c.BusinessPermission);
                        var EnrollmentPermissions = GrantedEnrollmentPermissionsThroughRoles.Union(GrantedEnrollmentDirectPermissions).Select(c => c.ID);

                        // Assert that every requested scope is a valid scope
                        var AreRequestedScopesValid = true;
                        requested_scopes.Split(" ")?.ToList().ForEach(async requestedScope =>
                        {
                            if (await DataContext.BusinessPermission.AnyAsync(c => c.ID == requestedScope))
                            {
                                AreRequestedScopesValid = false;
                            }
                        });

                        if (!AreRequestedScopesValid)
                        {
                            return BadRequest("Invalid Scopes.");
                        }

                        // Intersect Application Granted Permissions with Enrollment Granted Permissions.
                        var ApplicationIntersectionEnrollmentPermissions = applicationPermissions.Intersect(EnrollmentPermissions);
                        var ScopePermissionsRequestedButNotGranted = requested_scopes.Split(" ").ToList().Except(ApplicationIntersectionEnrollmentPermissions.ToList());

                        // 3. Assert that the secret exists.
                        if (ScopePermissionsRequestedButNotGranted.Any())
                        {
                            return Unauthorized("Not all the requested permissions have been granted for this application under the scope of this enrollment.");
                        }

                        // 7.Build the JWT Header.
                        var Header = new JsonWebTokenHeader()
                        {
                            alg = "RS256",
                            kid = SecretsSet.ID,
                            typ = "Bearer",
                            jku = $"https://fenixalliance.com.co/api/v2/oauth2/{SecretsSet.BusinessApplicationID}/Keys"
                        };

                        // 8. Build the JWT Payload.
                        var AudienceSchema = $"https://fenixalliance.com.co/api/v2/oauth2/{SecretsSet.BusinessApplication.ID}/{SecretsSet.BusinessApplication.BusinessID}/{Enrollment.ID}/";

                        var Payload = new JsonWebTokenPayload()
                        {
                            sub = Enrollment.ID,
                            aud = AudienceSchema,
                            act = Enrollment.BusinessID,
                            aid = Enrollment.AllianceIDHolderGUID,
                            cid = SecretsSet.BusinessApplication.ID,
                            iss = $"https://fenixalliance.com.co/api/v2/oauth2/{SecretsSet.BusinessApplication.BusinessID}/{SecretsSet.BusinessApplication.ID}/",
                            Scopes = requested_scopes.Split(" ").ToList(),
                            exp = (long)DateTime.Now.AddHours(1).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            iat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            nbf = (long)DateTime.Now.AddMinutes(30).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                        };

                        //Create a new instance of RSACryptoServiceProvider.
                        using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                        {
                            // 9. Get private signing key as RSAParameters.
                            var Params = StringHelpers.Base64Decode(SecretsSet.SigningPrivateKey);
                            RSA.FromXmlString(Params);
                            var RSAParams = RSA.ExportParameters(true);


                            // 10. Sign the JWT Payload and compose the token APIResponse.
                            JWT = new JsonWebToken()
                            {
                                TokenType = "Bearer",
                                ExpiresIn = 1799,
                                AccessToken = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Header)))}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Payload)))}.{SecurityHelpers.SignPayload(Payload, RSAParams)}"
                            };
                        }
                        break;
                    default:
                        return BadRequest("grant_type not valid.");

                }

                if (String.IsNullOrEmpty(JWT.AccessToken))
                {
                    return BadRequest("Token request was unsuccessful.");
                }
            }
            catch
            {
                return BadRequest("Token request was unsuccessful.");
            }

            return Ok(JWT);

        }

        [HttpGet("WhoAmI")]
        [Produces("application/json")]
        public async Task<ActionResult> Get(string TenantID)
        {
            var APIResponse = JsonSerializer.Deserialize<APIResponse>(JsonSerializer.Serialize(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse);
        }

        /// <summary>
        /// Gets the signing the keys for a particular application.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{ApplicationID}/Keys")]
        public async Task<ActionResult<JsonWebKeySet>> GetJWKs(string ApplicationID)
        {

            // 1. Check for required properties
            if (string.IsNullOrEmpty(ApplicationID) || !await DataContext.BusinessApplication.AnyAsync(c => c.ID == ApplicationID))
            {
                return BadRequest();
            }

            // 2. Try to retrieve the current secret using client_id (Public Key) and client_secret (Private Key).
            var Secrets = await DataContext.BusinessApplicationSecret
                .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationRequestedPermissions).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessApplication).ThenInclude(c => c.BusinessApplicationSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole)
                    .ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(c => c.BusinessApplicationID == ApplicationID).ToListAsync();


            var JWKs = new JsonWebKeySet()
            {
                Keys = new List<JsonWebKey>()
            };

            foreach (var secret in Secrets)
            {
                var JWK = new JsonWebKey()
                {
                    Kid = secret.ID,
                    Kty = "RSA",
                    Use = "sig",
                };

                // Get NBF
                if (secret.BusinessApplicationSecretPeriod == BusinessApplicationSecretPeriod.OneYear)
                {
                    JWK.Nbf = (long)(secret.Timestamp.AddYears(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }

                if (secret.BusinessApplicationSecretPeriod == BusinessApplicationSecretPeriod.TwoYears)
                {
                    JWK.Nbf = (long)(secret.Timestamp.AddYears(2).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }

                if (secret.BusinessApplicationSecretPeriod == BusinessApplicationSecretPeriod.DontExpire)
                {
                    JWK.Nbf = (long)(new DateTime(2099, 1, 1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }


                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.FromXmlString(StringHelpers.Base64Decode(secret.SigningPublicKey));
                    var RSAParams = RSA.ExportParameters(false);
                    JWK.E = WebEncoders.Base64UrlEncode(RSAParams.Exponent);
                    JWK.N = WebEncoders.Base64UrlEncode(RSAParams.Modulus);
                }

                JWKs.Keys.Add(JWK);
            }

            return JWKs;
        }

        /// <summary>
        /// Gets OpenID Configuration for a particular application in within the context of a business tenant.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{TenantID}/{ApplicationID}/.Well-Known/OpenId-Configuration")]
        public async Task<ActionResult<OpenIdConfiguration>> GetOpenIdConfiguration(string TenantID, string ApplicationID)
        {
            if (String.IsNullOrEmpty(ApplicationID) || string.IsNullOrEmpty(TenantID) || !await DataContext.BusinessApplication.AnyAsync(c => c.ID == ApplicationID) || !await DataContext.Business.AnyAsync(c => c.ID == TenantID))
            {
                return BadRequest("OpenID Configuration not found.");
            }

            var BusinessApplication = await DataContext.BusinessApplication
                .Include(c => c.BusinessApplicationRequestedPermissions).ThenInclude(c => c.BusinessPermission)
                .FirstAsync(c => c.ID == ApplicationID);


            var OpenIdConfiguration = new OpenIdConfiguration
            {
                ClaimsSupported = new List<string>(),
                ScopesSupported = new List<string>(),
                SubjectTypesSupported = new List<string>(),
                IdTokenSigningAlgValuesSupported = new List<string>(),
                TokenEndpointAuthMethodsSupported = new List<string>(),
                TokenEndpoint = "https://fenixalliance.com.co/api/v2/oauth2/Token",
                JwksUri = $"https://fenixalliance.com.co/api/v2/oauth2/{ApplicationID}/keys",
                EndSessionEndpoint = "https://fenixalliance.com.co/api/v2/oauth2/Token/Expire",
                Issuer = $"https://fenixalliance.com.co/api/v2/oauth2/{TenantID}/{ApplicationID}/",
                AuthorizationEndpoint = $"https://fenixalliance.com.co/api/v2/oauth2/{TenantID}/{ApplicationID}/Authorize"
            };



            OpenIdConfiguration.ClaimsSupported.Add("name");
            OpenIdConfiguration.ClaimsSupported.Add("email");
            OpenIdConfiguration.ClaimsSupported.Add("phone");
            OpenIdConfiguration.ClaimsSupported.Add("avatar");
            OpenIdConfiguration.ClaimsSupported.Add("country");
            OpenIdConfiguration.ClaimsSupported.Add("oid");
            OpenIdConfiguration.ClaimsSupported.Add("iat");
            OpenIdConfiguration.ClaimsSupported.Add("aud");
            OpenIdConfiguration.ClaimsSupported.Add("nbf");
            OpenIdConfiguration.ClaimsSupported.Add("exp");
            OpenIdConfiguration.ClaimsSupported.Add("aud");
            OpenIdConfiguration.ClaimsSupported.Add("acr");
            OpenIdConfiguration.ClaimsSupported.Add("ver");
            OpenIdConfiguration.ClaimsSupported.Add("nonce");

            OpenIdConfiguration.TokenEndpointAuthMethodsSupported.Add("client_secret_post");
            OpenIdConfiguration.TokenEndpointAuthMethodsSupported.Add("client_secret_basic");

            OpenIdConfiguration.SubjectTypesSupported.Add("pairwise");

            OpenIdConfiguration.IdTokenSigningAlgValuesSupported.Add("RS256");


            foreach (var RequiredPermission in BusinessApplication.BusinessApplicationRequestedPermissions)
            {
                OpenIdConfiguration.ScopesSupported.Add(RequiredPermission.BusinessPermissionID);
            }

            return OpenIdConfiguration;
        }
    }
}