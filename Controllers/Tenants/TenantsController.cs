using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Interfaces.Services;
using FenixAlliance.ABM.Models.DTOs.Components.Social;
using FenixAlliance.ABM.Models.DTOs.Components.Tenants;
using FenixAlliance.ABM.Models.DTOs.Responses;
using FenixAlliance.ABM.Models.DTOs.Responses.Business;
using FenixAlliance.ABM.Models.Mappers.Social;
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

namespace FenixAlliance.APS.Core.Controllers.Tenants
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiExplorerSettings(GroupName = "Tenants")]
    [Produces("application/json", "application/xml")]
    [Consumes("application/json", "application/xml")]
    public class TenantsController : ControllerBase
    {
        public ABMContext DataContext { get; set; }
        public IAuthService AuthService { get; set; }
        public IStoreService StoreHelpers { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostEnvironment Environment { get; set; }
        public IHolderService HolderService { get; set; }
        public ITenantService TenantService { get; set; }
        public IStorageService StorageService { get; set; }

        public TenantsController(ABMContext DataContext, IConfiguration Configuration, IHostEnvironment Environment,
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


        [HttpGet("Current")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Tenant>> GetMyCurrentTenant()
        {

            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }

        [HttpGet("{TenantID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetTenant(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }

        [HttpGet("{TenantID}/Wallet")]
        [Produces("application/json")]

        public async Task<ActionResult> GetTenantWallet(string TenantID)
        {

            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }

        [Produces("application/json")]
        [HttpGet("{TenantID}/SocialProfile")]

        public async Task<ActionResult> GetTenantSocialProfile(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }


        [HttpGet("{TenantID}/Cart")]
        [Produces("application/json")]
        public async Task<ActionResult> GetTenantCart(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }


        [HttpGet("{TenantID}/Enrollments")]
        [Produces("application/json")]
        public async Task<ActionResult> GetTenantEnrollments(string TenantID)
        {

            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }


        [Produces("application/json")]
        [HttpGet("{TenantID}/Enrollments/{EnrollmentID}")]
        public async Task<ActionResult> GetTenantEnrollment(string TenantID, string EnrollmentID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }



        [Produces("application/json")]
        [HttpGet("{TenantID}/Enrollments/{EnrollmentID}/Licenses")]
        public async Task<ActionResult> GetEnrollmentLicenses(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }



        [Produces("application/json")]
        [HttpGet("{TenantID}/Licenses")]
        public async Task<ActionResult> GetTenantLicenses(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }

        [HttpGet("{TenantID}/Select")]
        public async Task<ActionResult<ClientApplication>> SwitchTenant(string TenantID)
        {
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(
                await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }

        // GET: Business/SelectBusiness/Select/5
        [HttpGet("{TenantID}/Select")]
        public async Task<IActionResult> Select(string TenantID, string BackTo, bool EnableRedirect = true)
        {
            // Return Redirect back To
            // string Referer = Request.Headers["Referer"];

            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(
                await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));

            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            if (TenantID == null)
            {
                return NotFound();
            }

            if (TenantID == APIResponse.Holder.CurrentTenantID)
            {
                return Ok();
            }

            var GUID = HolderService.GetActiveDirectoryGUID(User);

            // If no BPR, not authorized.
            var BPR = await DataContext.BusinessProfileRecord.AsNoTracking().Where(c => c.AccountHolderID == GUID && c.BusinessID == TenantID).FirstAsync();
            if (BPR == null)
            {
                return Unauthorized();
            }

            var Tenant = await DataContext.AccountHolder
                // Load Business Owner Data
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business)
                  .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                    .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.ID == GUID).FirstOrDefaultAsync();

            // Load requested business data
            var business = await DataContext.Business.AsNoTracking().FirstOrDefaultAsync(m => m.ID == TenantID);

            if (BPR.IsDisabled)
            {
                return Unauthorized();
            }

            //// Search for User's Businesses Employee Ownerships to determine if the user can select the requested business
            if (BPR.IsBusinessOwner || TenantService.ResolveRequestedAccess(Tenant, TenantID, null, new List<string>() { "business_owner" }))
            {
                Tenant.SelectedBusinessID = business.ID;
                Tenant.SelectedBusinessAs = "Owner";
            }

            // Search for User's Businesses Employee Records to determine if the user can select the requested business
            if (TenantService.ResolveRequestedAccess(Tenant, TenantID, null, new List<string>() { "business_employee" }))
            {
                Tenant.SelectedBusinessID = business.ID;
                Tenant.SelectedBusinessAs = "Employee";
            }

            if (Tenant.SelectedBusinessID != TenantID)
            {
                return Unauthorized();
            }

            // Save changes
            try
            {
                DataContext.Update(Tenant);
                await DataContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();


            }

            if (EnableRedirect && !(string.IsNullOrEmpty(BackTo) && string.IsNullOrEmpty(BackTo)))
            {
                if (BackTo.ToUpperInvariant().Contains("/MyBusiness/Create".ToUpperInvariant()))
                {
                    return Redirect("https://fenixalliance.com.co/Business/MyBusiness/Edit?RecentlyBaked=true");
                }

                return Redirect(BackTo);
            }
            else
            {
                return Ok();
            }

        }

        // GET: Business/SelectBusiness/Select/5       
        [HttpGet("BackToHolder")]
        public async Task<IActionResult> DeSelect(string BackTo, bool EnableRedirect = true)
        {
            string Referer = Request.Headers["Referer"];
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            var GUID = APIResponse.Holder.ID;
            var Tenant = await DataContext.AccountHolder.Where(e => e.ID == GUID).FirstOrDefaultAsync();

            Tenant.SelectedBusinessID = null;
            Tenant.SelectedBusinessAs = null;

            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(Tenant).State = EntityState.Modified;
                    await DataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest();
                }
            }

            if (EnableRedirect || !string.IsNullOrEmpty(BackTo))
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
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Tenant);
        }


        [HttpGet("{TenantID}/Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Notification>>> GetBusinessNotifications(string TenantID)
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            var EndUser = await DataContext.AccountHolder.AsNoTracking().Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Notifications).FirstOrDefaultAsync(m => m.ID == APIResponse.Holder.ID);

            return Ok(NotificationBinder.ToDTO(EndUser.SelectedBusiness.BusinessSocialProfile.Notifications));
        }

        [HttpGet("{TenantID}/UserEnrollments")]
        [ProducesResponseType(typeof(List<Enrollment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BusinessEnrollmentsResponse>> GetBusinessUsers(string TenantID)
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<BusinessEnrollmentsResponse>(JsonConvert.SerializeObject(await AuthService.BindAPIBaseResponse(DataContext, HttpContext, Request, HolderService, User, TenantID)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null || APIResponse.Tenant == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            var Holder = await DataContext.AccountHolder
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
                .Where(e => e.ID == APIResponse.Holder.ID).FirstOrDefaultAsync().ConfigureAwait(false);

            TenantService.ResolveRequestedAccess(Holder, null, new List<string>() { "business_owner" });


            try
            {
                APIResponse.Enrollments = new List<Enrollment>();
                foreach (var item in Holder.SelectedBusiness.BusinessProfileRecords)
                {
                    var BRP = new Enrollment()
                    {
                        ID = item.ID,
                        AID = item.BusinessID,
                        SPID = item.Business.BusinessSocialProfile.ID,
                        PName = item.Business.BusinessName,
                        Avtr_URL = item.Business.BusinessAvatarURL,
                        Fb_URL = item.Business.BusinessAvatarURL,
                        Twtr_URL = item.Business.BusinessAvatarURL,
                        LnIn_URL = item.Business.BusinessAvatarURL,
                        Web_URL = item.Business.BusinessAvatarURL,
                        CO = item.Business.Country.Name,
                        CO_F_URL = item.Business.Country.CountryFlagUrl,
                        AsAdmin = TenantService.ResolveRequestedAccess(Holder, null, new List<string>() { "business_admin" }),
                        AsOwner = TenantService.ResolveRequestedAccess(Holder, null, new List<string>() { "business_owner" }),
                        // TODO: Add Guest Property
                        AsGuest = TenantService.ResolveRequestedAccess(Holder, null, new List<string>() { "business_guest" }),
                    };
                    APIResponse.Enrollments.Add(BRP);
                }
            }
            catch (Exception ex)
            {
                APIResponse.Status.Success = false;
                APIResponse.Status.Error.ID = $"E012 - {ex}";
                APIResponse.Status.Error.Description = "There was a problem loading your current business' users.";
            }

            return Ok(APIResponse.Enrollments);
        }
    }
}