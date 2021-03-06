using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Clients;
using FenixAlliance.ABM.Data.Access.Helpers;
using FenixAlliance.APS.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Controllers.Auth
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiExplorerSettings(GroupName = "IAM")]
    [Produces("application/json", "application/xml")]
    [Consumes("application/json", "application/xml")]
    public class ApplicationsController : ControllerBase
    {
        public ABMContext DataContext { get; }
        public StoreHelpers StoreHelpers { get; }
        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }
        public TenantHelpers TenantHelpers { get; }
        public AccountUsersHelpers AccountUsersHelpers { get; }
        public AccountGraphHelpers AccountGraphHelpers { get; }
        public TenantDataAccessClient BusinessDataAccess { get; }
        public ApiAuthorizationHelpers ApiAuthorizationHelpers { get; }

        public ApplicationsController(ABMContext context, IConfiguration Configuration, IHostEnvironment Environment,
            StoreHelpers StoreHelpers, TenantHelpers TenantHelpers, AccountUsersHelpers AccountUsersHelpers,
            AccountGraphHelpers AccountGraphHelpers, TenantDataAccessClient TenantDataAccessClient, ApiAuthorizationHelpers ApiAuthorizationHelpers)
        {
            this.DataContext = context;
            this.Environment = Environment;
            this.StoreHelpers = StoreHelpers;
            this.TenantHelpers = TenantHelpers;
            this.Configuration = Configuration;
            this.AccountUsersHelpers = AccountUsersHelpers;
            this.AccountGraphHelpers = AccountGraphHelpers;
            this.BusinessDataAccess = TenantDataAccessClient;
            this.ApiAuthorizationHelpers = ApiAuthorizationHelpers;
        }


        [HttpGet("{AppID}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetApplication(string AppID)
        {
            await Task.Delay(1000);
            return Ok();
        }

        [HttpGet("{AppID}/RequiredPermissions")]
        [Produces("application/json")]
        public async Task<ActionResult> GetRequiredPermissions(string AppID)
        {
            await Task.Delay(1000);
            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedPermissions")]
        public async Task<ActionResult> GetGrantedTenantPermissions(string AppID, string TenantID)
        {
            await Task.Delay(1000);
            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedRoles")]
        public async Task<ActionResult> GetGrantedTenantRoles(string AppID, string TenantID)
        {
            await Task.Delay(1000);
            return Ok();
        }


        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedRoles/{SecurityRoleID}/GrantedPermissions")]

        public async Task<ActionResult> GetGrantedEnrollmentPermissions(string AppID, string SecurityRoleID, string EnrollmentID)
        {
            await Task.Delay(1000);
            return Ok();
        }

    }
}