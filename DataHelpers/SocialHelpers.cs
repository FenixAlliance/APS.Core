using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataHelpers;
using FenixAlliance.APS.Core.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FenixAlliance.APS.Core.DataHelpers
{
    public class SocialHelpers : ISocialHelpers
    {
        private readonly ABMContext DataContext;
        private AccountUsersHelpers AccountTools { get; set; }
        private readonly BlobStorageDataAccessClient DataTools;

        public SocialHelpers(ABMContext ABMContext)
        {
            DataContext = ABMContext;
            DataTools = new BlobStorageDataAccessClient();
            AccountTools = new AccountUsersHelpers(DataContext);
        }


        public string GetSocialFollowType(string FollowerType, string FollowedType)
        {
            if (FollowerType == "Tenant" && FollowedType == "Tenant")
            {
                return "B2B";
            }

            if (FollowerType == "Tenant" && FollowedType == "Holder")
            {
                return "B2C";
            }

            if (FollowerType == "Holder" && FollowedType == "Tenant")
            {
                return "C2B";
            }

            if (FollowerType == "Holder" && FollowedType == "Holder")
            {
                return "C2C";
            }

            if (FollowerType == "Tenant" && FollowedType == "Tenant")
            {
                return "B2C";
            }

            if (FollowerType == "Contact" && FollowedType == "Contact")
            {
                return "Contact2Contact";
            }

            return null;
        }

        public async Task<string> GetSocialProfileType(string SocialProfileID)
        {
            if (await DataContext.AccountHolderSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
            {
                return "Holder";
            }

            if (await DataContext.BusinessSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
            {
                return "Tenant";
            }

            if (await DataContext.ContactSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
            {
                return "Contact";
            }

            return null;
        }
    }
}
