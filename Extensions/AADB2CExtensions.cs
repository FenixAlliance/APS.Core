
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AADB2CExtensions
    {
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder) => builder.AddAzureAdB2C(_ => { });

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Action<AADB2COptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        public class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AADB2COptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AADB2COptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.UseTokenLifetime = true;
                options.ClientId = _azureOptions.ClientId;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.Authority = $"{_azureOptions.Instance}/{_azureOptions.Domain}/{_azureOptions.SignUpSignInPolicyId}/v2.0";
                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    //OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                    OnRemoteFailure = OnRemoteFailure
                };
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public Task OnRedirectToIdentityProvider(RedirectContext context)
            {
                var defaultPolicy = _azureOptions.DefaultPolicy;
                if (context.Properties.Items.TryGetValue(AADB2COptions.PolicyAuthenticationProperty, out var policy) &&
                    !policy.Equals(defaultPolicy))
                {
                    context.ProtocolMessage.Scope = OpenIdConnectScope.OpenIdProfile;
                    context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
                    context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.ToLower()
                        .Replace($"/{defaultPolicy.ToLower()}/", $"/{policy.ToLower()}/");
                    context.Properties.Items.Remove(AADB2COptions.PolicyAuthenticationProperty);
                }
                return Task.CompletedTask;
            }

            //public async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            //{
            //    // Use MSAL to swap the code for an access token
            //    // Extract the code from the response notification
            //    var code = context.ProtocolMessage.Code;

            //    string signedInUserID = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

            //    IConfidentialClientApplication cca = ConfidentialClientApplicationBuilder.Create(_azureOptions.ClientId)
            //        .WithB2CAuthority(_azureOptions.Authority)
            //        .WithRedirectUri(_azureOptions.RedirectUri)
            //        .WithClientSecret(_azureOptions.ClientSecret)
            //        .Build();

            //    new MSALStaticCache(signedInUserID, context.HttpContext).EnablePersistence(cca.UserTokenCache);

            //    try
            //    {
            //        AuthenticationResult result = await cca.AcquireTokenByAuthorizationCode(_azureOptions.ApiScopes.Split(' '), code)
            //            .ExecuteAsync();

            //        context.HandleCodeRedemption(result.AccessToken, result.IdToken);
            //    }
            //    catch (Exception ex)
            //    {
            //        //TODO: Handle
            //        throw;
            //    }
            //}

            public Task OnRemoteFailure(RemoteFailureContext context)
            {
                context.HandleResponse();
                // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
                // because password reset is not supported by a "sign-up or sign-in policy"
                if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
                {
                    // If the user clicked the reset password link, redirect to the reset password route
                    context.Response.Redirect("/Account/ResetPassword");
                }
                else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
                {
                    context.Response.Redirect("/");
                }
                else
                {
                    context.Response.Redirect("/ID/Account");
                }
                return Task.CompletedTask;
            }
        }
    }
}
