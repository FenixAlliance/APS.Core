using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataHelpers;
using FenixAlliance.Data.Access.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FenixAlliance.Data.Access.Helpers
{
    public class SocialHelpers : ISocialHelpers
    {
        private readonly ABMContext _context;
        private AccountUsersHelpers AccountTools { get; set; }
        private readonly BlobStorageDataAccessClient DataTools;
        public SocialHelpers(ABMContext context)
        {
            _context = context;
            AccountTools = new AccountUsersHelpers(context);
            DataTools = new BlobStorageDataAccessClient();
        }


        public string GetSocialFollowType(string FollowerType, string FollowedType)
        {
            if (FollowerType == "Tenant" && FollowedType == "Tenant")
                return "B2B";

            if (FollowerType == "Tenant" && FollowedType == "Holder")
                return "B2C";

            if (FollowerType == "Holder" && FollowedType == "Tenant")
                return "C2B";

            if (FollowerType == "Holder" && FollowedType == "Holder")
                return "C2C";

            if (FollowerType == "Tenant" && FollowedType == "Tenant")
                return "B2C";

            if (FollowerType == "Contact" && FollowedType == "Contact")
                return "Contact2Contact";

            return null;
        }

        public async Task<string> GetSocialProfileType(string SocialProfileID)
        {
            if (await _context.AllianceIDHolderSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
                return "Holder";

            if (await _context.BusinessSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
                return "Tenant";

            if (await _context.ContactSocialProfile.AnyAsync(c => c.ID == SocialProfileID))
                return "Contact";

            return null;
        }
    }
}
