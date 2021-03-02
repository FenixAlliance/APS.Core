using FenixAlliance.ABM.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Interfaces
{
    /// <summary>
    /// This interface exposes required methods to
    /// </summary>
    public interface IAuthorizationService
    {

        IConfiguration Configuration { get; set; }
        IHostEnvironment Environment { get; set; }
        HttpClient HttpClient { get; set; }
        ABMContext DataContext { get; set; }

        string PublicKey { get; set; }
        string PrivateKey { get; set; }
        string Scopes { get; set; }
        string BaseEndpoint { get; set; }
        string AuthEndpoint { get; set; }
        bool IsAuthorized { get; set; }

        /// <summary>
        /// Reads properties from suitesettings.json and attempts to make a handshaking request to the defined Base API Set.
        /// </summary>
        /// <returns></returns>
        Task AuthorizeClient();
        /// <summary>
        /// This method retieves the Public and Private keys through OAuth endpoints at /api/v2/OAuth2/Token
        /// </summary>
        /// <param name="RequestedScopes">The list of required scopes by this particular client.</param>
        /// <returns></returns>
        Task AuthorizeClient(List<string> RequestedScopes);

        /// <summary>
        /// Does the account holder has to the queried Business Tenant?
        /// </summary>
        /// <param name="BusinessTenantID"></param>
        /// <returns></returns>
        Task<bool> IsHolderAuthorized(string BusinessTenantID);
        /// <summary>
        /// Returns wether or not this user is an admin for the selected Business Id.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsAdmin(ClaimsPrincipal User);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BusinessTenantID"></param>
        /// <returns></returns>
        Task<bool> IsAdmin(string BusinessTenantID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="User"></param>
        /// <param name="BusinessTenantID"></param>
        /// <param name="RequiredRoles"></param>
        /// <param name="RequiredPermissions"></param>
        /// <returns></returns>
        Task<bool> IsAdmin(ClaimsPrincipal User, string BusinessTenantID, List<string> RequiredRoles, List<string> RequiredPermissions);


    }
}
