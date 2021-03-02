using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Interfaces.Helpers;
using FenixAlliance.ABM.Models.Global.Carts.CartScopes;
using FenixAlliance.ABM.Models.Global.Wallets.WalletAccountScopes;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Social.SocialProfiles.Scopes;
using FenixAlliance.APS.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Helpers
{
    // ToDo: Document this Class. (God, please no.... )
    public class AccountUsersHelpers : IAccountUsersHelpers
    {
        private readonly ABMContext _context;
        public AccountUsersHelpers(ABMContext context)
        {
            _context = context;
        }
        public string GetActiveDirectoryGUID(ClaimsPrincipal User)
        {
            // Let's create a string to hold the user's ID.
            string objectIdentifier = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {

                objectIdentifier = GetActiveDirectoryObjectIdentifier(User) ?? GetActiveDirectoryNameIdentifier(User);
                var currentHolder = _context.AccountHolder.FirstOrDefault(c => c.ID == objectIdentifier);

                var name = GetActiveDirectoryGivenName(User);
                var nameIdentifier = GetActiveDirectoryNameIdentifier(User);
                var lastName = GetActiveDirectorySurName(User);
                var publicName = GetActiveDirectoryName(User);
                var email = GetActiveDirectoryEmail(User);
                var country = GetActiveDirectoryCountry(User);
                var CountryID = GetCountryID(country) ?? "USA";

                var identityProvider = GetActiveDirectoryIdentityProvider(User);
                var IdP_AccessToken = GetActiveDirectoryIdentityProviderToken(User);

                // If user not exists in Database
                if (currentHolder == null)
                {
                    try
                    {
                        //Make Temp Object
                        var Tenant = new AccountHolder
                        {
                            ID = objectIdentifier,
                            PublicName = publicName,
                            CountryID = CountryID,
                            LastName = lastName,
                            Name = name,
                            Email = email,
                            IdentityProvider = identityProvider,
                            NameIdentifier = nameIdentifier,
                            AccountHolderWallet = new AccountHolderWallet(),
                            AccountHolderCart = new AccountHolderCart(),
                            SocialProfile = new AccountHolderSocialProfile(),
                        };
                        // Adds default Currency
                        Tenant.AccountHolderCart.CurrencyID = "USD.USA";
                        _context.AccountHolder.Add(Tenant);
                        _context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DbUpdateException from {objectIdentifier}: {ex}");
                    }
                }
                else
                {
                    ValidateRequiredData(currentHolder, objectIdentifier, publicName, CountryID, lastName, name, email, identityProvider);
                }
            }
            return objectIdentifier;
        }

        public async Task<string> GetActiveDirectoryGUIDAsync(ClaimsPrincipal user)
        {
            string objectIdentifier = null;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {

                objectIdentifier = GetActiveDirectoryObjectIdentifier(user) ?? GetActiveDirectoryNameIdentifier(user);
                var nameIdentifier = GetActiveDirectoryNameIdentifier(user) ?? GetActiveDirectoryObjectIdentifier(user);

                var currentHolder = await _context.AccountHolder.FirstOrDefaultAsync(c => c.ID == objectIdentifier);

                var identityProvider = GetActiveDirectoryIdentityProvider(user);
                var name = GetActiveDirectoryGivenName(user);
                var lastName = GetActiveDirectorySurName(user);
                var publicName = GetActiveDirectoryName(user);
                var country = GetActiveDirectoryCountry(user);
                var email = GetActiveDirectoryEmail(user);
                var countryId = GetCountryID(country) ?? "USA";

                if (currentHolder == null)
                {
                    //Create Account Holder

                    try
                    {
                        //Make Temp Object
                        var tenant = new AccountHolder
                        {
                            ID = objectIdentifier,
                            PublicName = publicName,
                            CountryID = countryId,
                            LastName = lastName,
                            Name = name,
                            Email = email,
                            IdentityProvider = identityProvider,
                            NameIdentifier = nameIdentifier,
                            AccountHolderCart = new AccountHolderCart() { CurrencyID = "USD.USA" },
                            AccountHolderWallet = new AccountHolderWallet(),
                            SocialProfile = new AccountHolderSocialProfile(),
                        };
                        // Adds default Currency
                        await _context.AccountHolder.AddAsync(tenant);
                        await _context.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DbUpdateException from {objectIdentifier}: {ex}");
                    }
                }
                else
                {
                    ValidateRequiredData(currentHolder, objectIdentifier, publicName, countryId, lastName, name, email, identityProvider);
                    // Update
                    _context.Entry(currentHolder).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            return objectIdentifier;
        }

        private void ValidateRequiredData(AccountHolder currentHolder, string objectIdentifier, string publicName, string countryId, string lastName, string name, string email, string identityProvider)
        {
            // Update Account Holder
            try
            {
                currentHolder.ID = objectIdentifier;

                // If reported property is null or same as record, we need to stick to the old data.
                if (currentHolder.PublicName == null)
                    currentHolder.PublicName = publicName;

                if (currentHolder.CountryID == null)
                    currentHolder.CountryID = countryId;

                if (currentHolder.LastName == null)
                    currentHolder.LastName = lastName;

                if (currentHolder.Name == null)
                    currentHolder.Name = name;

                if (currentHolder.Email == null)
                    currentHolder.Email = email;

                if (currentHolder.IdentityProvider == null)
                    currentHolder.IdentityProvider = identityProvider;

                // If our identity provider is reporting new data, we need to update this property,
                if (currentHolder.PublicName != publicName && publicName != null)
                    currentHolder.PublicName = publicName;

                if (currentHolder.CountryID != countryId && countryId != null)
                    currentHolder.CountryID = countryId;

                if (currentHolder.LastName != lastName && lastName != null)
                    currentHolder.LastName = lastName;

                if (currentHolder.Name != name && name != null)
                    currentHolder.Name = name;

                if (currentHolder.Email != email && email != null)
                    currentHolder.Email = email;

                if (currentHolder.IdentityProvider != identityProvider && identityProvider != null)
                    currentHolder.IdentityProvider = identityProvider;



            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbUpdateException from {objectIdentifier}: {ex}");
            }
        }

        public string GetAndRegisterGUIDByToken(object Token)
        {
            B2CDecodedToken TokenUser = (B2CDecodedToken)Token;
            string GUID = null;
            if (TokenUser != null)
            {
                GUID = TokenUser.Oid;
                var nameIdentifier = TokenUser.Oid;
                var name = TokenUser.GivenName;
                var identityProvider = TokenUser.Idp;
                var lastName = TokenUser.FamilyName;
                var publicName = TokenUser.Name;
                var email = TokenUser.Emails?.FirstOrDefault();
                var CountryID = GetCountryID(TokenUser.Country) ?? "USA";


                // If user not exists in Database
                if (!_context.AccountHolder.Any(c => c.ID == GUID))
                {
                    try
                    {
                        //Make Temp Object
                        var Tenant = new AccountHolder
                        {
                            ID = GUID,
                            Name = name,
                            Email = email,
                            LastName = lastName,
                            CountryID = CountryID,
                            PublicName = publicName,
                            NameIdentifier = nameIdentifier,
                            IdentityProvider = identityProvider,
                            AccountHolderCart = new AccountHolderCart(),
                            SocialProfile = new AccountHolderSocialProfile(),
                            AccountHolderWallet = new AccountHolderWallet(),
                        };
                        // Adds default Currency
                        Tenant.AccountHolderCart.CurrencyID = "USD.USA";
                        _context.AccountHolder.Add(Tenant);
                        _context.SaveChanges();

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("DbUpdateException from :" + GUID);
                    }
                }
                else
                {
                    try
                    {
                        var Holder = _context.AccountHolder.FirstOrDefault(c => c.ID == GUID);
                        if (Holder != null)
                        {
                            Holder.ID = GUID;
                            Holder.PublicName = publicName;
                            Holder.CountryID = CountryID;
                            Holder.LastName = lastName;
                            Holder.Name = name;
                            Holder.Email = email;
                            Holder.IdentityProvider = identityProvider;

                            _context.Entry(Holder).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("DbUpdateException from :" + GUID);
                    }
                }
            }
            return GUID;
        }

        /// <summary>
        /// Gets the current holder and validate that required properties are set.
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public async Task<AccountHolder> GetCurrentHolder(ClaimsPrincipal User)
        {
            AccountHolder Response = null;
            if (User.Identity.IsAuthenticated)
            {
                var objectIdentifier = await GetActiveDirectoryGUIDAsync(User);

                Response = await _context.AccountHolder
                        .Include(c => c.SocialProfile)
                        .Include(c => c.AccountHolderWallet)
                        .Include(c => c.AccountHolderCart).ThenInclude(c => c.Currency)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessWallet)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessCart).ThenInclude(c => c.Currency)
                        // Get Permissions
                        .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                        .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                        .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                    .FirstOrDefaultAsync(c => c.ID == objectIdentifier);


                if (Response != null)
                {
                    // Verify & correct required data
                    Response.AccountHolderCart ??= new AccountHolderCart() { CurrencyID = "USD.USA" };

                    Response.CountryID ??= "USA";

                    Response.SocialProfile ??= new AccountHolderSocialProfile();

                    if (Response.SelectedBusiness != null)
                    {
                        Response.SelectedBusiness.BusinessCart ??= new BusinessCart() { CurrencyID = "USD.USA" };

                        Response.SelectedBusiness.CountryID ??= "USA";

                        Response.SelectedBusiness.BusinessWallet ??= new BusinessWallet() { CountryID = "USA" };

                        Response.SelectedBusiness.BusinessSocialProfile ??= new BusinessSocialProfile();

                        Response.SelectedBusiness.BusinessIndustryID ??= "Default";

                        Response.SelectedBusiness.ID ??= "Default";

                        // TODO: Add some non query security verifications for holder auth over current business
                    }

                    try
                    {
                        // Update
                        _context.Entry(Response).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return Response;
        }


        public string GetActiveDirectoryName(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Equals("name"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }


        public string GetActiveDirectoryNameIdentifier(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("nameidentifier"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }


        public string GetActiveDirectoryObjectIdentifier(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("objectidentifier"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string IsNewTenant(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("newUser"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetActiveDirectorySurName(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User?.Claims)
                {
                    if (claim.Type.Contains("surname"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetActiveDirectoryGivenName(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User?.Claims)
                {
                    if (claim.Type.Contains("givenname"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetActiveDirectoryJobTitle(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("jobTitle"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetActiveDirectoryCountry(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("country"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }


        public string GetActiveDirectoryEmail(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.Contains("email"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetActiveDirectoryIdentityProvider(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToLower().Contains("identityprovider"))
                    {
                        return claim.Value;
                    }
                }
            }
            return "Default";
        }

        public string GetActiveDirectoryIdentityProviderToken(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToLower().Contains("token"))
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public string GetCountryID(string CountryName)
        {
            try
            {
                var Country = _context.Country.FirstOrDefault(c =>
                c.Name == CountryName ||
                c.ISOAlpha3 == CountryName ||
                c.ISOAlpha2 == CountryName ||
                c.NativeName == CountryName
                );
                return Country.ISOAlpha3;
            }
            catch (Exception)
            {
                return "COL";
            }
        }

        public async Task<AccountHolder> GetTenantSocialProfileAsync(string GUID)
        {
            // Get Alliance ID Tenant from DB
            var Tenant = await _context.AccountHolder
                // Add Country
                .Include(c => c.Country)
                .Include(c => c.SocialProfile)
                .Include(c => c.AccountHolderCart)
                .Include(c => c.PersonaPartnerProfile)
                // Add Business I Work for
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business)
                .Include(c => c.SelectedBusiness).ThenInclude(c => c.Country)
                // Add users that I Follow
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AccountHolderSocialProfile)c.FollowedSocialProfile).AccountHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Business that I Follow
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AccountHolderSocialProfile)c.FollowerSocialProfile).AccountHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Social Comments
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((BusinessSocialProfile)c.OwnerSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((AccountHolderSocialProfile)c.OwnerSocialProfile).AccountHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((ContactSocialProfile)c.OwnerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Groups
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialPostReactions)
                // Social Posts
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group)
                // Add My Groups
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.SocialGroupEnrollmentRecords).ThenInclude(c => ((BusinessSocialProfile)c.SocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.SocialGroupEnrollmentRecords).ThenInclude(c => ((AccountHolderSocialProfile)c.SocialProfile).AccountHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SocialPostReactions)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SociaPostAttachments)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => c.ChildComments)
                // Where Current User
                .FirstOrDefaultAsync(m => m.ID == GUID);
            return Tenant;
        }
    }
}