using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        public ABMContext DataContext { get; set; }
        public IAuthService AuthService { get; set; }
        public IStoreService StoreHelpers { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostEnvironment Environment { get; set; }
        public IHolderService HolderService { get; set; }
        public ITenantService TenantService { get; set; }
        public IStorageService StorageService { get; set; }

        public AccountController(ABMContext DataContext, IConfiguration Configuration, IHostEnvironment Environment,
            IStoreService StoreHelpers, ITenantService TenantService, IHolderService HolderService, IAuthService AuthService, IStorageService StorageService)
        {
            this.AuthService = AuthService;
            this.DataContext = DataContext;
            this.Environment = Environment;
            this.StoreHelpers = StoreHelpers;
            this.Configuration = Configuration;
            this.HolderService = HolderService;
            this.TenantService = TenantService;
            this.StorageService = StorageService;
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