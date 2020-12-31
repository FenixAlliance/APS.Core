using FenixAlliance.Data;
using FenixAlliance.Data.Access.Helpers;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

/* Fenix Alliance Main Web Portal
 * @Author: Daniel Lozano Navas
 * @Copyright: Fenix Alliance Inc.
 * License: https://fenix-alliance.com/Legal/Policies/EULA
*/

namespace FenixAlliance.Controllers
{
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly ABMContext _context;
        private AccountGraphHelpers AccountGraphTools { get; set; }
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly AADB2COptions _options;
        private readonly IConfiguration _configuration;

        public AccountController(IOptions<AADB2COptions> b2cOptions, ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _options = b2cOptions.Value;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            AccountGraphTools = new AccountGraphHelpers(_context, _configuration);
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
            properties.Items[AADB2COptions.PolicyAuthenticationProperty] = _options.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            properties.Items[AADB2COptions.PolicyAuthenticationProperty] = _options.EditProfilePolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("/");

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