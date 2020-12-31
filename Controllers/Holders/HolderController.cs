using Microsoft.AspNetCore.Http;
using FenixAlliance.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.Data.Access.DataAccess;
using FenixAlliance.Data.Access.Helpers;
using FenixAlliance.Models.Binders;
using FenixAlliance.Models.Binders.Social;
using FenixAlliance.Models.DTOs.Components.Businesses;
using FenixAlliance.Models.DTOs.Components.Social;
using FenixAlliance.Models.DTOs.Responses;
using FenixAlliance.Models.DTOs.Responses.Base;
using FenixAlliance.Models.DTOs.Responses.Business;
using FollowRecord = FenixAlliance.Models.DTOs.Components.Social.FollowRecord;
using FenixAlliance.Models.DTOs.Components.ID;
using Microsoft.AspNetCore.Cors;

namespace FenixAlliance.API.v2.Controllers.ID.Me
{
    [ApiController]
    [Route("api/v2/Me")]
    [ApiExplorerSettings(GroupName = "Holders")]
    [Produces("application/json", new string[] { "application/xml" })]
    [Consumes("application/json", new string[] { "application/xml" })]
    public class HolderController : ControllerBase
    {

        private readonly ABMContext _context;
        public AccountUsersHelpers AccountTools { get; set; }
        public AccountGraphHelpers AccountGraphTools { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;
        private readonly BlobStorageDataAccessClient DataTools;
        private readonly BusinessDataAccessClient BusinessDataAccess;
        private readonly StoreHelpers StoreHelpers;

        public HolderController(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
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

            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            return Ok(Response.Holder);
        }


        [HttpGet("Follows")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<FollowRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APISocialActivityResponse>> GetMyFollows(int PageSize = 25, int PageIndex = 0)
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APISocialActivityResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            // Get Alliance ID Social Profile from DB
            var Tenant = await AccountTools.GetTenantSocialProfileAsync(Response.Holder.ID);


            Response.Pagination.PageIndex = PageIndex;
            Response.Pagination.PageSize = PageSize;

            Response.FollowRecords = new List<FollowRecord>();

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

                // Deterimine if CUSAR.SPID Follows => FA.SPID and then assing to => FID
                var Follow = (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == Response.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID);

                Follow = (Follow == null) ? (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == Response.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID) : Follow;

                FA.ID = Follow?.ID;

                // Deterimine if Follow Exsists FID
                // Deterimine if FA.SPID follows CUSAR.SPID and then assing to => FBID
                var FollowBack = (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowerID && c.FollowedSocialProfileID == Response.Holder.ID);
                FollowBack = (FollowBack == null) ? (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowerID && c.FollowedSocialProfileID == Response.Holder.ID) : FollowBack;
                // FA.FollowBackID = FollowBack?.ID;
                Response.FollowRecords.Add(FA);
            }


            return Ok(Response.FollowRecords);
        }

        [HttpGet("Follows")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<FollowRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APISocialActivityResponse>> GetMyFollowers(int PageSize = 25, int PageIndex = 0)
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APISocialActivityResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            // Get Alliance ID Social Profile from DB
            var Tenant = await AccountTools.GetTenantSocialProfileAsync(Response.Holder.ID);

            Response.Pagination.PageIndex = PageIndex;
            Response.Pagination.PageSize = PageSize;

            Response.FollowRecords = new List<FollowRecord>();

            foreach (var item in Tenant.SocialProfile.Followers)
            {
                var FA = new FollowRecord()
                {
                    ID = item.ID,
                    FollowerID = item.FollowerSocialProfileID,
                    FollowedID = item.FollowedSocialProfileID,
                    Alerts = item.EnableNotifications,
                    Type = item.FollowRecordType.ToString(),
                };
                // Deterimine if CUSAR.SPID Follows => FA.SPID and then assing to => FID
                var Follow = (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == Response.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID);
                Follow = (Follow == null) ? (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Follows.FirstOrDefault(c => c.FollowerSocialProfileID == Response.Holder.ID && c.FollowedSocialProfileID == FA.FollowedID) : Follow;
                FA.ID = Follow?.ID;
                // Deterimine if Follow Exsists FID
                // Deterimine if FA.SPID follows CUSAR.SPID and then assing to => FBID
                var FollowBack = (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowedID && c.FollowedSocialProfileID == Response.Holder.ID);
                FollowBack = (FollowBack == null) ? (ABM.Models.Social.Follows.FollowRecord)Tenant.SocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == FA.FollowedID && c.FollowedSocialProfileID == Response.Holder.ID) : FollowBack;
                // FA.FollowBackID = FollowBack?.ID;
                Response.FollowRecords.Add(FA);
            }


            return Ok(Response.FollowRecords);
        }

        [HttpGet("Businesses")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Enrollment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseStatus), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Enrollment>>> GetMyBusinesses()
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<BusinessEnrollmentsResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            var Tenant = await _context.AllianceIDHolder
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
                .Where(e => e.GUID == Response.Holder.ID).FirstOrDefaultAsync().ConfigureAwait(false);



            Response.Enrollments = new List<Enrollment>();


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
                Response.Enrollments.Add(BRP);
            }

            return Ok(Response.Enrollments);
        }

        [HttpGet("Library")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetMyLibrary()
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User).ConfigureAwait(false)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);



            return Ok(Response);
        }


        [HttpGet("Addresses")]
        [Produces("application/json")]
        public async Task<ActionResult<APIResponse>> GetMyAddresses()
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response);



            return Ok(Response);
        }

        [HttpGet("Notifications")]
        [Produces("application/json")]
        public async Task<ActionResult<List<Notification>>> GetMyNotifications()
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);

            var EndUser = await _context.AllianceIDHolder.Include(c => c.SocialProfile).ThenInclude(c => c.Notifications).FirstOrDefaultAsync(m => m.GUID == Response.Holder.ID);

            return Ok(NotificationBinder.ToDTO(EndUser.SocialProfile.Notifications));
        }


        [HttpGet("Settings")]
        [Produces("application/json")]
        public async Task<ActionResult<APIResponse>> GetMySettings()
        {
            // Get Header
            var Response = JsonConvert.DeserializeObject<APIResponse>(JsonConvert.SerializeObject(await APIHelpers.BindAPIBaseResponse(_context, HttpContext, Request, AccountTools, User)));
            if (Response == null || !Response.Status.Success || Response.Holder == null)
                return Unauthorized(Response.Status);


            return Ok(Response);
        }



    }
}