using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataHelpers;
using FenixAlliance.ABM.Models.Global.Carts.CartScopes;
using FenixAlliance.ABM.Models.Global.Wallets.WalletAccountScopes;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Social.SocialProfiles.Scopes;
using FenixAlliance.APS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FenixAlliance.APS.Core.DataHelpers
{
    public class AccountUsersHelpers : IAccountUsersHelpers
    {
        private readonly ABMContext _context;
        public AccountUsersHelpers(ABMContext context)
        {
            _context = context;
        }
        public string GetActiveDirectoryGUID(ClaimsPrincipal User)
        {
            string GUID = null;
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().ToLower().Contains("objectidentifier"))
                    {
                        var name = GetActiveDirectoryGivenName(User);
                        GUID = GetActiveDirectoryNameIdentifier(User);
                        var nameIdentifier = GetActiveDirectoryNameIdentifier(User);
                        var lastName = GetActiveDirectorySurName(User);
                        var publicName = GetActiveDirectoryName(User);
                        var email = GetActiveDirectoryEmail(User);
                        var country = GetActiveDirectoryCountry(User);
                        var CountryID = GetCountryID(country) ?? "USA";
                        var identityProvider = GetActiveDirectoryIdentityProvider(User);
                        var IdP_AccessToken = GetActiveDirectoryIdentityProviderToken(User);

                        // If user not exists in Database
                        if (!_context.AllianceIDHolder.Any(c => c.GUID == GUID))
                        {
                            try
                            {
                                //Make Temp Object
                                var Tenant = new AllianceIDHolder
                                {
                                    GUID = GUID,
                                    PublicName = publicName,
                                    CountryID = CountryID,
                                    LastName = lastName,
                                    Name = name,
                                    Email = email,
                                    IdentityProvider = identityProvider,
                                    NameIdentifier = nameIdentifier,
                                    AllianceIDHolderWallet = new AllianceIDHolderWallet(),
                                    AllianceIDHolderCart = new AllianceIDHolderCart(),
                                    SocialProfile = new AllianceIDHolderSocialProfile(),
                                };
                                // Adds default Currency
                                Tenant.AllianceIDHolderCart.CurrencyID = "USD.USA";
                                _context.AllianceIDHolder.Add(Tenant);
                                _context.SaveChanges();

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                        else
                        {
                            try
                            {
                                var Tenant = _context.AllianceIDHolder.FirstOrDefault(c => c.GUID == GUID);
                                Tenant.GUID = GUID;
                                Tenant.PublicName = publicName;
                                Tenant.CountryID = CountryID;
                                Tenant.LastName = lastName;
                                Tenant.Name = name;
                                Tenant.Email = email;
                                Tenant.IdentityProvider = identityProvider;
                                // Update
                                _context.Update(Tenant);
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                    }
                }
            }
            return GUID;
        }

        public async Task<string> GetActiveDirectoryGUIDAsync(ClaimsPrincipal User)
        {
            string GUID = null;
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().ToLower().Contains("objectidentifier"))
                    {
                        GUID = GetActiveDirectoryNameIdentifier(User);
                        var name = GetActiveDirectoryGivenName(User);
                        var nameIdentifier = Guid.NewGuid().ToString(); ;
                        var lastName = GetActiveDirectorySurName(User);
                        var publicName = GetActiveDirectoryName(User);
                        var email = GetActiveDirectoryEmail(User);
                        var country = GetActiveDirectoryCountry(User);
                        var CountryID = GetCountryID(country) ?? "USA";
                        var identityProvider = GetActiveDirectoryIdentityProvider(User);

                        // If user not exists in Database
                        if (!await _context.AllianceIDHolder.AnyAsync(c => c.GUID == GUID))
                        {
                            try
                            {
                                //Make Temp Object
                                var Tenant = new AllianceIDHolder
                                {
                                    GUID = GUID,
                                    PublicName = publicName,
                                    CountryID = CountryID,
                                    LastName = lastName,
                                    Name = name,
                                    Email = email,
                                    IdentityProvider = identityProvider,
                                    NameIdentifier = nameIdentifier,
                                    AllianceIDHolderCart = new AllianceIDHolderCart() { CurrencyID = "USD.USA" },
                                    AllianceIDHolderWallet = new AllianceIDHolderWallet(),
                                    SocialProfile = new AllianceIDHolderSocialProfile(),
                                };
                                // Adds default Currency
                                await _context.AllianceIDHolder.AddAsync(Tenant);
                                await _context.SaveChangesAsync();

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                        else
                        {
                            try
                            {
                                var Tenant = await _context.AllianceIDHolder.FirstOrDefaultAsync(c => c.GUID == GUID);
                                Tenant.GUID = GUID;
                                Tenant.PublicName = publicName;
                                Tenant.CountryID = CountryID;
                                Tenant.LastName = lastName;
                                Tenant.Name = name;
                                Tenant.Email = email;
                                Tenant.IdentityProvider = identityProvider;
                                // Update
                                _context.Entry(Tenant).State = EntityState.Modified;
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                    }
                }
            }
            return GUID;
        }

        public string GetAndRegisterGUIDByToken(object Token)
        {
            B2CDecodedToken TokenUser = (B2CDecodedToken) Token;
            string GUID = null;
            if (TokenUser != null)
            {
                GUID = TokenUser.Oid.ToString();
                var nameIdentifier = TokenUser.Oid.ToString();
                var name = TokenUser.GivenName;
                var identityProvider = TokenUser.Idp;
                var lastName = TokenUser.FamilyName;
                var publicName = TokenUser.Name;
                var email = TokenUser.Emails?.FirstOrDefault();
                var CountryID = GetCountryID(TokenUser.Country) ?? "USA";


                // If user not exists in Database
                if (!_context.AllianceIDHolder.Any(c => c.GUID == GUID))
                {
                    try
                    {
                        //Make Temp Object
                        var Tenant = new AllianceIDHolder
                        {
                            GUID = GUID,
                            Name = name,
                            Email = email,
                            LastName = lastName,
                            CountryID = CountryID,
                            PublicName = publicName,
                            NameIdentifier = nameIdentifier,
                            IdentityProvider = identityProvider,
                            AllianceIDHolderCart = new AllianceIDHolderCart(),
                            SocialProfile = new AllianceIDHolderSocialProfile(),
                            AllianceIDHolderWallet = new AllianceIDHolderWallet(),
                        };
                        // Adds default Currency
                        Tenant.AllianceIDHolderCart.CurrencyID = "USD.USA";
                        _context.AllianceIDHolder.Add(Tenant);
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
                        var Tenant = _context.AllianceIDHolder.FirstOrDefault(c => c.GUID == GUID);
                        Tenant.GUID = GUID;
                        Tenant.PublicName = publicName;
                        Tenant.CountryID = CountryID;
                        Tenant.LastName = lastName;
                        Tenant.Name = name;
                        Tenant.Email = email;
                        Tenant.IdentityProvider = identityProvider;

                        _context.Entry(Tenant).State = EntityState.Modified;

                        _context.SaveChanges();
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
        public async Task<AllianceIDHolder> GetCurrentHolder(ClaimsPrincipal User)
        {
            AllianceIDHolder Response = null;
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().ToLower().Contains("objectidentifier"))
                    {
                        var GUID = GetActiveDirectoryNameIdentifier(User);
                        var name = GetActiveDirectoryGivenName(User);
                        var nameIdentifier = Guid.NewGuid().ToString();
                        var lastName = GetActiveDirectorySurName(User);
                        var publicName = GetActiveDirectoryName(User);
                        var email = GetActiveDirectoryEmail(User);
                        var country = GetActiveDirectoryCountry(User);
                        var CountryID = GetCountryID(country) ?? "USA";
                        var identityProvider = GetActiveDirectoryIdentityProvider(User);

                        if (!await _context.AllianceIDHolder.AnyAsync(c => c.GUID == GUID))
                        {
                            // If user not exists in Database
                            try
                            {
                                //Make Temp Object
                                Response = new AllianceIDHolder
                                {
                                    GUID = GUID,
                                    PublicName = publicName,
                                    CountryID = CountryID,
                                    LastName = lastName,
                                    Name = name,
                                    Email = email,
                                    IdentityProvider = identityProvider,
                                    NameIdentifier = nameIdentifier,
                                    AllianceIDHolderWallet = new AllianceIDHolderWallet(),
                                    AllianceIDHolderCart = new AllianceIDHolderCart() { CurrencyID = "USD.USA" },
                                    SocialProfile = new AllianceIDHolderSocialProfile(),
                                };
                                // Adds default Currency
                                await _context.AllianceIDHolder.AddAsync(Response);
                                await _context.SaveChangesAsync();

                                // Get the full data object
                                Response = await _context.AllianceIDHolder
                                         .Include(c => c.SocialProfile)
                                         .Include(c => c.AllianceIDHolderWallet)
                                         .Include(c => c.AllianceIDHolderCart).ThenInclude(c => c.Currency)
                                         .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile)
                                         .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessWallet)
                                         .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessCart).ThenInclude(c => c.Currency)
                                         // Get Permissions
                                         .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                                                .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                                         .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                                         .FirstOrDefaultAsync(c => c.GUID == GUID);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                        else
                        {
                            // If user already exists
                            try
                            {
                                Response = await _context.AllianceIDHolder
                                    .Include(c => c.SocialProfile)
                                    .Include(c => c.AllianceIDHolderWallet)
                                    .Include(c => c.AllianceIDHolderCart).ThenInclude(c => c.Currency)
                                    .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile)
                                    .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessWallet)
                                    .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessCart).ThenInclude(c => c.Currency)
                                    // Get Permissions
                                    .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants)
                                        .ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c => c.BusinessPermission)
                                    .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                                    .FirstOrDefaultAsync(c => c.GUID == GUID);

                                Response.GUID = GUID;
                                Response.PublicName = publicName;
                                Response.CountryID = CountryID;
                                Response.LastName = lastName;
                                Response.Name = name;
                                Response.Email = email;
                                Response.IdentityProvider = identityProvider;

                                if (Response.AllianceIDHolderCart == null)
                                {
                                    Response.AllianceIDHolderCart = new AllianceIDHolderCart() { CurrencyID = "USD.USA" };
                                }

                                if (String.IsNullOrEmpty(Response.CountryID))
                                {
                                    Response.CountryID = "USA";
                                }

                                if (Response.SocialProfile == null)
                                {
                                    Response.SocialProfile = new AllianceIDHolderSocialProfile();
                                }

                                if (Response.SelectedBusiness != null)
                                {
                                    if (Response.SelectedBusiness.BusinessCart == null)
                                    {
                                        Response.SelectedBusiness.BusinessCart = new BusinessCart() { CurrencyID = "USD.USA" };
                                    }

                                    if (Response.SelectedBusiness.Country == null)
                                    {
                                        Response.SelectedBusiness.CountryID = "USA";
                                    }

                                    if (Response.SelectedBusiness.BusinessWallet == null)
                                    {
                                        Response.SelectedBusiness.BusinessWallet = new BusinessWallet() { CountryID = "USA" };
                                    }

                                    if (Response.SelectedBusiness.BusinessSocialProfile == null)
                                    {
                                        Response.SelectedBusiness.BusinessSocialProfile = new BusinessSocialProfile();
                                    }

                                    if (String.IsNullOrEmpty(Response.SelectedBusiness.BusinessIndustryID))
                                    {
                                        Response.SelectedBusiness.BusinessIndustryID = "Default";
                                    }

                                    if (String.IsNullOrEmpty(Response.SelectedBusiness.ID))
                                    {
                                        Response.SelectedBusiness.ID = "1 - 9 Employees";
                                    }

                                    if (String.IsNullOrEmpty(Response.SelectedBusiness.ID))
                                    {
                                        Response.SelectedBusiness.ID = "Default";
                                    }

                                    //TOOD: Add some non query security verifications for holder auth over current business
                                }

                                // Update
                                _context.Entry(Response).State = EntityState.Modified;
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                                // TODO: Send Email with error details.
                            }
                        }
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
                    if (claim.Type.ToString().Equals("name"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }


        public string GetActiveDirectoryNameIdentifier(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("nameidentifier"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string IsNewTenant(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("newUser"))
                    {
                        return claim.Value;
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectorySurName(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("surname"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectoryGivenName(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("givenname"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectoryJobTitle(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("jobTitle"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectoryCountry(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("country"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }


        public string GetActiveDirectoryEmail(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().Contains("email"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectoryIdentityProvider(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().ToLower().Contains("identityprovider"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
        }

        public string GetActiveDirectoryIdentityProviderToken(ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                //CheckValues
                foreach (Claim claim in User.Claims)
                {
                    if (claim.Type.ToString().ToLower().Contains("token"))
                    {
                        return claim.Value.ToString();
                    }
                }
            }
            return "";
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

        public async Task<AllianceIDHolder> GetTenantSocialProfileAsync(string GUID)
        {
            // Get Alliance ID Tenant from DB
            var Tenant = await _context.AllianceIDHolder
                // Add Country
                .Include(c => c.Country)
                .Include(c => c.SocialProfile)
                .Include(c => c.AllianceIDHolderCart)
                .Include(c => c.PersonaPartnerProfile)
                // Add Business I Work for
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business)
                .Include(c => c.SelectedBusiness).ThenInclude(c => c.Country)
                // Add users that I Follow
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowedSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((BusinessSocialProfile)c.FollowedSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Follows).ThenInclude(c => ((ContactSocialProfile)c.FollowedSocialProfile).Contact).ThenInclude(c => c.Country)
                // Add Business that I Follow
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.FollowerSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessType)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((BusinessSocialProfile)c.FollowerSocialProfile).Business).ThenInclude(c => c.BusinessIndustry)
                .Include(c => c.SocialProfile).ThenInclude(c => c.Followers).ThenInclude(c => ((ContactSocialProfile)c.FollowerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Social Comments
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((BusinessSocialProfile)c.OwnerSocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.OwnerSocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => ((ContactSocialProfile)c.OwnerSocialProfile).Contact).ThenInclude(c => c.Country)
                // Groups
                .Include(c => c.SocialProfile).ThenInclude(c => c.SocialPosts).ThenInclude(c => c.SocialPostReactions)
                // Social Posts
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group)
                // Add My Groups
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.SocialGroupEnrollmentRecords).ThenInclude(c => ((BusinessSocialProfile)c.SocialProfile).Business).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.SocialGroupEnrollmentRecords).ThenInclude(c => ((AllianceIDHolderSocialProfile)c.SocialProfile).AllianceIDHolder).ThenInclude(c => c.Country)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SocialPostReactions)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SociaPostAttachments)
                .Include(c => c.SocialProfile).ThenInclude(c => c.GroupMembershipRecords).ThenInclude(c => c.Group).ThenInclude(c => c.GroupSocialPosts).ThenInclude(c => c.SocialComments).ThenInclude(c => c.ChildComments)
                // Where Current User
                .FirstOrDefaultAsync(m => m.GUID == GUID);
            return Tenant;
        }
    }
}