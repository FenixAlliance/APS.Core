﻿using System;
using FenixAlliance.ABM.Data;
using FenixAlliance.ACL.Configuration.Enums;
using FenixAlliance.ACL.Configuration.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.APS.Core.Extensions
{
    public static class AlliancePassportServicesExtensions
    {
        public static void AddAlliancePassportServices(this IServiceCollection services, IConfiguration Configuration, IHostEnvironment Environment, ISuiteOptions Options)
        {
            #region Auth

            if (Options.APS.Enable)
            {

                if (Options?.APS?.AuthenticationProvider == AuthenticationProvider.DefaultIdentity)
                {


                    services.AddDefaultIdentity<IdentityUser>(options =>
                        {
                            options.SignIn.RequireConfirmedAccount = true;
                        })
                        .AddEntityFrameworkStores<ABMContext>();

                    services.AddRazorPages();

                    // Adds required services for Default Identity
                    services.Configure<IdentityOptions>(options =>
                    {
                        // Password settings.
                        options = Options?.APS?.DefaultIdentity?.IdentityOptions ?? new IdentityOptions();
                    });

                    services.ConfigureApplicationCookie(options =>
                    {

                        // Cookie settings
                        options.Cookie.HttpOnly = true;

                        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                        options.LoginPath = "/Identity/Account/Login";
                        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                        options.SlidingExpiration = true;

                    });
                    // Adds required services for IdentityServer4

                }

                if (Options?.APS?.AuthenticationProvider == AuthenticationProvider.AzureActiveDirectory)
                {
                    // Adds required Services for AAD B2C
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

                if (Options?.APS?.AuthenticationProvider == AuthenticationProvider.AzureActiveDirectoryB2C)
                {
                    // Adds required Services for AAD B2C
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
                                .AddAzureAdB2C(options =>
                                {
                                    Configuration.Bind($"APS:{Options.APS.Provider}", options);
                                })
                                //.AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options))
                                //.AddCertificate();
                                .AddCookie();

                            services.AddAuthorization();
                        }
                    }
                }

                if (Options?.APS?.AuthenticationProvider == AuthenticationProvider.IdentityServer4)
                {
                    // TODO: Finish implementing IdentityServer4
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
        public static void UseAlliancePassportServices(this IApplicationBuilder app, IConfiguration Configuration, IHostEnvironment Environment, ISuiteOptions Options)
        {

            if (Options.APS.Enable)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }

    }
}
