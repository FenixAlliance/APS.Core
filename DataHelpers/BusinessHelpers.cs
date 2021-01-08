using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataHelpers;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Social.SocialProfiles.Scopes;
using FenixAlliance.ABM.Models.Tenants;
using FenixAlliance.ABM.Models.Tenants.BusinessProfileRecords;
using FenixAlliance.Data.Access.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.Data.Access.Helpers
{
    public class BusinessHelpers : IBusinessHelpers
    {
        private readonly ABMContext _context;
        private AccountUsersHelpers AccountTools { get; set; }
        private BlobStorageDataAccessClient DataTools;
        private BusinessDataAccessClient BusinessDataAccessClient;
        public BusinessHelpers(ABMContext context, IConfiguration configuration = null, IHostEnvironment hostingEnvironment = null)
        {
            _context = context;
            AccountTools = new AccountUsersHelpers(context);
            DataTools = new BlobStorageDataAccessClient();
            BusinessDataAccessClient = new BusinessDataAccessClient(_context, configuration, hostingEnvironment);
        }
        public async Task<List<Business>> GetBusinessesWithSocialProfileAsync()
        {
            return await _context.Business.AsNoTracking()
                 .Include(b => b.Country)
                 .Include(b => b.BusinessSegment)
                 .Include(b => b.BusinessIndustry)
                 .Include(c => c.JobOffers).ThenInclude(c => c.JobField)
                 .Include(c => c.JobOffers).ThenInclude(c => c.Country)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessType)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                 // Add Business that I Follow
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowerSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.Country)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessType)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                 .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowerSocialProfile).Contact).ThenInclude(c => c.Country)
                 .ToListAsync();
        }
        public async Task<Business> GetBusinessWithSocialProfileAsync(string businessID)
        {
            return await _context.Business.AsNoTracking()
                 .Include(b => b.Items)
                 .Include(b => b.Country)
                 .Include(b => b.BusinessSegment)
                 .Include(b => b.BusinessIndustry)
                 .Include(c => c.JobOffers).ThenInclude(c => c.JobField)
                 .Include(c => c.JobOffers).ThenInclude(c => c.Country)
                 .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.AllianceIDHolder).ThenInclude(c => c.Country)
                 .Include(b => b.BusinessSocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group)
                // Social Followers
                // Add users that I Follow
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Business that I Follow
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowerSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Social Posts
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((BusinessSocialProfile)c.OwnerSocialProfile).Business)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialPostReactions).ThenInclude(c => ((BusinessSocialProfile)c.SocialProfile).Business)
                // Social Reactions
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.OwnerSocialProfile).AllianceIDHolder)
                .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialPostReactions).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.SocialProfile).AllianceIDHolder)

                 .FirstOrDefaultAsync(c => c.ID == businessID || c.Handler == businessID);
        }

        public async Task<Business> GetSelectedBusinessWithBusinessSuitesAsync(ClaimsPrincipal User)
        {
            // Get Current TenantID
            var GUID = AccountTools.GetActiveDirectoryGUID(User);

            // Get Tenant with selected Business and suites
            var Tenant = await _context.AllianceIDHolder
                .Include(c => c.SelectedBusiness)
                .FirstOrDefaultAsync(c => c.GUID == GUID);

            if ((Tenant.SelectedBusiness == null) || (Tenant.SelectedBusinessID == null))
            {
                return await Task.FromResult<Business>(null);
            }

            var BusinessWithSubscription = await _context.Business.AsNoTracking()
                .Include(b => b.Country)
                .Include(b => b.BusinessIndustry)
                .Include(c => c.DealUnitFlowStages)
                .Include(c => c.Activities)
                .Include(c => c.DealUnits)
                .Include(c => c.Contacts)
                .Include(c => c.JobOffers)
                .FirstOrDefaultAsync(m => m.ID == Tenant.SelectedBusinessID);

            return BusinessWithSubscription;
        }

        public async Task<bool> UserOwnsBusinessAsync(ClaimsPrincipal User, string BusinessID)
        {
            // Check for existance business data and User Tenant
            if (_context.Business.Any(m => m.ID == BusinessID) || BusinessID == null || User.Identity.IsAuthenticated == false)
            {
                return false;
            }

            var GUID = AccountTools.GetActiveDirectoryGUID(User);
            var Tenant = await _context.AllianceIDHolder.AsNoTracking()
                // Load Business Owner Data
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.GUID == GUID).FirstOrDefaultAsync();


            // Search for User's Businesses Employee Ownerships to determine if the user can select the requested business
            return BusinessDataAccessClient.ResolveRequestedAccess(Tenant, null, new List<string> { "business_owner" });
        }

        public async Task<bool> UserWorksForBusinessAsync(ClaimsPrincipal User, string BusinessID)
        {

            // Check for existance business data and User Tenant
            if (_context.Business.Any(m => m.ID == BusinessID) || BusinessID == null || User.Identity.IsAuthenticated == false)
            {
                return false;
            }

            var GUID = AccountTools.GetActiveDirectoryGUID(User);
            var Holder = await _context.AllianceIDHolder.AsNoTracking()
                // Load Business Employee data
                .Include(b => b.BusinessProfileRecords).ThenInclude(c => c.Business)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Where(e => e.GUID == GUID).FirstOrDefaultAsync();

            // Search for User's Businesses Employee Ownerships to determine if the user can select the requested business
            return BusinessDataAccessClient.ResolveRequestedAccess(Holder, null, new List<string> { "business_employee" });
        }

        public async Task<BusinessProfileRecord> GetBusinessProfileRecordAsync(string TenantGUID, string BusinessID)
        {

            // Check for existance business data and User Tenant
            if (!_context.Business.Any(m => m.ID == BusinessID) || BusinessID == null)
            {
                return null;
            }

            return await _context.BusinessProfileRecord.AsNoTracking()
                .Include(c => c.BusinessProfileSecurityRoleGrants)
                .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                // Load Business Employee data
                .FirstOrDefaultAsync(t => t.AllianceIDHolderGUID == TenantGUID && t.BusinessID == BusinessID);
        }

        public async Task<AllianceIDHolder> GetTenantWithSelectedBusinessAsync(ClaimsPrincipal User)
        {
            var GUID = AccountTools.GetActiveDirectoryGUID(User);
            var Tenant = await _context.AllianceIDHolder.AsNoTracking()
                 .Include(c => c.SocialProfile)
                 // Tenant Social Profile
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.Country)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessType)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.SocialPosts)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessType)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                 // Add Business that I Follow
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowerSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.Country)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessType)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                 .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Where Statement
                .Where(e => e.GUID == GUID).FirstOrDefaultAsync();

            return Tenant;

        }

    }
}

