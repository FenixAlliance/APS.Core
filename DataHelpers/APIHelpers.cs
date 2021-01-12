using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Models.DTOs.Authorization;
using FenixAlliance.ABM.Models.DTOs.Components.Global.Currencies;
using FenixAlliance.ABM.Models.DTOs.Components.Tenants;
using FenixAlliance.ABM.Models.DTOs.Responses;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Tenants;
using FenixAlliance.ABM.SDK.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FenixAlliance.APS.Core.DataHelpers
{
    public class APIHelpers
    {
        public static async Task<APIResponse> BindAPIBaseResponse(ABMContext _context, HttpContext _httpContext, HttpRequest _request, AccountUsersHelpers AccountTools, ClaimsPrincipal _user, string TenantID = "", List<string> RequiredPermissions = null, List<string> RequiredRoles = null, bool EnforceBusinessResponse = false)
        {
            Business Business = null;
            AllianceIDHolder AllianceIDHolder;
            string EnrollmentID;
            string HolderID;

            var Settings = await _context.Settings.FirstAsync(c => c.SettingsPK == "General");
            var ExchangeRates = JsonConvert.DeserializeObject<CurrencyExchangeRates>(Settings.OpenCurrencyExchangeRates );

            var Response = new APIResponse()
            {
                Status = new ResponseStatus()
                {
                    CorrelationID = Activity.Current?.Id ?? _httpContext.TraceIdentifier
                }
            };

            try
            {
                if (_user.Identity.IsAuthenticated)
                {
                    // If the current request comes from an authenticated user.
                    HolderID = AccountTools.GetActiveDirectoryGUID(_user);
                }
                else
                {
                    // If accessing data on behalf of certain user or customer by certain Application.
                    if (AccountOAuthHelpers.ExtractAuthType(_request).ToUpperInvariant() == "Bearer".ToUpperInvariant())
                    {
                        try
                        {
                            /*  
                             * 1. Get the authentication header
                             * 2. Assert the token issuer. If comes from us on behalf of certain application, run the rest of the workflow. else, try AAD B2C Authorization.
                             */

                            var AuthHeader = _request.Headers.FirstOrDefault(x => x.Key.ToLowerInvariant() == "authorization").Value.FirstOrDefault();

                            string BearerToken = "";
                            if (AuthHeader != null && AuthHeader.Split(' ')[0].ToLowerInvariant() == "bearer")
                            {
                                BearerToken = AuthHeader.Split(' ')[1];
                            }

                            var TokenComponents = BearerToken.Split('.');
                            var TokenHeader = TokenComponents[0];
                            var TokenPayload = TokenComponents[1];
                            var TokenSignature = TokenComponents[2];

                            var DecodedHeaderString = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(TokenHeader));
                            var DecodedPayloadString = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(TokenPayload));
                            //var DecodedSignatureString = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(TokenSignature));

                            var Header = JsonConvert.DeserializeObject<JsonWebTokenHeader>(DecodedHeaderString);
                            var Payload = JsonConvert.DeserializeObject<JsonWebTokenPayload>(DecodedPayloadString);

                            // Try to retrieve the application secret based on the kid property of the JWT Header.

                            var SecretSet = await _context.BusinessApplicationSecret.Include(c => c.BusinessApplication).ThenInclude(c => c.Business).FirstAsync(c => c.ID == Header.kid);

                            if (SecretSet == null)
                            {
                                throw new InvalidOperationException("Something went wrong retrieving application client. Decryption went wrong.");
                            }

                            // Verify signature
                            using (var rsa = new RSACryptoServiceProvider())
                            {
                                rsa.FromXmlString(StringHelpers.Base64Decode(SecretSet.SigningPublicKey));
                                var PrivateRSAKeyParameters = rsa.ExportParameters(false);
                                if (!SecurityHelpers.VerifyPayload(Payload, TokenSignature, PrivateRSAKeyParameters))
                                {
                                    throw new InvalidOperationException("Signature verification went wrong.");
                                }
                            }


                            var ApplicationID = Payload.cid;
                            EnrollmentID = Payload.sub;

                            if (!String.IsNullOrEmpty(TenantID))
                            {
                                if (TenantID != Payload.act)
                                {
                                    throw new InvalidOperationException($"Can't use this token for Business Tenant: {TenantID}.");
                                }
                            }

                            TenantID = Payload.act;


                            var RequestedBPR = await _context.BusinessProfileRecord.Include(c => c.AllianceIDHolder).AsNoTracking().Where(c => c.ID == EnrollmentID).FirstAsync();


                            if (RequestedBPR == null)
                            {
                                // Return new error
                                Response.Tenant = null;
                                Response.Holder = null;
                                Response.Application = null;
                                Response.Status.Success = false;
                                Response.Status.Error = new Error
                                {
                                    ID = "E05"
                                };
                                throw new InvalidOperationException("Requested Business Profile Record does not exists.");
                            }

                            // TODO: Check that applicationID exists and is equal to AppSecret.BusinessApplication.ID
                            if (SecretSet.BusinessApplication.ID != ApplicationID)
                            {
                                // Return new error
                                Response.Tenant = null;
                                Response.Holder = null;
                                Response.Application = null;
                                Response.Status.Success = false;
                                Response.Status.Error = new Error() { 
                                    ID = "E05",
                                    Description = "Cannot validate client's identity. Decryption went wrong."
                                };
                                throw new InvalidOperationException("Cannot validate client's identity. Decryption went wrong.");

                            }


                            Response.Application = new ClientApplication()
                            {
                                ID = ApplicationID,
                                Name = SecretSet.BusinessApplication.Name,
                                Avatar = SecretSet.BusinessApplication.AvatarURL,
                                TenantID = SecretSet.BusinessApplication.BusinessID,
                                TenantName = SecretSet.BusinessApplication.Business.BusinessName,
                                PrivacyPolicy = SecretSet.BusinessApplication.PrivacyPolicyURL,
                                TenantAvatar = SecretSet.BusinessApplication.Business.BusinessAvatarURL,
                                TermsAndConditions = SecretSet.BusinessApplication.TermsAndConditionsURL,
                                GrantedPermissions = Payload.Scopes
                            };

                            HolderID = RequestedBPR.AllianceIDHolder.GUID;
                        }
                        catch
                        {
                            // Try to retrieve by AAD B2c Token. 
                            HolderID = await AccountOAuthHelpers.GetAndRegisterGUIDByRequestAsync(_request, AccountTools);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Identity verification went wrong.");
                    }
                }


                AllianceIDHolder = await _context.AllianceIDHolder
                        .Include(c => c.Country)
                        .Include(c => c.SocialProfile)
                        .Include(c => c.AllianceIDHolderWallet)
                        .Include(c => c.BusinessProfileRecords)
                        .Include(c => c.AllianceIDHolderCart).ThenInclude(c => c.Currency).ThenInclude(c => c.Country)
                        // Partner Profile
                        .Include(c => c.PersonaPartnerProfile)
                        .Include(c => c.SocialProfile).ThenInclude(c => c.Followers)
                        .Include(c => c.SocialProfile).ThenInclude(c => c.Follows)
                        // Add Business I Work for
                        .Include(c => c.BusinessProfileRecords)
                        // Add Current Business Info
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.Country)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessIndustry)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSegment)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessWallet)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessProfileRecords)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessCart).ThenInclude(c => c.Currency).ThenInclude(c => c.Country)
                        // Partner Profiles
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessPartnerProfile)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.StartupsPartnerProfile)
                        // Add Current Business
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)
                        .Include(c => c.SelectedBusiness).ThenInclude(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)
                .FirstOrDefaultAsync(c => c.GUID == HolderID);

                if (String.IsNullOrEmpty(TenantID))
                {
                    TenantID = AllianceIDHolder.SelectedBusinessID;
                }

                if (!String.IsNullOrEmpty(TenantID))
                {

                    Business = await _context.Business
                      .Include(c => c.Country)
                      .Include(c => c.BusinessSocialProfile)
                      .Include(c => c.BusinessWallet)
                      .Include(c => c.BusinessProfileRecords)
                      .Include(c => c.BusinessCart).ThenInclude(c => c.Currency).ThenInclude(c => c.Country)
                      // Partner Profile
                      .Include(c => c.BusinessPartnerProfile)
                      .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)
                      .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)
                      // Add Business I Work for
                      .Include(c => c.BusinessProfileRecords)
                      // Add Current Business Info
                      .Include(c => c.Country)
                      .Include(c => c.BusinessIndustry)
                      .Include(c => c.BusinessSegment)
                      .Include(c => c.BusinessWallet)
                      .Include(c => c.BusinessProfileRecords)
                      .Include(c => c.BusinessCart).ThenInclude(c => c.Currency).ThenInclude(c => c.Country)
                      // Partner Profiles
                      .Include(c => c.BusinessPartnerProfile)
                      .Include(c => c.StartupsPartnerProfile)
                      // Add Current Business
                      .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Follows)
                      .Include(c => c.BusinessSocialProfile).ThenInclude(c => c.Followers)
                    .FirstOrDefaultAsync(c => c.ID == TenantID);
                }

                if (AllianceIDHolder != null)
                {
                    Response.Holder.ID = AllianceIDHolder.GUID;
                    Response.Holder.CartID = AllianceIDHolder.AllianceIDHolderCart.ID;
                    Response.Holder.AvatarURL = AllianceIDHolder.AvatarURL;
                    Response.Holder.CoverURL = AllianceIDHolder.ProfileCoverURL;
                    Response.Holder.FacebookURL = AllianceIDHolder.FacebookURL;
                    Response.Holder.TwitterURL = AllianceIDHolder.GithubURL;
                    Response.Holder.InstagramURL = AllianceIDHolder.InstagramURL;
                    Response.Holder.YouTubeURL = AllianceIDHolder.YouTubeURL;
                    Response.Holder.WebsiteURL = AllianceIDHolder.WebURL;
                    Response.Holder.LinkedInURL = AllianceIDHolder.LinkedInURL;
                    Response.Holder.GitHubURL = AllianceIDHolder.GithubURL;
                    Response.Holder.About = AllianceIDHolder.About;
                    Response.Holder.Email = AllianceIDHolder.Email;
                    Response.Holder.PublicName = AllianceIDHolder.PublicName;
                    Response.Holder.CountryID = AllianceIDHolder.Country.ISOAlpha3;
                    Response.Holder.CurrencyID = AllianceIDHolder.AllianceIDHolderCart.CurrencyID;
                    Response.Holder.IdProvider = AllianceIDHolder.IdentityProvider;
                    Response.Holder.CurrencyID = AllianceIDHolder.AllianceIDHolderCart.Currency.ISOCode;
                    Response.Holder.SocialProfileID = AllianceIDHolder.SocialProfile.ID;
                    Response.Holder.EnrollmentsCount = AllianceIDHolder.BusinessProfileRecords.Count;
                    Response.Holder.WalletID = AllianceIDHolder.AllianceIDHolderWallet.ID;
                    Response.Holder.FollowsCount = AllianceIDHolder.SocialProfile.Follows.Count;
                    Response.Holder.FollowersCount = AllianceIDHolder.SocialProfile.Followers.Count;
                    Response.Holder.CurrencyISO = AllianceIDHolder.AllianceIDHolderCart.Currency.ISOCode ?? "USD";
                    Response.Holder.CurrencyID = AllianceIDHolder.AllianceIDHolderCart.Currency.ID ?? "USD.USA";
                    Response.Holder.CountryFlag = AllianceIDHolder.Country.CountryFlagUrl;
                    Response.Holder.CurrencyExchange = ExchangeRates.Rates.First(c=>c.Key == (AllianceIDHolder.AllianceIDHolderCart.Currency.ISOCode ?? "USD")).Value;
                }

                if (Business != null)
                {
                    // Get Exchange Rate for
                    Response.Tenant = new Tenant
                    {
                        TaxID = Business.TaxID,
                        ID = Business.ID,
                        DUNS = Business.DUNS,
                        Industry = Business.BusinessIndustry.ID,
                        Segment = Business.BusinessSegment.Name,
                        Slogan = Business.Slogan,
                        About = Business.CorporateBoilerplate,
                        WebURL = Business.WebURL,
                        YouTubeURL = Business.YouTubeURL,
                        InstagramURL = Business.InstagramURL,
                        Email = Business.BusinessEmail,
                        FacebookURL = Business.FacebookPageURL,
                        LinkedInURL = Business.LinkedInPageURL,
                        GitHubURL = Business.GitHubOrganizationURL,
                        SocialProfileID = Business.BusinessSocialProfile.ID,
                        WalletID = Business.BusinessWallet.ID,
                        IsBuinessPartner = (Business.BusinessPartnerProfile == null),
                        IsA4StartupsPartner = (Business.StartupsPartnerProfile == null),
                        Name = Business.BusinessName,
                        TwitterURL = AllianceIDHolder.TwitterURL,
                        CartID = Business.BusinessCart.ID,
                        CountryName = Business.Country.Name,
                        LegalName = Business.BusinessLegalName,
                        CountryID = Business.Country.ISOAlpha3,
                        AvatarURL = Business.BusinessAvatarURL,
                        CoverURL = Business.BusinessProfileCoverURL,
                        CurrencyID = Business.BusinessCart.CurrencyID,
                        Country_Flg_URL = Business.Country.CountryFlagUrl,
                        CurrencyName = Business.BusinessCart.Currency.Name,
                        CurrencyISO = Business.BusinessCart.Currency.ISOCode,
                        CurrencySymbol = Business.BusinessCart.Currency.Symbol,
                        FollowsCount = Business.BusinessSocialProfile.Follows.Count,
                        FollowersCount = Business.BusinessSocialProfile.Followers.Count,
                        CurrencyCountryName = Business.BusinessCart.Currency.Country.Name,
                        CurrencyCountryID = Business.BusinessCart.Currency.Country.ISOAlpha3,
                        CurrencyExchangeRateTimestamp = Settings.ExchangeRatesUpdatedTimestamp,
                        CurrencyCountryFlagURL = Business.BusinessCart.Currency.Country.CountryFlagUrl,
                        EnrollmentID = AllianceIDHolder.BusinessProfileRecords.First(c => c.BusinessID == TenantID)?.ID,
                        CurrencyExchangeRate = ExchangeRates.Rates.First(c => c.Key == Business.BusinessCart.Currency.ISOCode).Value,
                        B2CFollowID = Business.BusinessSocialProfile.Follows.FirstOrDefault(c => c.FollowedSocialProfileID == AllianceIDHolder.SocialProfile.ID)?.ID,
                        C2BFollowID = Business.BusinessSocialProfile.Followers.FirstOrDefault(c => c.FollowerSocialProfileID == AllianceIDHolder.SocialProfile.ID)?.ID
                    };
                }
            }
            catch (Exception ex)
            {
                Response.Tenant = null;
                Response.Holder = null;
                Response.Application = null;
                Response.Status.Success = false;
                Response.Status.Error = new Error()
                {
                    ID = "E01",
                    Description = $"Something went wrong retrieving current user's information. Are you authenticated and authorized to use this endpoint? Exception: {ex.Message} ",
                };
            }

            return Response;
        }
    }
}
