using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FenixAlliance.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Identity;

namespace FenixAlliance.Passport.Core
{
    public static class AlliancePassportServicesExtensions
    {
        public static void AddAlliancePassportServices(this IServiceCollection services, IConfiguration Configuration,
            IHostEnvironment Environment, PlatformOptions Options)
        {

            #region Auth

            if (Options.Functionalities.AlliancePassportServices.Enable)
            {

                if (Options?.Functionalities?.AlliancePassportServices?.AzureAdb2C?.DefaultProvider ?? false)
                {
                    if (!Options?.Functionalities?.AlliancePassportServices?.AzureAd.DefaultProvider ?? false)
                    {
                        // Adds Azure AD B2C Authentication
                        services.AddAuthentication(o =>
                        {
                            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        })
                        .AddAzureAdB2C(options =>
                            Configuration.Bind(
                                $"Functionalities:AlliancePassportServices:{Options.Functionalities.AlliancePassportServices.Provider}",
                                options))
                        //.AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options))
                        //.AddCertificate();
                        .AddCookie();

                        services.AddAuthorization();
                    }
                }

                if (Options?.Functionalities?.AlliancePassportServices?.AzureAd.DefaultProvider ?? false)
                {
                    if (!Options?.Functionalities?.AlliancePassportServices?.AzureAdb2C?.DefaultProvider ?? false)
                    {
                        services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                            .AddAzureAD(options => Configuration.Bind(
                                $"Functionalities:AlliancePassportServices:{Options.Functionalities.AlliancePassportServices.Provider}",
                                options)).AddCookie();
                    }
                }

            }
            #endregion

            #region GDPR
            if (Options.Functionalities.Gdpr.Enable)
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
        public static void UseAlliancePassportServices(this IApplicationBuilder app, IConfiguration Configuration, IHostEnvironment Environment, PlatformOptions Options)
        {

            if (Options.Functionalities.AlliancePassportServices.Enable)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }

    }
}
