using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Clients;
using FenixAlliance.ABM.Data.Access.Helpers;
using FenixAlliance.APS.Core.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/* Fenix Alliance Main Web Portal
 * @Author: Daniel Lozano Navas
 * @Copyright: Fenix Alliance Inc.
 * License: https://fenix-alliance.com/Legal/Policies/EULA
*/

namespace FenixAlliance.APS.Core.Controllers
{
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        public ABMContext DataContext { get; }
        public StoreHelpers StoreHelpers { get; }
        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }
        public TenantHelpers TenantHelpers { get; }
        public AccountUsersHelpers AccountUsersHelpers { get; }
        public AccountGraphHelpers AccountGraphHelpers { get; }
        public TenantDataAccessClient BusinessDataAccess { get; }
        public ApiAuthorizationHelpers ApiAuthorizationHelpers { get; }

        public AccountController(ABMContext context, IConfiguration Configuration, IHostEnvironment Environment,
            StoreHelpers StoreHelpers, TenantHelpers TenantHelpers, AccountUsersHelpers AccountUsersHelpers,
            AccountGraphHelpers AccountGraphHelpers, TenantDataAccessClient TenantDataAccessClient, ApiAuthorizationHelpers ApiAuthorizationHelpers)
        {
            this.DataContext = context;
            this.Environment = Environment;
            this.StoreHelpers = StoreHelpers;
            this.TenantHelpers = TenantHelpers;
            this.Configuration = Configuration;
            this.AccountUsersHelpers = AccountUsersHelpers;
            this.AccountGraphHelpers = AccountGraphHelpers;
            this.BusinessDataAccess = TenantDataAccessClient;
            this.ApiAuthorizationHelpers = ApiAuthorizationHelpers;
        }


        [HttpGet]
        public IActionResult SignIn()
        {
            string BackTo = Request.Headers["Referer"].ToString();
            return Challenge(new AuthenticationProperties { RedirectUri = BackTo }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignInAD()
        {
            string BackTo = Request.Headers["Referer"].ToString();
            return Challenge(new AuthenticationProperties { RedirectUri = BackTo }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult ResetPassword(string PolicyAuthenticationProperty, string ResetPasswordPolicyId)
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            properties.Items[PolicyAuthenticationProperty] = ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult EditProfile(string PolicyAuthenticationProperty, string EditProfilePolicyId)
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };

            properties.Items[PolicyAuthenticationProperty] = EditProfilePolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to SignOut page if the user is still authenticated.
                return Redirect("/");
            }
            return View();
        }

    }
}