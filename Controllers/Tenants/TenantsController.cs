using FenixAlliance.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.Data.Access.DataAccess;
using FenixAlliance.Data.Access.Helpers;
using FenixAlliance.Models.Binders;
using FenixAlliance.Models.Binders.Social;
using FenixAlliance.Models.DTOs.Components.Businesses;
using FenixAlliance.Models.DTOs.Responses;
using FenixAlliance.Models.DTOs.Responses.Base;
using FenixAlliance.Models.DTOs.Responses.Business;
using FenixAlliance.Models.DTOs.Components.Social;

namespace FenixAlliance.API.v2.Controllers.Businesses
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiExplorerSettings(GroupName = "Tenants")]
    [Produces("application/json", new string[] { "application/xml" })]
    [Consumes("application/json", new string[] { "application/xml" })]
    public class TenantsController : ControllerBase
    {

        private readonly ABMContext _context;
        public AccountUsersHelpers AccountTools { get; set; }
        public AccountGraphHelpers AccountGraphTools { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;
        private readonly BlobStorageDataAccessClient DataTools;
        private readonly StoreHelpers StoreHelpers;
        private BusinessHelpers BusinessTools;
        private readonly BusinessDataAccessClient BusinessDataAccess;

        public TenantsController(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _env = hostingEnvironment;
            StoreHelpers = new StoreHelpers(_context);
            BusinessTools = new BusinessHelpers(context);
            DataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(context);
            AccountGraphTools = new AccountGraphHelpers(_context, _configuration);
            BusinessDataAccess = new BusinessDataAccessClient(_context, _configuration, _env);

        }

        [HttpGet("Current")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Tenant>> GetMyCurrentTenant()
        {

            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);

            return Ok(Response.Tenant);
        }

        [HttpGet("{TenantID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetTenant(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }

        [HttpGet("{TenantID}/Wallet")]
        [Produces("application/json")]

        public async Task<ActionResult> GetTenantWallet(string TenantID)
        {

            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }

        [Produces("application/json")]
        [HttpGet("{TenantID}/SocialProfile")]

        public async Task<ActionResult> GetTenantSocialProfile(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }


        [HttpGet("{TenantID}/Cart")]
        [Produces("application/json")]
        public async Task<ActionResult> GetTenantCart(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }


        [HttpGet("{TenantID}/Enrollments")]
        [Produces("application/json")]
        public async Task<ActionResult> GetTenantEnrollments(string TenantID)
        {

            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }


        [Produces("application/json")]
        [HttpGet("{TenantID}/Enrollments/{EnrollmentID}")]
        public async Task<ActionResult> GetTenantEnrollment(string TenantID, string EnrollmentID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }



        [Produces("application/json")]
        [HttpGet("{TenantID}/Enrollments/{EnrollmentID}/Licences")]
        public async Task<ActionResult> GetEnrollmentLicenses(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }



        [Produces("application/json")]
        [HttpGet("{TenantID}/Licenses")]
        public async Task<ActionResult> GetTenantLicenses(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }

        [HttpGet("{TenantID}/Select")]
        public async Task<ActionResult<ClientApplication>> SwitchTenant(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }

        // GET: Business/SelectBusiness/Select/5
        [HttpGet("{TenantID}/Select")]
        public async Task<IActionResult> Select(string TenantID,string BackTo, bool EnableRedirect = true)
        {
            // Return Redirect back To
            string Referer = Request.Headers["Referer"];

            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            if (TenantID == null)
                return NotFound();


            if (TenantID == Response.Holder.CurrentTenantID)
                return Ok();

            var GUID = AccountTools.GetActiveDirectoryGUID(User);
            // If no BPR, not authorized.
            var BPR = await _context.BusinessProfileRecord.AsNoTracking().Where(c => c.AllianceIDHolderGUID == GUID && c.BusinessID == TenantID).FirstAsync();
            if (BPR == null)
                return Unauthorized();

            var Tenant = await _context.AllianceIDHolder
                // Load Business Owner Data
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business)
                  .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                    .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.GUID == GUID).FirstOrDefaultAsync();

            // Load requested business data
            var business = await _context.Business.AsNoTracking().FirstOrDefaultAsync(m => m.ID == TenantID);

            if (BPR.IsDisabled == true)
                return Unauthorized();

            //// Search for User's Businesses Employee Ownerships to determine if the user can select the requested business
            if (BPR.IsBusinessOwner || BusinessDataAccess.ResolveRequestedAccess(Tenant, TenantID, null, new List<string>() { "business_owner" }))
            {
                Tenant.SelectedBusinessID = business.ID;
                Tenant.SelectedBusinessAs = "Owner";
            }

            // Search for User's Businesses Employee Records to determine if the user can select the requested business
            if (BusinessDataAccess.ResolveRequestedAccess(Tenant, TenantID, null, new List<string>() { "business_employee" }))
            {
                Tenant.SelectedBusinessID = business.ID;
                Tenant.SelectedBusinessAs = "Employee";
            }

            if (Tenant.SelectedBusinessID != TenantID)
                return Unauthorized();

            // Save changes
            try
            {
                _context.Update(Tenant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();


            }

            if(EnableRedirect == true && !(String.IsNullOrEmpty(BackTo) && String.IsNullOrEmpty(BackTo)))
            {
                if (BackTo.ToUpperInvariant().Contains("/MyBusiness/Create".ToUpperInvariant()))
                    return Redirect("https://fenixalliance.com.co/Business/MyBusiness/Edit?RecentlyBaked=true");

                return Redirect(BackTo ?? Referer);
            }
            else
            {
                return Ok();
            }

        }

        // GET: Business/SelectBusiness/Select/5       
        [HttpGet("BackToHolder")]
        public async Task<IActionResult> DeSelect(string BackTo,bool EnableRedirect = true)
        {
            string Referer = Request.Headers["Referer"];
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);


            var GUID = Response.Holder.ID;
            var Tenant = await _context.AllianceIDHolder.Where(e => e.GUID == GUID).FirstOrDefaultAsync();

            Tenant.SelectedBusinessID = null;
            Tenant.SelectedBusinessAs = null;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(Tenant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest();
                }
            }

            if(EnableRedirect == true || !String.IsNullOrEmpty(BackTo))
            {
                var RedirectTo = BackTo ?? Referer;
                return Redirect(RedirectTo);
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet("{TenantID}/AppAuthorization")]
        public async Task<ActionResult<ClientApplication>> AppAuthorization(string TenantID)
        {
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);


            return Ok(Response.Tenant);
        }


        [HttpGet("{TenantID}/Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Notification>>> GetBusinessNotifications(string TenantID)
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);

            var EndUser = await _context.AllianceIDHolder.AsNoTracking().Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Notifications).FirstOrDefaultAsync(m => m.GUID == Response.Holder.ID);

            return Ok(NotificationBinder.ToDTO(EndUser.SelectedBusiness.BusinessSocialProfile.Notifications));
        }

        [HttpGet("{TenantID}/UserEnrollments")]
        [ProducesResponseType(typeof(List<Enrollment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BusinessEnrollmentsResponse>> GetBusinessUsers(string TenantID)
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<BusinessEnrollmentsResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User, TenantID)));
            if (Response == null || !Response.Status.Success || Response.Holder == null || Response.Tenant == null)
                return Unauthorized(Response.Status);

            var Holder = await _context.AllianceIDHolder
                // Include Business Profile Records
                .Include(b => b.Country)
                // Include Business Profile Records
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.Country)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(b => b.BusinessSegment)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessIndustry)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessSocialProfile)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessPartnerProfile)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessProfileRecords)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.StartupsPartnerProfile)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                    .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.GUID == Response.Holder.ID).FirstOrDefaultAsync().ConfigureAwait(false);

            BusinessDataAccess.ResolveRequestedAccess(Holder, null, new List<string>() { "business_owner" });


            try
            {
                Response.Enrollments = new List<Enrollment>();
                foreach (var item in Holder.SelectedBusiness.BusinessProfileRecords)
                {
                    var BRP = new Enrollment()
                    {
                        ID = item.ID,
                        AID = item.BusinessID,
                        SPID = item.Business.BusinessSocialProfile.ID,
                        PName = item.Business.BusinessName,
                        Avtr_URL = item.Business.BusinessAvatarURL ,
                        Fb_URL = item.Business.BusinessAvatarURL,
                        Twtr_URL = item.Business.BusinessAvatarURL,
                        LnIn_URL = item.Business.BusinessAvatarURL,
                        Web_URL = item.Business.BusinessAvatarURL,
                        CO = item.Business.Country.Name,
                        CO_F_URL = item.Business.Country.CountryFlagUrl,
                        AsAdmin = BusinessDataAccess.ResolveRequestedAccess(Holder, null, new List<string>() { "business_admin" }),
                        AsOwner = BusinessDataAccess.ResolveRequestedAccess(Holder, null, new List<string>() { "business_owner" }),
                        // TODO: Add Guest Property
                        AsGuest = BusinessDataAccess.ResolveRequestedAccess(Holder, null, new List<string>() { "business_guest" }),
                    };
                    Response.Enrollments.Add(BRP);
                }
            }
            catch (Exception ex)
            {
                Response.Status.Success = false;
                Response.Status.Error.ID = $"E012 - {ex.ToString()}";
                Response.Status.Error.Description = "There was a problem loading your current business' users.";
            }

            return Ok(Response.Enrollments);
        }
    }
}