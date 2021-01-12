using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using FenixAlliance.ABM.Models.Social.Follows;
using FenixAlliance.ABM.Models.Social.Notifications;
using FenixAlliance.ABM.Models.Social.SocialProfiles.Scopes;
using FenixAlliance.APS.Core.DataHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.APS.Core.DataAccess
{
    public class SocialDataAccessClient : ISocialDataAccess
    {
        private readonly ABMContext _context;
        private AccountUsersHelpers AccountTools { get; set; }
        private AccountGraphHelpers AccountGraphTools { get; set; }
        private BlobStorageDataAccessClient dataTools;
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;

        public SocialDataAccessClient(ABMContext context, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            _context = context;
            _config = AppConfiguration;
            _env = HostingEnvironment;
            dataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(_context);
            AccountGraphTools = new AccountGraphHelpers(_context, _config);
        }

        public SocialDataAccessClient(DbContextOptions<ABMContext> dbContextOptions, IConfiguration AppConfiguration, IHostEnvironment HostingEnvironment)
        {
            _context = new ABMContext(dbContextOptions);
            _config = AppConfiguration;
            _env = HostingEnvironment;
            dataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(_context);
            AccountGraphTools = new AccountGraphHelpers(_context, _config);
        }

        public Task<bool> AssertFollowExists(string FollowerSocialProfileID, string FollowedSocialProfileID)
        {
            return  _context.FollowRecord.AnyAsync(c=>c.FollowerSocialProfileID == FollowerSocialProfileID && c.FollowedSocialProfileID ==  FollowedSocialProfileID);
        }

        public Task<List<FollowRecord>> GetBusinessB2BFollowers(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetBusinessB2BFollows(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetBusinessB2CFollows(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetBusinessC2BFollowers(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<List<Notification>> GetBusinessNotifications(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<BusinessSocialProfile> GetBusinessSocialProfile(string BusinessID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetHolderB2CFollowers(string HolderGUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetHolderC2BFollows(string HolderGUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetHolderC2CFollowers(string HolderGUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowRecord>> GetHolderC2CFollows(string HolderGUID)
        {
            throw new NotImplementedException();
        }

        public Task<AllianceIDHolderSocialProfile> GetHolderSocialProfile(string HolderGUID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Notification>> GetNotifications(string BusinessID)
        {
            return (await _context.BusinessSocialProfile.AsNoTracking().Include(c=>c.Notifications)
            .FirstAsync(c => c.BusinessID == BusinessID)).Notifications.ToList();
         }

    }
}
