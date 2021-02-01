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
                        if (!_context.AccountHolder.Any(c => c.ID == GUID))
                        {
                            try
                            {
                                //Make Temp Object
                                var Tenant = new AccountHolder
                                {
                                    ID = GUID,
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
                                Console.WriteLine($"DbUpdateException from {GUID}: {ex.ToString()}");
                            }
                        }
                        else
                        {
                            try
                            {
                                var Holder = _context.AccountHolder.FirstOrDefault(c => c.ID == GUID);
                                Holder.ID = GUID;
                                Holder.PublicName = publicName;
                                Holder.CountryID = CountryID;
                                Holder.LastName = lastName;
                                Holder.Name = name;
                                Holder.Email = email;
                                Holder.IdentityProvider = identityProvider;
                                // Update
                                _context.Update(Holder);
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
                        if (!await _context.AccountHolder.AnyAsync(c => c.ID == GUID))
                        {
                            try
                            {
                                //Make Temp Object
                                var Tenant = new AccountHolder
                                {
                                    ID = GUID,
                                    PublicName = publicName,
                                    CountryID = CountryID,
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
                                await _context.AccountHolder.AddAsync(Tenant);
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
                                var Holder = await _context.AccountHolder.FirstOrDefaultAsync(c => c.ID == GUID);
                                Holder.ID = GUID;
                                Holder.PublicName = publicName;
                                Holder.CountryID = CountryID;
                                Holder.LastName = lastName;
                                Holder.Name = name;
                                Holder.Email = email;
                                Holder.IdentityProvider = identityProvider;
                                // Update
                                _context.Entry(Holder).State = EntityState.Modified;
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

                        if (!await _context.AccountHolder.AnyAsync(c => c.ID == GUID))
                        {
                            // If user not exists in Database
                            try
                            {
                                //Make Temp Object
                                Response = new AccountHolder
                                {
                                    ID = GUID,
                                    PublicName = publicName,
                                    CountryID = CountryID,
                                    LastName = lastName,
                                    Name = name,
                                    Email = email,
                                    IdentityProvider = identityProvider,
                                    NameIdentifier = nameIdentifier,
                                    AccountHolderWallet = new AccountHolderWallet(),
                                    AccountHolderCart = new AccountHolderCart() { CurrencyID = "USD.USA" },
                                    SocialProfile = new AccountHolderSocialProfile(),
                                };
                                // Adds default Currency
                                await _context.AccountHolder.AddAsync(Response);
                                await _context.SaveChangesAsync();

                                // Get the full data object
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
                                         .FirstOrDefaultAsync(c => c.ID == GUID);

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
                                    .FirstOrDefaultAsync(c => c.ID == GUID);

                                Response.ID = GUID;
                                Response.PublicName = publicName;
                                Response.CountryID = CountryID;
                                Response.LastName = lastName;
                                Response.Name = name;
                                Response.Email = email;
                                Response.IdentityProvider = identityProvider;

                                if (Response.AccountHolderCart == null)
                                {
                                    Response.AccountHolderCart = new AccountHolderCart() { CurrencyID = "USD.USA" };
                                }

                                if (String.IsNullOrEmpty(Response.CountryID))
                                {
                                    Response.CountryID = "USA";
                                }

                                if (Response.SocialProfile == null)
                                {
                                    Response.SocialProfile = new AccountHolderSocialProfile();
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