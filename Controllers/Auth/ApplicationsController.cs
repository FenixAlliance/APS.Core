using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Interfaces.Services;
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
        public ABMContext DataContext { get; set; }
        public IAuthService AuthService { get; set; }
        public IStoreService StoreHelpers { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostEnvironment Environment { get; set; }
        public IHolderService HolderService { get; set; }
        public ITenantService TenantService { get; set; }
        public IStorageService StorageService { get; set; }

        public ApplicationsController(ABMContext DataContext, IConfiguration Configuration, IHostEnvironment Environment,
            IStoreService StoreHelpers, ITenantService TenantService, IHolderService HolderService, IAuthService AuthService, IStorageService StorageService)
        {
            this.AuthService = AuthService;
            this.DataContext = DataContext;
            this.Environment = Environment;
            this.StoreHelpers = StoreHelpers;
            this.Configuration = Configuration;
            this.HolderService = HolderService;
            this.TenantService = TenantService;
            this.StorageService = StorageService;
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