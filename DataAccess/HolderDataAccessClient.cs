using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Security.BusinessSecurityLogs;
using FenixAlliance.ABM.Models.Social.SocialProfiles.Scopes;
using FenixAlliance.ABM.Models.Tenants.BusinessProfileRecords;
using FenixAlliance.Data.Access.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.Data.Access.DataAccess
{
    public partial class HolderDataAccessClient : IHolderDataAccess
    {
        private readonly ABMContext _context;
        private AccountUsersHelpers AccountTools { get; set; }
        private AccountGraphHelpers AccountGraphTools { get; set; }
        private BlobStorageDataAccessClient dataTools;
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;

        //public HolderDataAccessClient(ABMContext context, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        //{
        //    _context = context;
        //    _config = AppConfiguration;
        //    _env = HostingEnvironment;
        //    dataTools = new BlobStorageDataAccessClient();
        //    AccountTools = new AccountUsersHelpers(_context);
        //    AccountGraphTools = new AccountGraphHelpers(_context, _config);
        //}

        public HolderDataAccessClient(DbContextOptions<ABMContext> dbContextOptions, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            _context = new ABMContext(dbContextOptions);
            _config = AppConfiguration;
            _env = HostingEnvironment;
            dataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(_context);
            AccountGraphTools = new AccountGraphHelpers(_context, _config);
        }

        public string GetCurrentHolderGUID(ClaimsPrincipal CurrentUser)
        {
            return AccountTools.GetActiveDirectoryGUID(CurrentUser);
        }


        public async Task<string> GetCurrentHolderGUIDAsync(ClaimsPrincipal CurrentUser)
        {
            return await AccountTools.GetActiveDirectoryGUIDAsync(CurrentUser);
        }

        public Task<AllianceIDHolder> GetHolder(string HolderGUID, bool TrackEntity = false)
        {
            if (TrackEntity == true)
            {
                return _context.AllianceIDHolder.Include(c => c.SelectedBusiness).FirstAsync(c => c.GUID == HolderGUID);
            }
            else
            {
                return _context.AllianceIDHolder.Include(c => c.SelectedBusiness).AsNoTracking().FirstAsync(c => c.GUID == HolderGUID);
            }
        }

        public async Task UpdateHolder(AllianceIDHolder Holder)
        {
            _context.Update(Holder);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<BusinessProfileRecord> GetCurrentBPR(string HolderGUID)
        {
            var User = await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords)
                .Include(c => c.SelectedBusiness)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID);

            if (User.SelectedBusiness == null)
            {
                return null;
            }

            return User.BusinessProfileRecords.First(c => c.BusinessID == User.SelectedBusiness.ID);
        }


        public async Task<AllianceIDHolder> GetFullHolderAndBusiness(ClaimsPrincipal CurrentUser)
        {
            var HolderGUID = await GetCurrentHolderGUIDAsync(CurrentUser);
            return await GetFullHolderAndBusiness(HolderGUID);
        }


        public async Task<AllianceIDHolder> GetFullHolderAndBusiness(ClaimsPrincipal CurrentUser, string BusinessID)
        {
            var HolderGUID = await GetCurrentHolderGUIDAsync(CurrentUser);
            return await GetFullHolderAndBusiness(HolderGUID, BusinessID);
        }

        public async Task<AllianceIDHolder> GetFullHolderAndBusiness(string HolderGUID)
        {
            return await _context.AllianceIDHolder
                .AsNoTracking()
                // Tenant Data
                .Include(c => c.Country)
                .Include(c => c.AllianceIDHolderCart)//.ThenInclude(c => c.Currency)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business)
                // Social Data
                // Add Follows
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                //.Include(c => c.SocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                //.Include(c => c.SocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Followers
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers)//.ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                //.Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                //.Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Businesses Data
                .Include(c => c.SelectedBusiness).ThenInclude(c => c.Country)
                .Include(c => c.SelectedBusiness).ThenInclude(c => c.Currency)
                .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessCart)
                // Businesses Social Data
                // Add Follows
                .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                //.Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                //.Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)//.ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Followers
                .Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)//.ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                //.Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)//.ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                //.Include(x => x.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)//.ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID);
        }

        public async Task<AllianceIDHolder> GetFullHolderAndBusiness(string HolderGUID, string TenantID)
        {
            var Holder = await _context.AllianceIDHolder
                // Tenant Data
                .Include(c => c.Country)
                .Include(c => c.AllianceIDHolderCart).ThenInclude(c => c.Currency)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business)
                // Social Data
                // Add Follows
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Followers
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID);
            // Validate if selection access is granted for this Tenant.
            if(Holder.BusinessProfileRecords.Any(c=>c.BusinessID == TenantID))
            {
                // Businesses Data
                var Business = await _context.Business.Include(c => c.Country)
                  .Include(c => c.Currency)
                  .Include(c => c.BusinessCart).ThenInclude(c => c.Currency)
                  // Businesses Social Data
                  // Add Follows
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                  // Add Followers
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                  .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                  .FirstOrDefaultAsync(c => c.ID == TenantID);

                Holder.SelectedBusiness = Business;
                Holder.SelectedBusinessID = Business.ID;
            }
            else
            {
                Holder.SelectedBusiness = null;
            }

            return Holder;

        }

        public async Task<List<BusinessSecurityLog>> GetBusinessSecurityLogs(string HolderGUID, string BusinessProfileRecordID, string BusinessID)
        {
            var Response = new List<BusinessSecurityLog>();
            if (!String.IsNullOrEmpty(HolderGUID))
            {
                if (!String.IsNullOrEmpty(BusinessProfileRecordID))
                {
                    if (!String.IsNullOrEmpty(BusinessID))
                    {
                        Response = (await _context.AllianceIDHolder
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c => c.BusinessSecurityLogs)
                            .FirstAsync(c => c.GUID == HolderGUID))
                            .BusinessProfileRecords.Where(c => c.ID == BusinessProfileRecordID && c.BusinessID == BusinessID)
                            .SelectMany(c => c.BusinessSecurityLogs).ToList();
                    }
                    else
                    {
                        Response = (await _context.AllianceIDHolder
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c => c.BusinessSecurityLogs)
                            .FirstAsync(c => c.GUID == HolderGUID))
                            .BusinessProfileRecords.Where(c => c.ID == BusinessProfileRecordID)
                            .SelectMany(c => c.BusinessSecurityLogs).ToList();
                    }
                }
                else
                {

                    if (!String.IsNullOrEmpty(BusinessID))
                    {
                        Response = (await _context.AllianceIDHolder
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c => c.BusinessSecurityLogs)
                            .FirstAsync(c => c.GUID == HolderGUID))
                            .BusinessProfileRecords.Where(c => c.BusinessID == BusinessID)
                            .SelectMany(c => c.BusinessSecurityLogs).ToList();
                    }
                    else
                    {
                        Response = (await _context.AllianceIDHolder
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c => c.BusinessSecurityLogs)
                            .FirstAsync(c => c.GUID == HolderGUID))
                            .BusinessProfileRecords
                            .SelectMany(c => c.BusinessSecurityLogs).ToList();
                    }
                }
            }
            return Response;
        }
    }
}
