using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Models.DTOs.Components.Holders;
using FenixAlliance.ABM.Models.DTOs.Components.Social;
using FenixAlliance.ABM.Models.DTOs.Components.Tenants;
using FenixAlliance.ABM.Models.DTOs.Responses;
using FenixAlliance.ABM.Models.DTOs.Responses.Business;
using FenixAlliance.ABM.Models.Mappers.Social;
using FenixAlliance.APS.Core.DataAccess;
using FenixAlliance.APS.Core.DataHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.Controllers.Holders
{
    [ApiController]
    [Route("api/v2/Me")]
    [ApiExplorerSettings(GroupName = "Holders")]
    [Produces("application/json", "application/xml")]
    [Consumes("application/json", "application/xml" )]
    public class HolderController : ControllerBase
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

        public HolderController(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
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
        /// Get the current user profile
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Holder), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetMe()
        {

            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse.Holder);
        }


        [HttpGet("Follows")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<FollowRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APISocialActivityResponse>> GetMyFollows(int PageSize = 25, int PageIndex = 0)
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APISocialActivityResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            // Get Alliance ID Social Profile from DB
            var Tenant = await AccountUsersHelpers.GetTenantSocialProfileAsync(APIResponse.Holder.ID);


            APIResponse.Pagination.PageIndex = PageIndex;
            APIResponse.Pagination.PageSize = PageSize;

            APIResponse.FollowRecords = new List<FollowRecord>();

            foreach (var item in Tenant.SocialProfile.Follows)
            {
                var FA = new FollowRecord()
                {
                    ID = item.ID,
                    FollowerID = item.FollowerSocialProfileID,
                    FollowedID = item.FollowedSocialProfileID,
                    Alerts = item.EnableNotifications,
                    Type = item.FollowRecordType.ToString(),
                };

                // Determine if CUSAR.SPID Follows => FA.SPID and then assign to => FID
                var Follow = Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == APIResponse.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID);

                Follow ??= Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == APIResponse.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID);

                FA.ID = Follow?.ID;

                // Determine if Follow Exists FID
                // Determine if FA.SPID follows CUSAR.SPID and then assign to => FBID
                var FollowBack = Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowerID && c.FollowedSocialProfileID == APIResponse.Holder.ID);
                FollowBack ??= Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowerID && c.FollowedSocialProfileID == APIResponse.Holder.ID);
                // FA.FollowBackID = FollowBack?.ID;
                APIResponse.FollowRecords.Add(FA);
            }


            return Ok(APIResponse.FollowRecords);
        }

        [HttpGet("Follows")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<FollowRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APISocialActivityResponse>> GetMyFollowers(int PageSize = 25, int PageIndex = 0)
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APISocialActivityResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            // Get Alliance ID Social Profile from DB
            var Tenant = await AccountUsersHelpers.GetTenantSocialProfileAsync(APIResponse.Holder.ID);

            APIResponse.Pagination.PageIndex = PageIndex;
            APIResponse.Pagination.PageSize = PageSize;

            APIResponse.FollowRecords = new List<FollowRecord>();

            foreach (var item in Tenant.SocialProfile.Followers)
            {
                var FollowRecord = new FollowRecord()
                {
                    ID = item.ID,
                    FollowerID = item.FollowerSocialProfileID,
                    FollowedID = item.FollowedSocialProfileID,
                    Alerts = item.EnableNotifications,
                    Type = item.FollowRecordType.ToString(),
                };
                // Determine if CUSAR.SPID Follows => FA.SPID and then assing to => FID
                var Follow = Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == APIResponse.Holder.ID && c.FollowedSocialProfileID == FollowRecord.FollowedID);
                FollowRecord.ID = Follow?.ID;
                // Determine if Follow Exists FID
                // Determine if FA.SPID follows CUSAR.SPID and then assign to => FBID
                var FollowBack = Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FollowRecord.FollowedID && c.FollowedSocialProfileID == APIResponse.Holder.ID);
                FollowRecord.ID = Follow?.ID;
                // FA.FollowBackID = FollowBack?.ID;
                APIResponse.FollowRecords.Add(FollowRecord);
            }


            return Ok(APIResponse.FollowRecords);
        }

        [HttpGet("Businesses")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Enrollment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Enrollment>>> GetMyBusinesses()
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<BusinessEnrollmentsResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            var Tenant = await DataContext.AccountHolder
                // Include Business Profile Records
                .Include(b => b.Country)
                .Include(b => b.SocialProfile)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.Country)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(b => b.BusinessSegment)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessIndustry)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessSocialProfile)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessPartnerProfile)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.BusinessProfileRecords)
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.StartupsPartnerProfile)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessPermissionGrants)
                    .ThenInclude(c => c.GrantorBusinessProfileRecord).ThenInclude(c => c.BusinessPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.ID == APIResponse.Holder.ID).FirstOrDefaultAsync().ConfigureAwait(false);



            APIResponse.Enrollments = new List<Enrollment>();


            foreach (var item in Tenant.BusinessProfileRecords)
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
                    AsAdmin = BusinessDataAccess.ResolveRequestedAccess(Tenant, null, new List<string>() { "business_admin" }),
                    AsOwner = BusinessDataAccess.ResolveRequestedAccess(Tenant, null, new List<string>() { "business_owner" }),
                    // TODO: Add Guest Property
                    AsGuest = BusinessDataAccess.ResolveRequestedAccess(Tenant, null, new List<string>() { "business_guest" }),
                };
                APIResponse.Enrollments.Add(BRP);
            }

            return Ok(APIResponse.Enrollments);
        }

        [HttpGet("Library")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetMyLibrary()
        {
            // Get Header
            APIResponse APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User).ConfigureAwait(false)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse);
        }


        [HttpGet("Addresses")]
        [Produces("application/json")]
        public async Task<ActionResult<APIResponse>> GetMyAddresses()
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse);
            }

            return Ok(APIResponse);
        }

        [HttpGet("Notifications")]
        [Produces("application/json")]
        public async Task<ActionResult<List<Notification>>> GetMyNotifications()
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            var EndUser = await DataContext.AccountHolder.Include(c => c.SocialProfile).ThenInclude(c => c.Notifications).FirstOrDefaultAsync(m => m.ID == APIResponse.Holder.ID);

            return Ok(NotificationBinder.ToDTO(EndUser.SocialProfile.Notifications));
        }


        [HttpGet("Settings")]
        [Produces("application/json")]
        public async Task<ActionResult<APIResponse>> GetMySettings()
        {
            // Get Header
            var APIResponse = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(DataContext, HttpContext, Request, AccountUsersHelpers, User)));
            if (APIResponse == null || !APIResponse.Status.Success || APIResponse.Holder == null)
            {
                return Unauthorized(APIResponse?.Status);
            }

            return Ok(APIResponse);
        }
    }
}