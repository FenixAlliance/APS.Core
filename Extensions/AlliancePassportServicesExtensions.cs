using FenixAlliance.ACL.Configuration.Interfaces;
using FenixAlliance.ACL.Configuration.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.APS.Core.Extensions
{
    public static class AlliancePassportServicesExtensions
    {
        public static void AddAlliancePassportServices(this IServiceCollection services, IConfiguration Configuration,
            IHostEnvironment Environment, ISuiteOptions Options)
        {

            #region Auth

            if (Options.APS.Enable)
            {

                if (Options?.APS?.AzureADB2C?.DefaultProvider ?? false)
                {
                    if (!Options?.APS?.AzureAd.DefaultProvider ?? false)
                    {
                        // Adds Azure AD B2C Authentication
                        services.AddAuthentication(o =>
                        {
                            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        })
                        .AddAzureAdB2C(options => Configuration.Bind($"APS:{Options.APS.Provider}", options))
                        //.AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options))
                        //.AddCertificate();
                        .AddCookie();

                        services.AddAuthorization();
                    }
                }

                if (Options?.APS?.AzureAd.DefaultProvider ?? false)
                {
                    if (!Options?.APS?.AzureADB2C?.DefaultProvider ?? false)
                    {
                        services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                            .AddAzureAD(options => Configuration.Bind(
                                $"APS:{Options.APS.Provider}",
                                options)).AddCookie();
                    }
                }

            }
            #endregion

            #region GDPR
            if (Options.ABP.Privacy.Enable)
            {
                // Adds Cookies Consent for GDPR Compliance
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
                });
            }
            #endregion
        }
        public static void UseAlliancePassportServices(this IApplicationBuilder app, IConfiguration Configuration, IHostEnvironment Environment, SuiteOptions Options)
        {

            if (Options.APS.Enable)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }

    }
}
