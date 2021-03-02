using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Interfaces.Helpers;
using FenixAlliance.ACL.Configuration.Interfaces;
using FenixAlliance.APS.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FenixAlliance.APS.Core.Helpers
{
    // We could make this a service. In fact, I'm doing it ASAP.
    public class AccountGraphHelpers : IAccountGraphHelpers
    {
        private ABMContext DataContext { get; set; } 
        public IAccountUsersHelpers AccountTools { get; set; }
        public  ISuiteOptions SuiteOptions { get; set; }

        private readonly IConfiguration Configuration;


        public AccountGraphHelpers(ABMContext context, IConfiguration configuration, ISuiteOptions SuiteOptions, IWebHostEnvironment Environment)
        {
            DataContext = context;
            Configuration = configuration;
            this.SuiteOptions = SuiteOptions;
            AccountTools = new AccountUsersHelpers(context);
        }

        public IAADB2CGraphHelpers GetB2CGraphClient()
        {
            // ToDo: Use ISuiteOptions instead
            var ADGraphSettings = Configuration.GetSection("ADGraphSettings");

            return new AADB2CGraphHelpers(ADGraphSettings.GetValue<string>("ClientId"),
                ADGraphSettings.GetValue<string>("ClientSecret"),
                ADGraphSettings.GetValue<string>("Tenant"));
        }

        public Task<bool> IsInSecurityGroup(string RoleName, ClaimsPrincipal User)
        {
            // Get users's GUID.
            var GUID = AccountTools.GetActiveDirectoryGUID(User);
            // Get ADGraph client
            var ADGraphSettings = Configuration.GetSection("ADSecurityGroup");
            var GroupID = ADGraphSettings.GetValue<string>(RoleName);
            AADB2CGraphHelpers client = (AADB2CGraphHelpers)GetB2CGraphClient();
            return IsMemberOf(GUID, GroupID, client);
        }

        public Task<bool> IsInSecurityGroupByGUID(string RoleName, string GUID)
        {
            // Get ADGraph client
            var ADGraphSettings = Configuration.GetSection("ADSecurityGroup");
            var GroupID = ADGraphSettings.GetValue<string>(RoleName);
            AADB2CGraphHelpers client = (AADB2CGraphHelpers)GetB2CGraphClient();
            return IsMemberOf(GUID, GroupID, client);
        }

        public static MemberOf GetMemberOf(string GUID, AADB2CGraphHelpers _client)
        {
            return JsonConvert.DeserializeObject<MemberOf>(_client.GetMemberOf(GUID).Result); ;
        }

        /// <summary>
        /// Asserts if a specific Alliance ID Holder belongs to a particular Security Group.
        /// </summary>
        /// <param name="GUID">The Alliance ID GUID for the consulted holder.</param>
        /// <param name="ADGroupID">The Security group ID to validate against.</param>
        /// <param name="_client"></param>
        /// <returns>True if the user belongs to the Security Group, False otherwise.</returns>
        public static async Task<bool> IsMemberOf(string GUID, string ADGroupID, AADB2CGraphHelpers _client)
        {
            var result = await _client.GetMemberOf(GUID);
            MemberOf formatted = JsonConvert.DeserializeObject<MemberOf>(result);
            foreach (Value v in formatted.Value)
            {
                if (v.Url.ToString().Contains(ADGroupID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
