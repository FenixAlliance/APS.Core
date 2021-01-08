using FenixAlliance.ABM.Data;
using FenixAlliance.Data.Access.DataAccess;
using FenixAlliance.Data.Access.Helpers;
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
        public AADB2COptions AADB2COptions { get; }
        public IConfiguration Configuration { get; }
        public BusinessHelpers BusinessHelpers { get; }
        public IHostEnvironment HostEnvironment { get; }
        public AccountUsersHelpers AccountUsersHelpers { get; }
        public AccountGraphHelpers AccountGraphHelpers { get; }
        public BusinessDataAccessClient BusinessDataAccess { get; }
        public BlobStorageDataAccessClient StorageDataAccessClient { get; }

        public AccountController(IOptions<AADB2COptions> b2cOptions, ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            DataContext = context;
            AADB2COptions = b2cOptions.Value;
            Configuration = configuration;
            HostEnvironment = hostingEnvironment;
            StoreHelpers = new StoreHelpers(DataContext);
            BusinessHelpers = new BusinessHelpers(context);
            AccountUsersHelpers = new AccountUsersHelpers(context);
            AccountGraphHelpers = new AccountGraphHelpers(DataContext, Configuration);
            BusinessDataAccess = new BusinessDataAccessClient(DataContext, Configuration, HostEnvironment);
            StorageDataAccessClient = new BlobStorageDataAccessClient();
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
        public IActionResult ResetPassword()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            properties.Items[AADB2COptions.PolicyAuthenticationProperty] = AADB2COptions.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            properties.Items[AADB2COptions.PolicyAuthenticationProperty] = AADB2COptions.EditProfilePolicyId;
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