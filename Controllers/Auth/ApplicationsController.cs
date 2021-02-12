using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Clients;
using FenixAlliance.ABM.Data.Access.Helpers;
using FenixAlliance.APS.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        public IHostEnvironment HostEnvironment { get; }
        public TenantHelpers TenantHelpers { get; }
        public AccountUsersHelpers AccountUsersHelpers { get; }
        public AccountGraphHelpers AccountGraphHelpers { get; }
        public TenantDataAccessClient BusinessDataAccess { get; }
        public BlobStorageDataAccessClient StorageDataAccessClient { get; }

        public ApplicationsController(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            DataContext = context;
            Configuration = configuration;
            HostEnvironment = hostingEnvironment;
            StoreHelpers = new StoreHelpers(DataContext);
            TenantHelpers = new TenantHelpers(context);
            AccountUsersHelpers = new AccountUsersHelpers(context);
            AccountGraphHelpers = new AccountGraphHelpers(DataContext, Configuration);
            BusinessDataAccess = new TenantDataAccessClient(DataContext, Configuration, HostEnvironment);
            StorageDataAccessClient = new BlobStorageDataAccessClient();
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