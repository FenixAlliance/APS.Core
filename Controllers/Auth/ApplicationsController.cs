using FenixAlliance.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.Data.Access.DataAccess;
using FenixAlliance.Data.Access.Helpers;


namespace FenixAlliance.API.v2.Controllers.Developers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiExplorerSettings(GroupName = "IAM")]
    [Produces("application/json", new string[] { "application/xml" })]
    [Consumes("application/json", new string[] { "application/xml" })]
    public class ApplicationsController : ControllerBase
    {
        private readonly ABMContext _context;
        public AccountUsersHelpers AccountTools { get; set; }
        public AccountGraphHelpers AccountGraphTools { get; set; }
        private readonly IHostEnvironment _env;
        private readonly StoreHelpers StoreHelpers;
        private readonly IConfiguration _configuration;
        private readonly BlobStorageDataAccessClient DataTools;
        private readonly BusinessDataAccessClient BusinessDataAccess;

        public ApplicationsController(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _env = hostingEnvironment;
            AccountTools = new AccountUsersHelpers(context);
            AccountGraphTools = new AccountGraphHelpers(_context, _configuration);
            StoreHelpers = new StoreHelpers(_context);
            DataTools = new BlobStorageDataAccessClient();
            BusinessDataAccess = new BusinessDataAccessClient(_context, _configuration, _env);
        }

        [HttpGet("{AppID}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetApplication(string AppID)
        {

            return Ok();
        }

        [HttpGet("{AppID}/RequiredPermissions")]
        [Produces("application/json")]
        public async Task<ActionResult> GetRequiredPermissions(string AppID)
        {

            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedPermissions")]
        public async Task<ActionResult> GetGrantedTenantPermissions(string AppID, string TenantID)
        {

            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedRoles")]
        public async Task<ActionResult> GetGrantedTenantRoles(string AppID, string TenantID)
        {

            return Ok();
        }


        [Produces("application/json")]
        [HttpGet("{AppID}/GrantedRoles/{SecurityRoleID}/GrantedPermissions")]

        public async Task<ActionResult> GetGrantedEnrollmentPermissions(string AppID, string SecurityRoleID, string EnrollmentID)
        {

            return Ok();
        }

    }
}