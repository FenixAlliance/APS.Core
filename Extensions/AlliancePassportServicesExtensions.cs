using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using FenixAlliance.ABM.Data;
using FenixAlliance.ACL.Configuration.Enums;
using FenixAlliance.ACL.Configuration.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

                }

                if (Options?.APS?.AuthenticationProvider == AuthenticationProvider.IdentityServer4)
                {
                    var apsOperationalDataStores = Options?.ABM?.Providers.Where(c =>
                        c.ProviderPurpose == AllianceBusinessModelProviderPurpose.APS_Operational);

                    var apsConfigurationStores = Options?.ABM?.Providers.Where(c =>
                        c.ProviderPurpose == AllianceBusinessModelProviderPurpose.APS_Operational);

                    if (apsOperationalDataStores.Any() && apsConfigurationStores.Any())
                    {

                        var builder = services.AddIdentityServer()
                            // this adds the operational data from DB (codes, tokens, consents)
                            .AddOperationalStore(options =>
                            {

                                options.ConfigureDbContext = builder =>
                                    builder.UseSqlServer(apsOperationalDataStores.First().ConnectionString,
                                        sql => sql.MigrationsAssembly("")
                                            .MigrationsHistoryTable(apsOperationalDataStores?.First()?.MigrationsHistoryTableName ?? "ApsConfigurationMigrationTable",
                                                apsOperationalDataStores?.First()?.MigrationsHistoryTableSchema ?? "ApsConfigurationSchema"));

                                // this enables automatic token cleanup. this is optional.
                                options.EnableTokenCleanup = true;
                                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)

                            }).AddConfigurationStore(options =>
                            {
                                options.ConfigureDbContext = builder =>
                                    builder.UseSqlServer("",
                                        sql => sql.MigrationsAssembly(""));
                            });

                    }

                    services.AddAuthentication("Bearer")
                        .AddJwtBearer("Bearer", options =>
                            {
                                options = Options?.APS?.IdentityServer4.JwtBearerOptions ?? new JwtBearerOptions();
                            });


                    // MVC
                    JwtSecurityTokenHandler.DefaultMapInboundClaims = (bool)Options?.APS?.IdentityServer4.DefaultMapInboundClaims;

                    services.AddAuthentication(options =>
                        {
                            options.DefaultScheme = "Cookies";
                            options.DefaultChallengeScheme = "oidc";
                        })
                        .AddCookie("Cookies")
                        .AddOpenIdConnect("oidc", options =>
                        {
                            options.Authority = "https://localhost:5001";

                            options.ClientId = "mvc";
                            options.ClientSecret = "secret";
                            options.ResponseType = "code";
                            options.SaveTokens = true;
                        });
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
